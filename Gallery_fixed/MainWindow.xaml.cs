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

namespace Gallery
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool logged = false;

        public MainWindow()
        {
            InitializeComponent();
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

        private void btnSearch_Click(object sender, RoutedEventArgs e) // метод нужно отладить
        {
            string url = @"http://pikabu.ru/story/fotografu_na_zametku_2743001";
            WebClient wc = new WebClient();
            wc.Proxy = new WebProxy("10.3.0.3", 3128);
            wc.Proxy.Credentials = new NetworkCredential("inet", "netnetnet");

            try
            {
                wc.DownloadFile(url, "1.html");
            }
            catch
            {
                MessageBox.Show("Ошибка при попытке парсинга страницы\n" + url);
            }

            HtmlDocument doc = new HtmlDocument();
            doc.Load("1.html", Encoding.UTF8);
            
            HtmlNodeCollection images = doc.DocumentNode.SelectNodes("//img");
            int errors = 0;

            foreach (HtmlNode n in images)
            {
                try
                {
                    string imgPath = n.Attributes["src"].Value;
                    if (!imgPath.Contains("//")) // Если урл не содержит двух слешей, значит адрес изображения относительный и нужно вычленить из url домен сайта
                    {
                        int c = 0;
                        for (int i=0; i<url.Length; i++)
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
                    
                    wc.DownloadFile(imgPath, @"..\..\Images\" + n.Attributes["src"].Value.Substring(n.Attributes["src"].Value.LastIndexOf('/')));
                }
                catch 
                {
                    errors++;
                }
            }
            MessageBox.Show(errors.ToString());
        }
    }
}
