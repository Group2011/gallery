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

        /*private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (logged)
            {
                tbSearch.Visibility = System.Windows.Visibility.Visible;
            }
        }*/

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
                        MainWindow.window.Dispatcher.Invoke(new Action(delegate() { MessageBox.Show("Поиск завершен!\nОшибок: " + tmp.ToString()); }));
                    }
                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // тестируем потоковые методы
            List<string> s = new List<string>();
            s.Add(@"http://mostua.com/");
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
    }
}
