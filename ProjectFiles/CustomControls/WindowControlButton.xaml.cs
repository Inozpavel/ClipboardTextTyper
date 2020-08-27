using System.Windows.Controls;

namespace ClipboardTextTyper
{
    /// <summary>
    /// Логика взаимодействия для WindowControlButton.xaml
    /// </summary>
    public partial class WindowControlButton : UserControl
    {
        private string _startImagePath;
        private string _mouseOverImagePath;

        public string StartImagePath
        {
            get => _startImagePath;
            set => _startImagePath = "../" + value;
        }
        public string MouseOverImagePath
        {
            get => _mouseOverImagePath;
            set => _mouseOverImagePath = "../" + value;
        }

        public WindowControlButton()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
