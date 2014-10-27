using System;
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


        public MainWindow()
        {
            InitializeComponent();
            window = this;
            CreateGallery();
            areaRef = area;
            gallRef = GalleryContainer;
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

                        string imgName = @"..\..\Images\" + n.Attributes["src"].Value.Substring(n.Attributes["src"].Value.LastIndexOf('/'));
                        if (imgName.IndexOf('&') != -1)
                            imgName = imgName.Substring(0, imgName.IndexOf('&')); // обираем все лишнее из адреса изображения

                        wc.DownloadFile(imgPath, Regex.Replace(imgName, @"[-%_^]", "", RegexOptions.Compiled));

                    }
                    catch
                    {
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
                        MainWindow.window.Dispatcher.Invoke(new Action(delegate() { CreateGallery(); }));
                    }
                }
            }
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
            /*Label l1 = (Label)sender;
            l1.Style = (Style)l1.TryFindResource("TagDefaultStyle");*/
        }

        void tag_MouseEnter(object sender, MouseEventArgs e)
        {
            /*Label l1 = (Label)sender;
            l1.Style = (Style)l1.TryFindResource("TagMouseEntertStyle");*/
        }
        


        public void CreateGallery()
        {
            DirectoryInfo di = new DirectoryInfo("../../Images");
            FileInfo[] images = di.GetFiles();
            Random r = new Random();
            double tmp = r.Next(200, 301);

            foreach (FileInfo f in images)
            {
                ImageSourceConverter imgConv = new ImageSourceConverter();
                ImageSource imageSource = (ImageSource)imgConv.ConvertFromString(f.FullName);
                Image img = new Image();
                img.Height = tmp;
                img.Source = imageSource;
                img.MouseDown += img_MouseDown;
                img.MouseEnter += img_MouseEnter;
                img.MouseLeave += img_MouseLeave;
                Border border = new Border();
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCCCCC"));
                border.BorderThickness = new Thickness(1);
                border.Margin = new Thickness(3);
                img.Cursor = Cursors.SizeAll;
                border.Child = img;
                area.Children.Add(border);
            }
        }

        void img_MouseLeave(object sender, MouseEventArgs e)
        {
            Border b = (Border)(sender as Image).Parent;
            b.BorderThickness = new Thickness(1);
            b.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCCCCC")); 
        }

        void img_MouseEnter(object sender, MouseEventArgs e)
        {
            Border b = (Border)(sender as Image).Parent;
            b.BorderThickness = new Thickness(1);
            b.BorderBrush = new SolidColorBrush(Colors.Blue);
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

        private void Window_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
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
                l.FontSize=14;
                Tags.Children.Add(l);
            }
        }
    }
}
