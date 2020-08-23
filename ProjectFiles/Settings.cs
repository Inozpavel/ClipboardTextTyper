using System.ComponentModel;
using System.IO;
using System.Windows;
using Newtonsoft.Json;

namespace WpfClipboardTextTyper
{
    internal class Settings : INotifyPropertyChanged
    {
        private static readonly string settingsFilePath = @".\Settings.json";

        public event PropertyChangedEventHandler PropertyChanged;

        public bool ShouldDelayBe { get; set; } = false;

        public int DelayTime { get; set; } = 10;

        public string CharsToDelete { get; set; } = "";

        [JsonIgnore]
        private string _typingStatus = "Печать не запущена";

        [JsonIgnore]
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
            try
            {
                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(settings));
            }
            catch
            {
                MessageBox.Show("Не получилось сохранить настройки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static Settings LoadSettings()
        {
            try
            {
                return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsFilePath));
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
