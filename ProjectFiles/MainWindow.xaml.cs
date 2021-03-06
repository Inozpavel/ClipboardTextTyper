﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace ClipboardTextTyper
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Storyboard _startAnimation;
        private Storyboard _closingAnimation;

        internal static SettingsViewModel userSettings;
        private bool _isAnimanting = false;
        private bool _isClosing = false;

        public MainWindow()
        {
            userSettings = SettingsViewModel.LoadSettings() ?? new SettingsViewModel();

            int sliderTime = userSettings.DelayTime;
            InitializeComponent();
            Slider.Value = sliderTime;

            DataContext = userSettings;
        }

        private void DragMoveApp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void SliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider.Value = Math.Round(e.NewValue);
            SettingsViewModel.SaveSetting(userSettings);
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
            TextTyper.KeysListening.Initialize();
            TextTyper.window = Process.GetProcessesByName("ClipboardTextTyper")[0];
            _startAnimation = WindowAnimationDictionary["WindowStartingAnimation"] as Storyboard;
            _closingAnimation = WindowAnimationDictionary["WindowClosingAnimation"] as Storyboard;
            TimeInput.Opacity = userSettings.ShouldDelayBe ? 1 : 0;
            _startAnimation.Completed += (x, y) =>
            {
                RBHideTimeInput.IsChecked = !userSettings.ShouldDelayBe;
                RBShowTimeInput.IsChecked = userSettings.ShouldDelayBe;
            };
            BeginStoryboard(_startAnimation);
        }


        private void ShowTimeInputChecked(object sender, RoutedEventArgs e)
        {
            SettingsViewModel.SaveSetting(userSettings);
        }

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            SettingsViewModel.SaveSetting(userSettings);
        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            if (_isClosing)
                return;
            _isClosing = true;
            BeginStoryboard(_closingAnimation);

            Task.Run(() =>
            {
                Thread.Sleep(1970);
                Dispatcher.Invoke(() => Close());
            });
        }

        private void MinimizeApp(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            SettingsViewModel.SaveSetting(userSettings);
        }

        private void ShowTypingInfo(object sender, RoutedEventArgs e)
        {
            int oneLetterPrintTime = 23;
            string resultInfo;
            string text = Regex.Replace(TextTyper.GetBufferText() ?? "", @"\r", "");
            int charsCount = text.Length;
            int charsToPrintCount = TextTyper.FilterText(text, userSettings).Length;

            if (charsToPrintCount == 0)
            {
                resultInfo =
                $"Всего в буфере обмена символов текста: {charsCount}\n" +
                $"Из них будет напечатано символов: {charsToPrintCount}\n\n" +
                "Печатать нечего.";
                System.Windows.MessageBox.Show(resultInfo, "Информация о печати",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            TimeSpan printTime;

            if (userSettings.ShouldDelayBe)
                printTime = new TimeSpan(0, 0, 0, (charsToPrintCount * (userSettings.DelayTime + oneLetterPrintTime)) / 1000,
                    (charsToPrintCount * (userSettings.DelayTime + oneLetterPrintTime)) % 1000);
            else
                printTime = new TimeSpan(0, 0, 0, charsToPrintCount * oneLetterPrintTime / 1000,
                    charsToPrintCount * oneLetterPrintTime % 1000);

            resultInfo = $"Всего в буфере обмена символов текста: {charsCount}\n" +
                $"Из них будет напечатано символов: {charsToPrintCount}\n" +
                $"Приблизительное время печати:\n" +
                (printTime.Hours.Equals(0) ? "" : $"Часы: {printTime.Hours}\n") +
                (printTime.Minutes.Equals(0) ? "" : $"Минуты: {printTime.Minutes}\n") +
                (printTime.Seconds.Equals(0) ? "" : $"Секунды: {printTime.Seconds}\n") +
                (printTime.Milliseconds.Equals(0) ? "" : $"Милисекунды: {printTime.Milliseconds}\n");

            System.Windows.MessageBox.Show(resultInfo, "Информация о печати",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
