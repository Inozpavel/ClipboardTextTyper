using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
            '^',
            '%',
            '~',
            '(',
            ')',
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
        public static Process window;
        public static System.Threading.Timer[] KeysListening =
        {
            new System.Threading.Timer(ListenShift, null, 0, 1),
            new System.Threading.Timer(ListenAbordTyping, null, 0, 1),
            new System.Threading.Timer(ListenPauseTyping, null, 0, 1),
            new System.Threading.Timer(ListenContinueTyping, null, 0, 1),
            new System.Threading.Timer(ListenStartTyping, null, 0, 1),
        };

        #region Win32

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern bool IsClipboardFormatAvailable(uint format);

        [DllImport("user32.dll")]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        private static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("Kernel32.dll")]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("Kernel32.dll")]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("Kernel32.dll")]
        private static extern int GlobalSize(IntPtr hMem);

        private const uint CF_UNICODETEXT = 13U;

        #endregion

        #region Keys listening

        public static void ListenShift(object obj)
        {
            _isShiftPressed = GetAsyncKeyState((int)Keys.LShiftKey) != 0;
        }

        static void ListenAbordTyping(object obj)
        {
            if (GetAsyncKeyState((int)Keys.F2) != 0 && _isShiftPressed)
                _shouldAbort = true;
        }

        static void ListenPauseTyping(object obj)
        {
            if (_isTyping == false)
                return;
            if (GetAsyncKeyState((int)Keys.F11) != 0 && _isShiftPressed)
            {
                _shouldPause = true;
                MainWindow.userSettings.TypingStatus = "Печать приостановлена";
                SetForegroundWindow(window.MainWindowHandle);
            }
        }

        static void ListenContinueTyping(object obj)
        {
            if (_isTyping == false)
                return;
            if (GetAsyncKeyState((int)Keys.F12) != 0 && _isShiftPressed)
            {
                _shouldPause = false;
                MainWindow.userSettings.TypingStatus = "Печать запущена";
            }
        }

        static void ListenStartTyping(object obj)
        {
            if (GetAsyncKeyState((int)Keys.F4) != 0 && _isShiftPressed)
            {
                if (_isTyping == false)
                {
                    _isTyping = true;
                    MainWindow.userSettings.TypingStatus = "Печать запущена";
                    Task.Run(() => Print());
                }
            }
        }

        #endregion

        #region Methods working with text

        public static string GetBufferText()
        {
            if (IsClipboardFormatAvailable(CF_UNICODETEXT) == false)
                return null;
            try
            {
                if (!OpenClipboard(IntPtr.Zero))
                    return null;

                IntPtr handle = GetClipboardData(CF_UNICODETEXT);
                if (handle == IntPtr.Zero)
                    return null;

                IntPtr pointer = IntPtr.Zero;

                try
                {
                    pointer = GlobalLock(handle);
                    if (pointer == IntPtr.Zero)
                        return null;

                    int size = GlobalSize(handle);
                    byte[] buff = new byte[size];

                    Marshal.Copy(pointer, buff, 0, size);

                    return Encoding.Unicode.GetString(buff).TrimEnd('\0');
                }
                finally
                {
                    if (pointer != IntPtr.Zero)
                        GlobalUnlock(handle);
                }
            }
            finally
            {
                CloseClipboard();
            }
        }
        static Task task = new Task(() => Console.WriteLine());
        public static void Print()
        {
            BufferText = FilterText(MainWindow.userSettings);
            if (BufferText == null)
                return;
            try
            {
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
                    else
                        Thread.Sleep(1);
                }
            }
            catch
            {
                MessageBox.Show("При печати произошла ошибка.", "Ошибка.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            MainWindow.userSettings.TypingStatus = "Печать не запущена";
            _isTyping = false;
        }

        public static char[] FilterText(Settings settings)
        {
            string bufferText = Regex.Replace(GetBufferText() ?? "", @"\r", "");

            var spacesToDelete = Regex.Matches(settings.CharsToDelete, @"\s+")
                .Cast<Match>().Select(x => x.Value).ToList();

            var symbolsToDelete = settings.CharsToDelete;
            var commonSymbols = string.Join("", Regex.Split(symbolsToDelete, @"\\\w")
                .Where(x => !string.IsNullOrEmpty(x)).ToList())
                .Select(x => x.ToString()).Where(x => x != " ").ToList();
            var spesialSymbols = Regex.Matches(symbolsToDelete, @"\\\w")
                .Cast<Match>().Select(x => x.Value).ToList();

            foreach (var symbol in commonSymbols.Union(spesialSymbols).Union(spacesToDelete))
                bufferText = Regex.Replace(bufferText, symbol, "");

            return bufferText.ToArray();
        }

        #endregion
    }
}
