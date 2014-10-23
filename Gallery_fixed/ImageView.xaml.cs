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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Gallery_fixed
{
    /// <summary>
    /// Логика взаимодействия для twind.xaml
    /// </summary>
    public partial class ImageView : Window
    {
        public ImageView(Image img)
        {
            InitializeComponent();

            //создание градиента для фона окна
            LinearGradientBrush lb = new LinearGradientBrush();
            GradientStop gst1 = new GradientStop(Colors.Green, 0);
            GradientStop gst2 = new GradientStop(Colors.White, 0.5);
            GradientStop gst3 = new GradientStop(Colors.Green, 1);
            lb.GradientStops.Add(gst1);
            lb.GradientStops.Add(gst2);
            lb.GradientStops.Add(gst3);
            this.Background = lb;


            Image img2 = new Image();
            img2.Source = img.Source;
            img2.MouseDown += ImageView_MouseDown;
            img2.Stretch = Stretch.None; //значение None позволяет открывать картинки "родного" размера, не растягивая их
            this.Width = img2.Source.Width;
            this.Height = img2.Source.Height;

            MakeAnimation(img2);
            img2.Cursor = Cursors.Hand;

            area.Children.Add(img2);
            this.MouseDown += ImageView_MouseDown;
        }

        public void MakeAnimation(Image img)
        {
            //анимация градиента фона окна
            DoubleAnimation gradient = new DoubleAnimation();
            gradient.Duration = TimeSpan.FromSeconds(0.25);
            gradient.From = 0;
            gradient.To = 1;
            gradient.AutoReverse = true;
            //gradient.RepeatBehavior = RepeatBehavior.Forever;
            (this.Background as LinearGradientBrush).GradientStops[1].BeginAnimation(GradientStop.OffsetProperty, gradient);

            //анимация появления картинки через изменения прозрачности
            DoubleAnimation animationOpacity = new DoubleAnimation();
            animationOpacity.Duration = TimeSpan.FromSeconds(0.5);
            animationOpacity.From = 0;
            animationOpacity.To = 1;
            img.BeginAnimation(Image.OpacityProperty, animationOpacity);

            //анимация появления границ окна
            DoubleAnimation opacityAnBorder = new DoubleAnimation();
            opacityAnBorder.Duration = TimeSpan.FromSeconds(0.5);
            opacityAnBorder.From = 0;
            opacityAnBorder.To = 1;
            b.BeginAnimation(Border.OpacityProperty, opacityAnBorder);


            //еще один вариант появления картинки через изменение высоты, вместо изменения прозрачности
            //DoubleAnimation animationH = new DoubleAnimation();
            //animationH.Duration = TimeSpan.FromSeconds(0.2);
            //animationH.From = 0;
            //animationH.To = this.Height;
            //img.BeginAnimation(Image.HeightProperty,animationH);
        }

        void ImageView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
            SolidColorBrush bgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fafafa"));
            Gallery.MainWindow.gallRef.Background = bgBrush;
            foreach (Border img in Gallery.MainWindow.areaRef.Children)
            {
                img.Opacity = 1;
            }
        }
    }
}
