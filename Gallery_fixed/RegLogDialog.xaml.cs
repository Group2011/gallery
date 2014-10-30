using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Gallery;

namespace Gallery_fixed
{
    /// <summary>
    /// Логика взаимодействия для RegLogDialog.xaml
    /// </summary>
    public partial class RegLogDialog : Window
    {
        string type;

        public RegLogDialog(string type)
        {
            InitializeComponent();
            this.type = type;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (tbName.Text.Length < 3 || tbPass.Text.Length < 3)
                MessageBox.Show("Имя или пароль должны быть длинее 3-х символов");
            else
            {
                if (type.Equals("reg"))
                    DBHelper.AddUser(tbName.Text, tbPass.Text);
                else if (type.Equals("log"))
                {
                    DBHelper.UserEnter(tbName.Text, tbPass.Text);
                }
                this.Close();
                Gallery.MainWindow.window.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.Close();
            Gallery.MainWindow.window.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
