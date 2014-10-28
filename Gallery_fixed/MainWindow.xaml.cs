﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using HtmlAgilityPack;
using System.Net;
using System.Threading;
using System.IO;
using Gallery_fixed;
using System.Text.RegularExpressions;

namespace Gallery
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Window window;
        public static WrapPanel areaRef;
        public static ScrollViewer gallRef;

        public bool logged = false;
        private static int unique = 0;
        private object uniqueLocker = new object();
        private static int threadJobs = 0;
        private object threadJobLocker = new object();
        private static int errors = 0;
        private object errorsLocker = new object();
        private RotateTransform rt = null;
        private double rotateAngle = 0;

        public MainWindow()
        {
            InitializeComponent();
            window = this;
            CreateGallery();
            areaRef = area;
            gallRef = GalleryContainer;
            this.WindowState = System.Windows.WindowState.Maximized;
        }

        private void Label_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            Login.Visibility = System.Windows.Visibility.Visible;
            Register.Visibility = System.Windows.Visibility.Visible;
            RegisterLabel.Visibility = System.Windows.Visibility.Collapsed;
            RegisterAction.Visibility = System.Windows.Visibility.Visible;
        }

        private void Login_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RegisterLabel.Visibility = System.Windows.Visibility.Visible;
            RegisterAction.Visibility = System.Windows.Visibility.Collapsed;
            Register.Visibility = System.Windows.Visibility.Hidden;
            Login.Visibility = System.Windows.Visibility.Visible;
            logged = true;
        }

        private List<string> GetLinkList()
        {
            try
            {
                List<string> tmp = new List<string>(tbSearch.Text.Split(new string[] { "http" }, StringSplitOptions.RemoveEmptyEntries));
                List<string> links = new List<string>();
                foreach (string str in tmp)
                {
                    string s = "http" + str;
                    links.Add(@s);
                }
                return links;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
                return new List<string>();
            }
        }

        private void StartParsing(List<string> urls)
        {
            foreach (string s in urls)
            {
                ThreadPool.QueueUserWorkItem(ParseUrl, s);
                lock (threadJobLocker)
                {
                    threadJobs++;
                }
            }
        }

        private void ParseUrl(object parseUrl)
        {
            string url = parseUrl.ToString();
            string htmlName = "";
            lock (uniqueLocker)
            {
                if (!Directory.Exists(@"..\..\Parse\"))
                    Directory.CreateDirectory(@"..\..\Parse\");
                htmlName = @"..\..\Parse\" + (unique++).ToString() + ".html";
            }
            WebClient wc = new WebClient();
            wc.Proxy = new WebProxy("10.3.0.3", 3128);
            wc.Proxy.Credentials = new NetworkCredential("inet", "netnetnet");


            try
            {
                wc.DownloadFile(url, htmlName);
                HtmlDocument doc = new HtmlDocument();
                doc.Load(htmlName, Encoding.UTF8);
                HtmlNodeCollection images = doc.DocumentNode.SelectNodes("//img");

                foreach (HtmlNode n in images)
                {
                    try
                    {
                        char[] DelChars = { '%', '_', '-', };
                        string imgPath = n.Attributes["src"].Value;
                        if (imgPath.IndexOf(".jpg") == -1 && imgPath.IndexOf(".JPG") == -1 &&
                            imgPath.IndexOf(".PNG") == -1 && imgPath.IndexOf(".png") == -1 &&
                            imgPath.IndexOf(".GIF") == -1 && imgPath.IndexOf(".gif") == -1)
                            continue;
                        if (!imgPath.Contains("//")) // Если урл не содержит двух слешей, значит адрес изображения относительный и нужно вычленить из url домен сайта
                        {
                            int c = 0;
                            for (int i = 0; i < url.Length; i++)
                            {
                                if (url[i] == '/')
                                    c++;
                                if (c == 3)
                                {
                                    imgPath = url.Substring(0, i + 1) + imgPath;
                                    break;
                                }
                            }
                        }

                        string imgName = @"..\..\Images\" + n.Attributes["src"].Value.Substring(n.Attributes["src"].Value.LastIndexOf('/') + 1);

                        if (imgName.IndexOf('&') != -1)
                            imgName = imgName.Substring(0, imgName.IndexOf('&')); // обираем все лишнее из адреса изображения

                        imgName = Regex.Replace(imgName, @"[-%_^]", "", RegexOptions.Compiled);
                        wc.DownloadFile(imgPath, imgName);

                        FileInfo fi = new FileInfo(imgName);
                        DBHelper.AddImage(fi.Name, imgPath, 1, GetTagName(imgPath));
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show("Упало во время парсинга\n" + ex.Message + "\n" + ex.StackTrace);

                        lock (errorsLocker)
                        {
                            errors++;
                        }
                    }
                }
            }
            catch
            {
                // нужно заменить выбрасывание окон логгированием (записью ошибок в файл)
                MessageBox.Show("Ошибка при попытке парсинга страницы\n" + url);
            }
            finally
            {
                lock (threadJobLocker)
                {
                    threadJobs--;
                    if (threadJobs == 0)
                    {
                        int tmp = 0;
                        lock (errorsLocker)
                        {
                            tmp = errors;
                            errors = 0;
                        }
                        MainWindow.window.Dispatcher.Invoke(new Action(delegate() { Cursor = Cursors.Arrow; }));
                        MainWindow.window.Dispatcher.Invoke(new Action(delegate() { CreateGallery3(); }));
                    }
                }
            }
        }

        private string GetTagName(string url)
        {
            string tagName = url.Substring(url.IndexOf("//") + 2);
            return tagName.Substring(0, tagName.IndexOf("/"));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // тестируем потоковые методы
                List<string> s = new List<string>();
                s = GetLinkList();
                StartParsing(s);
                Cursor = Cursors.Wait;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Очищаем директорию "..\..\Parse\" от html файлов, оставшихся после парсинга
            try
            {
                DirectoryInfo di = new DirectoryInfo(@"..\..\Parse\");
                FileInfo[] fi = di.GetFiles();
                for (int i = 0; i < fi.Length; i++)
                    fi[i].Delete();
            }
            catch { }
        }

        private void Label_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
        }

        void tag_MouseLeave(object sender, MouseEventArgs e)
        {
        }

        void tag_MouseEnter(object sender, MouseEventArgs e)
        {
        }



        public void CreateGallery()
        {
            area.Children.Clear();
            DirectoryInfo di = new DirectoryInfo("../../Images/");
            FileInfo[] images = di.GetFiles();
            Random r = new Random();
            double tmp = r.Next(200, 301);

            foreach (FileInfo f in images)
            {
                ImageSourceConverter imgConv = new ImageSourceConverter();
                ImageSource imageSource = (ImageSource)imgConv.ConvertFromString(f.FullName);
                Image img = new Image();
                img.Height = 250;
                img.Source = imageSource;
                img.MouseDown += img_MouseDown;
                img.MouseEnter += img_MouseEnter;
                img.MouseLeave += img_MouseLeave;
                Border border = new Border();
                border.BorderBrush = new SolidColorBrush(Colors.Black);
                border.BorderThickness = new Thickness(2);
                border.Margin = new Thickness(3);
                img.Cursor = Cursors.SizeAll;
                border.Child = img;
                area.Children.Add(border);
            }
        }

        void img_MouseLeave(object sender, MouseEventArgs e)
        {
            Border b = (Border)(sender as Image).Parent;
            b.BorderThickness = new Thickness(2);
            b.BorderBrush = new SolidColorBrush(Colors.Black); 
            b.BorderThickness = new Thickness(1);
            b.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCCCCC"));
        }

        void img_MouseEnter(object sender, MouseEventArgs e)
        {
            Border b = (Border)(sender as Image).Parent;
            b.BorderThickness = new Thickness(2);
            b.BorderBrush = new SolidColorBrush(Colors.DarkGoldenrod);
        }

        void img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ImageView mv = new ImageView(sender as Image);
            SolidColorBrush bgBrush = new SolidColorBrush(Colors.Black);
            bgBrush.Opacity = 0.3;
            GalleryContainer.Background = bgBrush;
            foreach (Border img in area.Children)
            {
                img.Opacity = 0.1;
            }
            mv.Show();
        }

        private void Label_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Label).Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B5287"));
            (sender as Label).Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAFAFA"));
            (sender as Label).Cursor = Cursors.Hand;
        }

        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Label).Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAFAFA"));
            (sender as Label).Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B5287"));
            (sender as Label).Cursor = Cursors.Arrow;
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            List<string> tags = DBHelper.GetTags();
            foreach (var v in tags)
            {
                Label l = new Label();
                l.FontFamily = new FontFamily("Calibri");
                l.Content = v.ToString();
                l.Tag = v.ToString();
                l.MouseEnter += Label_MouseEnter;
                l.MouseLeave += Label_MouseLeave;
                l.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B5287"));
                l.FontSize = 14;
                l.MouseDown += l_MouseDown;
                Tags.Children.Add(l);
            }
        }

        void l_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                area.Children.Clear();
                Random r = new Random();
                double tmp = r.Next(150, 250);
                List<FileInfo> images = new List<FileInfo>();
                List<string> imgPathes = DBHelper.GetImagesPathes((sender as Label).Content.ToString());
                if ((sender as Label).Content.ToString().Equals("случайные"))
                {
                    if (imgPathes.Count != 0)
                    {
                        int count = 0;
                        int attempts = 0;
                        List<int> ids = new List<int>();
                        while (true)
                        {
                            int i = r.Next(imgPathes.Count);
                            if (!ids.Contains(i))
                            {
                                images.Add(new FileInfo("../../Images/" + imgPathes[i]));
                                attempts = 0;
                                count++;
                            }
                            else
                                attempts++;
                            if (count == 21 || attempts == 10)
                                break;
                        }
                    }
                }                
                else
                {
                    foreach (string s in imgPathes)
                        images.Add(new FileInfo("../../Images/" + s));
                }

                foreach (FileInfo f in images)
                {
                    ImageSourceConverter imgConv = new ImageSourceConverter();
                    ImageSource imageSource = (ImageSource)imgConv.ConvertFromString(f.FullName);
                    Image img = new Image();
                    img.Height = tmp;
                    img.Source = imageSource;
                    img.MouseDown += img_MouseDown;
                    img.MouseEnter += imgRotate_MouseEnter;
                    img.MouseLeave += imgRotate_MouseLeave;
                    Border border = new Border();
                    border.BorderBrush = new SolidColorBrush(Colors.Black);
                    border.BorderThickness = new Thickness(2);
                    border.Margin = new Thickness(12);
                    RotateTransform rt = new RotateTransform(r.Next(-5, 5));
                    border.RenderTransformOrigin = new Point(0.5, 0.5);
                    border.RenderTransform = rt;
                    img.Cursor = Cursors.SizeAll;
                    border.Child = img;
                    area.Children.Add(border);
                }
                
                if (images.Count == 0)
                {
                    Label l = new Label();
                    l.FontFamily = new FontFamily("Arial");
                    l.Content = "C таким тегом картинок нет :(";                    
                    l.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B5287"));
                    l.FontSize = 34;
                    area.Children.Add(l);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace);
            }
        }

        public void CreateGallery3()
        {
            area.Children.Clear();
            LinearGradientBrush lgb = new LinearGradientBrush();
            GradientStop gs1 = new GradientStop((Color)ColorConverter.ConvertFromString("#003973"), 0);
            GradientStop gs2 = new GradientStop((Color)ColorConverter.ConvertFromString("#E5E5BE"), 0.80);
            lgb.GradientStops.Add(gs1);
            lgb.GradientStops.Add(gs2);
            area.Background = lgb;

            DirectoryInfo di = new DirectoryInfo("../../Images");
            FileInfo[] images = di.GetFiles();
            Random r = new Random();
            double tmp = r.Next(150, 250);

            foreach (FileInfo f in images)
            {
                ImageSourceConverter imgConv = new ImageSourceConverter();
                ImageSource imageSource = (ImageSource)imgConv.ConvertFromString(f.FullName);
                Image img = new Image();
                img.Height = tmp;
                img.Source = imageSource;
                img.MouseDown += img_MouseDown;
                img.MouseEnter += imgRotate_MouseEnter;
                img.MouseLeave += imgRotate_MouseLeave;
                Border border = new Border();
                border.BorderBrush = new SolidColorBrush(Colors.Black);
                border.BorderThickness = new Thickness(2);
                border.Margin = new Thickness(12);
                RotateTransform rt = new RotateTransform(r.Next(-5, 5));
                border.RenderTransformOrigin = new Point(0.5, 0.5);
                border.RenderTransform = rt;
                img.Cursor = Cursors.SizeAll;
                border.Child = img;
                area.Children.Add(border);
            }
        }

        private void imgRotate_MouseLeave(object sender, MouseEventArgs e)
        {
            ((sender as Image).Parent as Border).BorderBrush = Brushes.Black;
            RotateTransform rotation = new RotateTransform(rt.Angle);
            ((sender as Image).Parent as Border).RenderTransform = rotation;
        }

        private void imgRotate_MouseEnter(object sender, MouseEventArgs e)
        {
            ((sender as Image).Parent as Border).BorderBrush = Brushes.DarkGoldenrod;
            RotateTransform rotation = ((sender as Image).Parent as Border).RenderTransform as RotateTransform;
            if (rotation != null)
            {
                rotateAngle = rotation.Angle;
            }
            rt = new RotateTransform(rotateAngle);
            ((sender as Image).Parent as Border).RenderTransform = rt;
            DoubleAnimation da = new DoubleAnimation();
            da.From = rotateAngle;
            if (rotateAngle < 0)
                da.To = 10;
            else if (rotateAngle >= 0)
                da.To = -10;
            da.AutoReverse = true;
            da.RepeatBehavior = RepeatBehavior.Forever;
            da.SpeedRatio = 2;
            rt.BeginAnimation(RotateTransform.AngleProperty, da);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            area.Children.Clear();
            CreateGallery();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            area.Children.Clear();
            CreateGallery3();
        }
    }
}
