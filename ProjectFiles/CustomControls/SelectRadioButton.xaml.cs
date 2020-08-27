using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using WpfClipboardTextTyper;

namespace ClipboardTextTyper
{
    /// <summary>
    /// Логика взаимодействия для RadioButton.xaml
    /// </summary>
    public partial class SelectRadioButton : UserControl, INotifyPropertyChanged
    {
        public static DependencyProperty IsCheckedProperty =
        DependencyProperty.Register("IsChecked", typeof(bool), typeof(SelectRadioButton));


        public string Text { get; set; }
        public string GroupName { get; set; }
        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set
            {
                SetValue(IsCheckedProperty, value);
                OnPropertyChanged();
            }
        }

        public event RoutedEventHandler Checked
        {
            add
            {
                MainButton.Checked += value;
            }
            remove
            {
                MainButton.Checked -= value;
            }
        }

        public event RoutedEventHandler Unchecked
        {
            add
            {
                MainButton.Unchecked += value;
            }
            remove
            {
                MainButton.Unchecked -= value;
            }
        }

        public SelectRadioButton()
        {
            InitializeComponent();

            DataContext = new ArrayList()
            {
                this,
                MainWindow.userSettings,
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
