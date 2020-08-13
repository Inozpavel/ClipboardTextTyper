using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfClipboardTextTyper
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal bool isAnimanting = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CloseApp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Close();
        }

        private void MinimizeApp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void DragMoveApp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            (sender as Slider).Value = Math.Round(e.NewValue);
        }

        private void TBCaption_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!isAnimanting)
            {
                isAnimanting = true;
                Task.Run(() =>
                {
                    string caption = "";
                    Dispatcher.Invoke(() => caption = TBCaption.Text);
                    int i = 1;
                    while (i <= caption.Length)
                    {
                        Dispatcher.Invoke(() => TBCaption.Text = string.Join("", caption.Take(i).ToArray()));
                        i++;
                        Thread.Sleep(200);
                    }
                    Thread.Sleep(500);
                    isAnimanting = false;
                });
            }
        }
    }
}
