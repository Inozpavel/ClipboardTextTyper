using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfClipboardTextTyper
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static Settings userSettings;
        private bool _isAnimanting = false;

        private readonly DoubleAnimation _showHalfTimeInput = new
            DoubleAnimation(0, 0.6, new Duration(new TimeSpan(0, 0, 0, 0, 600)));

        private readonly DoubleAnimation _showTimeInput = new
            DoubleAnimation(0.6, 1, new Duration(new TimeSpan(0, 0, 0, 0, 300)));

        private readonly DoubleAnimation _hideHalfTimeInput = new
            DoubleAnimation(1, 0.6, new Duration(new TimeSpan(0, 0, 0, 0, 600)));

        private readonly DoubleAnimation _hideTimeInput = new
            DoubleAnimation(0.6, 0, new Duration(new TimeSpan(0, 0, 0, 0, 300)));

        public MainWindow()
        {
            userSettings = Settings.LoadSettings() ?? new Settings();
            int sliderTime = userSettings.DelayTime;
            InitializeComponent();
            DataContext = userSettings;
            Slider.Value = sliderTime;
            _showHalfTimeInput.Completed += new EventHandler((x, y) =>
            TimeInput.BeginAnimation(OpacityProperty, _showTimeInput));

            _hideHalfTimeInput.Completed += new EventHandler((x, y) =>
            TimeInput.BeginAnimation(OpacityProperty, _hideTimeInput));
        }

        private void DragMoveApp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void SliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider.Value = Math.Round(e.NewValue);
            Settings.SaveSetting(userSettings);
        }

        private void TBCaptionMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isAnimanting == false)
            {
                _isAnimanting = true;
                try
                {
                    Task.Run(() =>
                    {
                        string caption = "";
                        Dispatcher.Invoke(() => caption = TBCaption.Text);
                        int count = 1;
                        while (count <= caption.Length)
                        {
                            Dispatcher.Invoke(() => TBCaption.Text = string.Join("", caption.Take(count)));
                            count++;
                            Thread.Sleep(200);
                        }
                        Thread.Sleep(500);
                        _isAnimanting = false;
                    });
                }
                catch
                {
                    TBCaption.Text = "ClipboardTextTyper";
                    _isAnimanting = false;
                }
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            HideTimeInput.IsChecked = !userSettings.ShouldDelayBe;
            ShowTimeInput.IsChecked = userSettings.ShouldDelayBe;
            TimeInput.Opacity = HideTimeInput.IsChecked ? 0 : 1;
            TextTyper.KeysListening.Initialize();
            TextTyper.window = Process.GetProcessesByName("ClipboardTextTyper")[0];
            DoubleAnimation showHalfOpacity = new DoubleAnimation(0, 0.2, new Duration(new TimeSpan(0, 0, 0, 1)));
            showHalfOpacity.Completed += (x, y) =>
            {
                BeginAnimation(Window.OpacityProperty, new DoubleAnimation(0.2, 1, new Duration(new TimeSpan(0, 0, 0, 2))));
            };
            BeginAnimation(Window.OpacityProperty, showHalfOpacity);
        }


        private void ShowTimeInputChecked(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).IsChecked ?? false)
            {
                TimeInput.BeginAnimation(OpacityProperty, _showHalfTimeInput);
                MainWindow.userSettings.ShouldDelayBe = true;
            }
            else
            {
                TimeInput.BeginAnimation(OpacityProperty, _hideHalfTimeInput);
                MainWindow.userSettings.ShouldDelayBe = false;
            }

            Settings.SaveSetting(userSettings);
        }

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.SaveSetting(userSettings);
        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeApp(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            Settings.SaveSetting(userSettings);
        }
    }
}
