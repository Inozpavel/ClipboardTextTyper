using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace WpfClipboardTextTyper
{
    public class Settings : INotifyPropertyChanged
    {
        private static readonly string settingsFilePath = @".\Settings.xml";

        private readonly static XmlSerializer xmlSerializer = new XmlSerializer(typeof(Settings));

        public event PropertyChangedEventHandler PropertyChanged;

        public bool ShouldDelayBe { get; set; } = false;

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
                PropertyChanged(this, new PropertyChangedEventArgs("TypingStatus"));
            }
        }

        public static void SaveSetting(Settings settings)
        {
            FileStream stream = new FileStream(settingsFilePath, FileMode.Create);
            try
            {
                xmlSerializer.Serialize(stream, settings);
            }
            catch
            {
                MessageBox.Show("Не получилось сохранить настройки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                stream.Close();
            }
        }

        public static Settings LoadSettings()
        {
            try
            {
                FileStream stream = new FileStream(settingsFilePath, FileMode.Open);
                return (Settings)xmlSerializer.Deserialize(stream);
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
    }


}
