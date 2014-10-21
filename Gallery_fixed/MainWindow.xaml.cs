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

namespace Gallery
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Window window;
        public bool logged = false;
        private static int unique = 0;
        private object uniqueLocker = new object();
        private static int threadJobs = 0;
        private object threadJobLocker = new object();
        private static int errors = 0;
        private object errorsLocker = new object();
<<<<<<< HEAD
        private static int user_id = 1; // пока по дефолту

=======
        public static List<ImageSource> sources = new List<ImageSource>();
        public static int iterator = 0;
>>>>>>> 2d201b58e0a4e9faaa2e50479c62b7e842cc8805

        public MainWindow()
        {
            InitializeComponent();
            window = this;
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
            List<string> tmp = new List<string>(tbSearch.Text.Split(new string[] { "http" }, StringSplitOptions.RemoveEmptyEntries));
            List<string> links = new List<string>();
            foreach (string str in tmp)
            {
                links.Add("http" + str);
            }
            return links;
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

        private string GetFileName(string path)
        {
            if (File.Exists(path))
            {
                FileInfo fi = new FileInfo(path);
                return fi.Name;
            }
            else
                return "--Error";
        }

        private string GetFilePath(string path)
        {
            if (File.Exists(path))
            {
                FileInfo fi = new FileInfo(path);
                return fi.FullName;
            }
            else
                return "--Error";
        }

        private void ParseUrl(object parseUrl)
        {
            string url = parseUrl.ToString();
            string htmlName = "";
            lock (uniqueLocker)
            {
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
                        string imgPath = n.Attributes["src"].Value;
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
                        wc.DownloadFile(imgPath, imgName);
<<<<<<< HEAD
                        DBHelper.AddImage(GetFileName(imgName), GetFilePath(imgName), user_id, GetTagName(imgPath));
=======
                        
>>>>>>> 2d201b58e0a4e9faaa2e50479c62b7e842cc8805
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message + ex.StackTrace);
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
                        MainWindow.window.Dispatcher.Invoke(new Action(delegate() { MessageBox.Show("Поиск завершен!\nОшибок: " + tmp.ToString()); }));
                    }
                }
            }
        }

        /// <summary>
        /// Формирует имя тега url адреса (вычленяет домен без протокола)
        /// </summary>
        /// <param name="url">Адрес к изображению</param>
        /// <returns>Имя тега</returns>
        public string GetTagName(string url)
        {
            if (url == null)
                return "";
            if (url.IndexOf("//") != -1)
                url = url.Substring(url.IndexOf("//") + 2);
            if (url.IndexOf("/") != -1)
                url = url.Substring(0, url.IndexOf("/"));
            return url;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            List<string> s = new List<string>();
            s.Add(@"http://www.kristianhammerstad.com/");
            s.Add(@"http://erikjohanssonphoto.com/work/imagecats/personal/");
            StartParsing(s);
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
            if (tagcloud.Visibility == System.Windows.Visibility.Collapsed)
                tagcloud.Visibility = System.Windows.Visibility.Visible;
            else
            {
                tagcloud.Visibility = System.Windows.Visibility.Collapsed;
                tagcloud.Children.Clear();
            }
            Random rnd = new Random();
            Label l1 = new Label();
            
            //это цикл-заглушка. заменить на foreach по тэгам из базы (или что-то более удобное)
            int xpos = 0; 
            for (int i = 0; i < 10; i++)
            {
                l1 = new Label();
                l1.Style = (Style)l1.TryFindResource("TagDefaultStyle");
                l1.MouseEnter += tag_MouseEnter;
                l1.MouseLeave += tag_MouseLeave;
                l1.Content = "Tag"; //тэг должен браться из базы
                xpos += (int)l1.Width;
                Canvas.SetTop(l1, rnd.Next(30));
                Canvas.SetLeft(l1, rnd.Next((int)window.ActualWidth - (int)VisualTreeHelper.GetOffset(tagbtn).X - 80)); //вычисляем ширину канваса в зависимости от текущих размеров окна
                tagcloud.Children.Add(l1);
            }
        }

        void tag_MouseLeave(object sender, MouseEventArgs e)
        {
            Label l1 = (Label)sender;
            l1.Style = (Style)l1.TryFindResource("TagDefaultStyle");
        }

        void tag_MouseEnter(object sender, MouseEventArgs e)
        {
            Label l1 = (Label)sender;
            l1.Style = (Style)l1.TryFindResource("TagMouseEntertStyle");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (iterator + 1 >= sources.Count)
                iterator = 0;
            else
                iterator++;

            largeIMG.Source = sources[iterator];
            mirrorLargeIMG.Source = sources[iterator];

            if (iterator + 1 >= sources.Count)
            {
                mediumRightIMG.Source = sources[0];
                mirrorMediumRightIMG.Source = sources[0];
            }
            else
            {
                mediumRightIMG.Source = sources[iterator + 1];
                mirrorMediumRightIMG.Source = sources[iterator + 1];
            }

            if (iterator + 2 >= sources.Count)
            {
                smallRightImg.Source = sources[1];
                mirrorSmallRightIMG.Source = sources[1];
            }
            else
            {
                smallRightImg.Source = sources[iterator + 2];
                mirrorSmallRightIMG.Source = sources[iterator + 2];
            }

            if (iterator - 1 < 0)
            {
                mediumLeftIMG.Source = sources[sources.Count - 1];
                mirrorMediumLeftIMG.Source = sources[sources.Count - 1];
            }
            else
            {
                mediumLeftIMG.Source = sources[iterator - 1];
                mirrorMediumLeftIMG.Source = sources[iterator - 1];
            }

            if (iterator - 2 < 0)
            {
                smallLeftIMG.Source = sources[sources.Count - 2];
                mirrorSmallLeftIMG.Source = sources[sources.Count - 2];
            }
            else
            {
                smallLeftIMG.Source = sources[iterator - 2];
                mirrorSmallLeftIMG.Source = sources[iterator - 2];
            }

            
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (iterator - 1 < 0)
                iterator = sources.Count - 1;
            else
                iterator--;

            largeIMG.Source = sources[iterator];
            mirrorLargeIMG.Source = sources[iterator];

            if (iterator + 1 >= sources.Count)
            {
                mediumRightIMG.Source = sources[0];
                mirrorMediumRightIMG.Source = sources[0];
            }
            else
            {
                mediumRightIMG.Source = sources[iterator + 1];
                mirrorMediumRightIMG.Source = sources[iterator + 1];
            }

            if (iterator + 2 >= sources.Count)
            {
                smallRightImg.Source = sources[1];
                mirrorSmallRightIMG.Source = sources[1];
            }
            else
            {
                smallRightImg.Source = sources[iterator + 2];
                mirrorSmallRightIMG.Source = sources[iterator + 2];
            }

            if (iterator - 1 < 0)
            {
                mediumLeftIMG.Source = sources[sources.Count - 1];
                mirrorMediumLeftIMG.Source = sources[sources.Count - 1];
            }
            else
            {
                mediumLeftIMG.Source = sources[iterator - 1];
                mirrorMediumLeftIMG.Source = sources[iterator - 1];
            }

            if (iterator - 2 < 0)
            {
                smallLeftIMG.Source = sources[sources.Count - 2];
                mirrorSmallLeftIMG.Source = sources[sources.Count - 2];
            }
            else
            {
                smallLeftIMG.Source = sources[iterator - 2];
                mirrorSmallLeftIMG.Source = sources[iterator - 2];
            }

            
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            DirectoryInfo di = new DirectoryInfo("../../Images");
            FileInfo[] images = di.GetFiles();
            Random r = new Random();
            double tmp = r.Next(200, 301);
            foreach (FileInfo f in images)
            {
                ImageSourceConverter imgConv = new ImageSourceConverter();
                sources.Add((ImageSource)imgConv.ConvertFromString(f.FullName));
            }

            largeIMG.Source = sources[iterator];
            mirrorLargeIMG.Source = sources[iterator];

            mediumRightIMG.Source = sources[iterator + 1];
            mirrorMediumRightIMG.Source = sources[iterator + 1];

            smallRightImg.Source = sources[iterator + 2];
            mirrorSmallRightIMG.Source = sources[iterator + 2];

            if (iterator - 1 < 0)
            {
                mediumLeftIMG.Source = sources[sources.Count - 1];
                mirrorMediumLeftIMG.Source = sources[sources.Count - 1];
            }
            else
            {
                mediumLeftIMG.Source = sources[iterator - 1];
                mirrorMediumLeftIMG.Source = sources[iterator - 1];
            }

            if (iterator - 2 < 0)
            {
                smallLeftIMG.Source = sources[sources.Count - 2];
                mirrorSmallLeftIMG.Source = sources[sources.Count - 2];
            }
            else
            {
                smallLeftIMG.Source = sources[iterator - 2];
                mirrorSmallLeftIMG.Source = sources[iterator - 2];
            }
        }
    }
}
