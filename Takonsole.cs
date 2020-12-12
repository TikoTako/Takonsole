using System;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Reflection;

namespace TikoTako
{
    /// <summary>
    /// Enable the console
    /// Output different types: Normal, Informations, Warnings, Errors with different colors.
    /// Has a lock just in case.
    /// Has timestamp output.
    /// 
    /// https://docs.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences
    /// </summary>
    public class Takonsole
    {
        [DllImport("kernel32")] //https://docs.microsoft.com/en-us/windows/console/allocconsole
        private static extern bool AllocConsole();
        [DllImport("kernel32")] // https://docs.microsoft.com/en-us/windows/console/getstdhandle
        private static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32")] // https://docs.microsoft.com/en-us/windows/console/setconsolemode
        private static extern bool SetConsoleMode(IntPtr nStdHandle, uint dwMode);

        private enum OutType { Normal, Information, Warning, Error }

        private static readonly object theLock = new object();
        private static readonly Encoding encoding = Encoding.Default; // change this if you see random chars

        public Color NormalColor { get; set; } = Color.White;
        public Color InformationColor { get; set; } = Color.Cyan;
        public Color WarningColor { get; set; } = Color.Orange;
        public Color ErrorColor { get; set; } = Color.IndianRed;
        public Color? BackGround { get; set; } = null;

        private static Takonsole HighLander = null;

        private Takonsole()
        {
            if (HighLander == null)
            {
                HighLander = this;
            }
        }

        /// <summary>
        /// This is used instead of the constructor so it allocate only one console.
        /// </summary>
        /// <param name="Title">Console window title.</param>
        /// <returns></returns>
        public static Takonsole Alloc(string Title)
        {
            if (HighLander == null)
            {
                AllocConsole();
                Console.Title = Title;
                _ = new Takonsole();
                SetConsoleMode(GetStdHandle(-11), 0x0001 | 0x0004);
                // this little crap took over nine thousand hours to find
                // Console.OutputEncoding != Console.Out.Encoding
                Console.OutputEncoding = encoding;
                Console.SetOut(
                    new StreamWriter(
                                     new FileStream(
                                                    new SafeFileHandle(
                                                                       GetStdHandle(-11),
                                                                       true),
                                                    FileAccess.Write),
                                                    encoding)
                    { AutoFlush = true });
            }
            return HighLander;
        }

        public void Test()
        {
            string buff;
            PropertyInfo[] arrayOfColors = (typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public));
            Console.WriteLine($"arrayOfColors.Length = {arrayOfColors.Length}");
            foreach (var bColor in arrayOfColors)
            {
                buff = "";
                foreach (var fColor in arrayOfColors)
                {
                    buff += RawPrint("#", false, (Console.CursorLeft >= Console.WindowWidth), false, Color.FromName(fColor.Name), Color.FromName(bColor.Name));
                }
                Console.Write(buff);
            }
            RawPrintToConsole("", false, true, true, null, null);
            ConsoleWrite("#### TEST NORMAL ####", OutType.Normal);
            ConsoleWrite("#### TEST INFORMATION ####", OutType.Information);
            ConsoleWrite("#### TEST WARNING ####", OutType.Warning);
            ConsoleWrite("#### TEST ERROR ####", OutType.Error);
        }

        /// <summary>
        /// Print out _message to the console with more control.
        /// </summary>
        /// <param name="_message">The message to print.</param>
        /// <param name="_timeStamp">Timestamp on/off</param>
        /// <param name="newLine">Newline on message end yes/no.</param>
        /// <param name="resetColors">Reset the colors back to normal on text end.</param>
        /// <param name="_char">Text color, can be null.</param>
        /// <param name="_back">Background color, can be null.</param>
        public void RawPrintToConsole(string _message, bool _timeStamp, bool newLine, bool resetColors, Color? _char, Color? _back)
        {
            lock (theLock)
            {
                Console.Write(RawPrint(_message, _timeStamp, newLine, resetColors, _char, _back));
            }
        }

        private string RawPrint(string _message, bool _timeStamp, bool newLine, bool resetColors, Color? _char, Color? _back)
        {
            string backColor = _back != null ? $"\u001b[48;2;{_back?.R};{_back?.G};{_back?.B}m" : String.Empty;
            string charColor = _char != null ? $"\u001b[38;2;{_char?.R};{_char?.G};{_char?.B}m" : String.Empty;
            string timeStamp = _timeStamp == true ? $"[{DateTime.Now.ToLocalTime()}] " : String.Empty;
            string reset = resetColors ? /*((_char != null && _back != null) ? */"\u001b[0m"/* : String.Empty)*/ : String.Empty;
            string happyEnding = newLine ? Environment.NewLine : String.Empty;
            return $"{timeStamp}{backColor}{charColor}{_message}{reset}{happyEnding}";
        }

        private void ConsoleWrite(string str, OutType outputType)
        {
            var tmpColor = outputType switch
            {
                OutType.Information => InformationColor,
                OutType.Warning => WarningColor,
                OutType.Error => ErrorColor,
                _ => NormalColor,
            };
            RawPrintToConsole(str, true, true, true, tmpColor, BackGround);
        }

        public static Action<string> Out = delegate (string str) { HighLander?.ConsoleWrite(str, OutType.Normal); };
        public static Action<string> Inf = delegate (string str) { HighLander?.ConsoleWrite(str, OutType.Information); };
        public static Action<string> Warn = delegate (string str) { HighLander?.ConsoleWrite(str, OutType.Warning); };
        public static Action<string> Err = delegate (string str) { HighLander?.ConsoleWrite(str, OutType.Error); };
    }
}
