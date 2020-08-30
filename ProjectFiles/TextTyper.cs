using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;

namespace ClipboardTextTyper
{
    internal static class TextTyper
    {
        public static Process window;
        public static System.Threading.Timer[] KeysListening =
        {
            new System.Threading.Timer(ListenShift, null, 0, 1),
            new System.Threading.Timer(ListenAbordTyping, null, 0, 1),
            new System.Threading.Timer(ListenPauseTyping, null, 0, 1),
            new System.Threading.Timer(ListenContinueTyping, null, 0, 1),
            new System.Threading.Timer(ListenStartTyping, null, 0, 1),
        };

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
        private static char[] BufferText { get; set; }

        #region Win32

        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hMem);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hMem, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetAsyncKeyState(int vKey);

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
        private const int SW_SHOWNORMAL = 1;
        private const int SW_MINIMIZE = 6;

        #endregion

        #region Keys listening

        private static void ListenShift(object obj)
        {
            _isShiftPressed = GetAsyncKeyState((int)Keys.LShiftKey) != 0;
        }

        private static void ListenAbordTyping(object obj)
        {
            if (GetAsyncKeyState((int)Keys.F2) != 0 && _isShiftPressed)
                _shouldAbort = true;
        }

        private static void ListenPauseTyping(object obj)
        {
            if (_isTyping == false)
                return;
            if (GetAsyncKeyState((int)Keys.F11) != 0 && _isShiftPressed)
            {
                _shouldPause = true;
                MainWindow.userSettings.TypingStatus = "Печать приостановлена";
                if (MainWindow.userSettings.ShouldChangeWindowOnPause)
                {
                    SetForegroundWindow(window.MainWindowHandle);
                    ShowWindow(window.MainWindowHandle, SW_SHOWNORMAL);
                }
            }
        }

        private static void ListenContinueTyping(object obj)
        {
            if (_isTyping == false)
                return;
            if (GetAsyncKeyState((int)Keys.F12) != 0 && _isShiftPressed)
            {
                _shouldPause = false;
                MainWindow.userSettings.TypingStatus = "Печать запущена";
            }
        }

        private static void ListenStartTyping(object obj)
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

        private static void Print()
        {
            BufferText = FilterText(GetBufferText(), MainWindow.userSettings);
            if (BufferText == null)
                return;
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                foreach (char symbol in BufferText)
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
                stopwatch.Stop();

                if (MainWindow.userSettings.ShouldNotifyOnPrintComplete)
                {
                    TimeSpan resultTime = stopwatch.Elapsed;

                    string resultTimeInfo = string.Format("Часы:\t\t{0:00}\nМинуты:\t\t{1:00}\nСекунды:\t{2:00}\nМилисекунды:\t{3:000}",
                        resultTime.Hours, resultTime.Minutes, resultTime.Seconds, resultTime.Milliseconds);

                    ShowWindow(window.MainWindowHandle, SW_MINIMIZE);
                    SetForegroundWindow(window.MainWindowHandle);

                    MessageBox.Show($"Программа завершила печать, время печати:\n{resultTimeInfo}",
                        "Печать завершена", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ShowWindow(window.MainWindowHandle, SW_SHOWNORMAL);
                }
            }
            catch
            {
                MessageBox.Show("При печати произошла ошибка.", "Ошибка.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            MainWindow.userSettings.TypingStatus = "Печать не запущена";
            _isTyping = false;

        }

        public static char[] FilterText(string text, SettingsViewModel settings)
        {
            text = Regex.Replace(text ?? "", @"\r", "");

            string[] splittedText = text.Split('\n');

            for (int i = 0; i < splittedText.Length; i++)
            {
                if (string.IsNullOrEmpty(splittedText[i]))
                    continue;

                int spacesAtStringStartCount = Regex.Matches(splittedText[i], "^    *")
                    .Cast<Match>().ToList().FirstOrDefault()?.Length ?? 0;

                splittedText[i] = new string('\t', spacesAtStringStartCount / 4) +
                                  new string(' ', spacesAtStringStartCount % 4) +
                                  splittedText[i].TrimStart(' ');
            }
            text = string.Join("\n", splittedText);

            List<string> spacesToDelete = Regex
                .Matches(settings.CharsToDelete, @"\s+")
                .Cast<Match>().Select(x => x.Value).ToList();

            string symbolsToDelete = settings.CharsToDelete;

            List<string> commonSymbols = string
                .Join("", Regex.Split(symbolsToDelete, @"\\\w")
                .Where(x => !string.IsNullOrEmpty(x)).ToList())
                .Select(x => x.ToString()).Where(x => x != " ").ToList();

            List<string> spesialSymbols = Regex
                .Matches(symbolsToDelete, @"\\\w")
                .Cast<Match>().Select(x => x.Value).ToList();

            List<string> finalArrayToDelete = commonSymbols
                .Union(spesialSymbols).Union(spacesToDelete).ToList();

            foreach (string symbol in finalArrayToDelete)
                text = Regex.Replace(text, symbol, "");

            return text.ToArray();
        }

        #endregion
    }
}
