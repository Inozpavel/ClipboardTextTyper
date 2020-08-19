using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
                _isShiftPressed = GetAsyncKeyState((int)Keys.LShiftKey) != 0;
            }
        });

        #region Win32

        [DllImport("User32.dll")]
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

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private const uint CF_UNICODETEXT = 13U;

        #endregion

        public static string GetBufferText()
        {
            if (IsClipboardFormatAvailable(CF_UNICODETEXT) == false)
                return "";
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
                }
            }
            catch
            {
                MessageBox.Show("При печати произошла ошибка.", "Ошибка.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            _isTyping = false;
        }

        private static void ListenKeys()
        {
            while (true)
            {
                if (GetAsyncKeyState((int)Keys.F2) != 0 && GetAsyncKeyState((int)Keys.LShiftKey) != 0)
                    _shouldAbort = true;
                if (GetAsyncKeyState((int)Keys.F12) != 0 && GetAsyncKeyState((int)Keys.LShiftKey) != 0)
                    _shouldPause = !_shouldPause;
                if (GetAsyncKeyState((int)Keys.F4) != 0 && GetAsyncKeyState((int)Keys.LShiftKey) != 0)
                {
                    if (_isTyping == false)
                    {
                        _isTyping = true;
                        Task.Run(() => Print());
                    }
                }
            }
        }

        public static char[] FilterText(Settings settings)
        {
            string bufferText = Regex.Replace(GetBufferText(), @"\r", "");
            string s = GetBufferText();

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
    }
}
