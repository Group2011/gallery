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
            if (tbName.Text.Length < 3 || tbPass.Password.Length < 3)
                MessageBox.Show("Имя или пароль должны быть длинее 3-х символов");
            else
            {
                if (!DBHelper.CheckUser(tbName.Text))
                {
                    DBHelper.AddUser(tbName.Text, tbPass.Password);
                    Gallery.MainWindow.idUser = DBHelper.GetUserID(tbName.Text);
                    Gallery.MainWindow.search.Visibility = System.Windows.Visibility.Visible;
                    Gallery.MainWindow.MenuItemsHide();
                    this.Close();
                }
                else
                {
                    if (DBHelper.CheckUser(tbName.Text, tbPass.Password))
                    {
                        Gallery.MainWindow.search.Visibility = System.Windows.Visibility.Visible;
                        Gallery.MainWindow.MenuItemsHide();
                        Gallery.MainWindow.idUser = DBHelper.GetUserID(tbName.Text);
                        this.Close();
                    }
                    else
                        MessageBox.Show("А пароль-то неправильный! \n(Либо юзер с таким именем уже существует)", "Ошибко");
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
