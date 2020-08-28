using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Serialization;

namespace ClipboardTextTyper
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private static readonly string settingsFilePath = @".\Settings.xml";

        private static readonly XmlSerializer xmlSerializer = new
            XmlSerializer(typeof(SettingsViewModel));

        private bool _shouldDelayBe = false;

        public bool ShouldDelayBe
        {
            get => _shouldDelayBe;
            set
            {
                _shouldDelayBe = value;
                OnPropertyChanged();
            }
        }

        public int DelayTime { get; set; } = 10;

        public string CharsToDelete { get; set; } = "";

        public bool ShouldChangeWindowOnPause { get; set; } = true;

        public bool ShouldNotifyOnPrintComplete { get; set; } = true;

        private string _typingStatus = "Печать не запущена";

        [XmlIgnore]
        public string TypingStatus
        {
            get => _typingStatus;
            set
            {
                _typingStatus = value;
                OnPropertyChanged();
            }
        }

        public static void SaveSetting(SettingsViewModel settings)
        {
            using var stream = new FileStream(settingsFilePath, FileMode.Create);
            try
            {
                xmlSerializer.Serialize(stream, settings);
            }
            catch
            {
                MessageBox.Show("Не получилось сохранить настройки", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static SettingsViewModel LoadSettings()
        {
            using var stream = new FileStream(settingsFilePath, FileMode.Open);
            try
            {
                return (SettingsViewModel)xmlSerializer.Deserialize(stream);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Файл с настройками не найден, будут использованы настройки по умолчанию",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                return null;
            }
            catch
            {
                MessageBox.Show("Не удалось загрузить настройки, будут использованы настройки по умолчанию",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string paramName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(paramName));
        }
    }
}
