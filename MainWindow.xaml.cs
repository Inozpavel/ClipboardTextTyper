using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace WpfClipboardTextTyper
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {   internal static Settings userSettings;
        internal bool isAnimanting = false;
        internal DoubleAnimation showHalfTimeInput = new DoubleAnimation(0, 0.6, new Duration(new TimeSpan(0, 0, 0, 0, 600)));
        internal DoubleAnimation showTimeInput = new DoubleAnimation(0.6, 1, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
        internal DoubleAnimation hideHalfTimeInput = new DoubleAnimation(1, 0.6, new Duration(new TimeSpan(0, 0, 0, 0, 600)));
        internal DoubleAnimation hideTimeInput = new DoubleAnimation(0.6, 0, new Duration(new TimeSpan(0, 0, 0, 0, 300)));

        public MainWindow()
        {
            userSettings = Settings.LoadSettings() ?? new Settings();
            int sliderTime = userSettings.DelayTime;
            InitializeComponent();
            DataContext = userSettings;
            Slider.Value = sliderTime;
            showHalfTimeInput.Completed += new EventHandler((x, y) => TimeInput.BeginAnimation(OpacityProperty, showTimeInput));
            hideHalfTimeInput.Completed += new EventHandler((x, y) => TimeInput.BeginAnimation(OpacityProperty, hideTimeInput));
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

        private void SliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider.Value = Math.Round(e.NewValue);
            Settings.SaveSetting(userSettings);
        }

        private void TBCaptionMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!isAnimanting)
            {
                isAnimanting = true;
                try
                {
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
                catch
                {
                    TBCaption.Text = "ClipboardTextTyper";
                    isAnimanting = false;
                }
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            HideTimeInput.IsChecked = !userSettings.ShouldDelayBe;
            ShowTimeInput.IsChecked = userSettings.ShouldDelayBe;
            TimeInput.Opacity = HideTimeInput.IsChecked ?? true ? 0 : 1;
            TextTyper.HotkeysListening.Start();
            TextTyper.ListenShift.Start();
        }

        private void ShowTimeInputChecked(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).IsChecked ?? false)
                TimeInput.BeginAnimation(OpacityProperty, showHalfTimeInput);
            else
                TimeInput.BeginAnimation(OpacityProperty, hideHalfTimeInput);

            Settings.SaveSetting(userSettings);
        }

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.SaveSetting(userSettings);
        }

        private void WindowActivated(object sender, EventArgs e)
        {
            TextTyper.BufferText = TextTyper.GetBufferText(userSettings);
        }

    }
}
