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
    /// Логика взаимодействия для RegDialog.xaml
    /// </summary>
    public partial class RegDialog : Window
    {
        public RegDialog()
        {
            InitializeComponent();
        }

        private void btConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (tbUsername.Text.Length < 3 || tbPass.Text.Length<3)
                MessageBox.Show("Имя слишком маленькое");
            else
            {
                DBHelper.AddUser(tbUsername.Text, tbPass.Text);
                Gallery.MainWindow.window.Visibility = System.Windows.Visibility.Visible;
                this.Close();
            }
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            Gallery.MainWindow.window.Visibility = System.Windows.Visibility.Visible;
            this.Close();
        }
    }
}
