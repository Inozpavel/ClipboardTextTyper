using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WpfClipboardTextTyper
{
    internal static class TextTyper
    {
        private static readonly char[] _specialKeys =
        {
            '+',
            '(',
            ')',
            '^',
            '%',
            '~',
            '{',
            '}',
            '[',
            ']',
        };

        private static bool _shouldAbort = false;
        private static bool _isShiftPressed = false;
        private static bool _isTyping = false;
        private static bool _shouldPause = false;
        public static char[] BufferText { get; set; }

        public static Task HotkeysListening = new Task(() => ListenKeys());

        public static Task ListenShift = new Task(() =>
        {
            while (true)
            {
                _isShiftPressed = IsKeysPressed(Keys.LShiftKey, null);
            }
        });

        [DllImport("User32.dll")]
        public static extern int GetAsyncKeyState(int vKey);

        public static void Print()
        {
            if (BufferText == null)
                return;
            foreach (var symbol in BufferText)
            {
                while (_isShiftPressed || _shouldPause)
                {
                }

                if (_shouldAbort)
                {
                    _shouldAbort = false;
                    break;
                }

                if (_specialKeys.Contains(symbol))
                    SendKeys.SendWait($"{{{symbol}}}");
                else
                    SendKeys.SendWait(symbol.ToString());
                if (MainWindow.userSettings.ShouldDelayBe)
                    Thread.Sleep(MainWindow.userSettings.DelayTime);
            }
            _isTyping = false;
        }

        private static void ListenKeys()
        {
            while (true)
            {
                if (IsKeysPressed(Keys.F2, Keys.LShiftKey))
                    _shouldAbort = true;
                if (IsKeysPressed(Keys.F12, Keys.LShiftKey))
                    _shouldPause = !_shouldPause;
                if (IsKeysPressed(Keys.F4, Keys.LShiftKey))
                {
                    if (_isTyping == false)
                    {
                        _isTyping = true;
                        Task.Run(() => Print());
                    }
                }
            }
        }

        public static char[] GetBufferText(Settings settings)
        {
            string bufferText = Regex.Replace(Clipboard.GetText(TextDataFormat.Text), @"\r", "");

            var spacesToDelete = Regex.Matches(settings.CharsToDelete, @"\s+")
                .Cast<Match>().Select(x => x.Value).ToList();

            var CharsToDelete = settings.CharsToDelete;
            var commonSymbols = string.Join("", Regex.Split(CharsToDelete, @"\\\w")
                .Where(x => !string.IsNullOrEmpty(x)).ToList())
                .Select(x => x.ToString()).Where(x => x != " ").ToList();
            var spesialSymbols = Regex.Matches(CharsToDelete, @"\\\w")
                .Cast<Match>().Select(x => x.Value).ToList();

            foreach (var symbol in commonSymbols.Union(spesialSymbols).Union(spacesToDelete))
                bufferText = Regex.Replace(bufferText, symbol, "");
            
            return bufferText.ToArray();
        }

        private static bool IsKeysPressed(Keys key1, Keys? key2)
        {
            if (key2 == null)
                return GetAsyncKeyState((int)key1) != 0;
            return GetAsyncKeyState((int)key1) != 0 && GetAsyncKeyState((int)key2) != 0;
        }
    }
}
