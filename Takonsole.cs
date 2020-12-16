using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Reflection;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace TikoTako
{
    /// <summary>
    /// Alloc/Dealloc the Windows console.
    /// <para>Output different types: Normal, Informations, Warnings, Errors with timestamp and a different color each.</para>
    /// <para>Color is a System.Drawing.Color</para>
    /// </summary>
    public static partial class Takonsole
    {
        // https://docs.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences

        private enum OutType { Normal, Information, Warning, Error }

        private const string ESC = "\x1b[{0}m";
        // Unset [R]everse [U]nderline [B]old
        //private readonly static string[] RUB = { F(ESC, 27), F(ESC, 24), F(ESC, 22) };
        private readonly static string[] BUR = { F(ESC, 22), F(ESC, 24), F(ESC, 27), F(ESC, 00) };
        // Set [B]old [U]nderline [R]everse [N]ormal
        private readonly static string[] BURN = { F(ESC, 01), F(ESC, 04), F(ESC, 07), F(ESC, 00) };

        private static readonly object theLock = new object();
        private static bool HighLander = false; // There-will-be-only-one-joke.webm.exe

        private static Color _nC = Color.LightGray;
        private static Color _bgC = Color.Black;
        public static Color ErrorColor { get; set; } = Color.IndianRed;
        public static Color WarningColor { get; set; } = Color.Orange;
        public static Color InformationColor { get; set; } = Color.Cyan;
        public static Color TimestampColor { get; set; } = Color.SlateGray;
        public static Color NormalColor
        {
            get { return _nC; }
            set
            {
                _nC = value;
                if (HighLander) Console.Write(SetFontColor(value));
            }
        }
        public static Color BackgroundColor
        {
            get { return _bgC; }
            set
            {
                _bgC = value;
                if (HighLander) Console.Write(SetBackgroundColor(value));
            }
        }

        /// <summary>
        /// Set the console background.
        /// <para>The method will call a CLS()</para>
        /// <para>If you don't want to clear the screen, use the BackgroundColor property.</para>
        /// </summary>
        /// <param name="backgroundColor">The background color.</param>
        public static void SetConsoleBackground(Color backgroundColor)
        {
            if (!HighLander) return;
            BackgroundColor = backgroundColor;
            Console.Clear();
        }

        /// <summary>
        /// Short version of String.Format(string, args[])
        /// </summary>
        private static string F(string str, params object?[] args) { return String.Format(str, args); }

        /// <summary>
        /// Allocate the console with default encoding.
        /// </summary>
        /// <param name="ConsoleWindowTitle">Title of the console window.</param>
        /// <returns>true if the console is allocated</returns>
        public static bool Alloc(string ConsoleWindowTitle)
        {
            return Alloc(ConsoleWindowTitle, Encoding.Default);
        }

        /// <summary>
        /// Allocate the console.
        /// <para>Can throw a TakonsoleException.</para>
        /// </summary>
        /// <param name="consoleWindowTitle">Title of the console window.</param>
        /// <param name="encoding">The encoding for the output.</param>
        /// <returns>true if the console is allocated</returns>
        public static bool Alloc(string consoleWindowTitle, Encoding encoding)
        {
            if (HighLander == false)
            {
                if (!AllocConsole())
                {
                    throw new TakonsoleException(Marshal.GetLastWin32Error(), TakonsoleException.ErrorCode.Alloc);
                }
                HighLander = true;
                Console.Title = consoleWindowTitle;
                if (!SetConsoleMode(GetStdHandle(-11), 0x0001 | 0x0004)) // check link in dllimport for more info
                {
                    throw new TakonsoleException(Marshal.GetLastWin32Error(), TakonsoleException.ErrorCode.SetMode);
                }
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
                GetFont();
            }
            return HighLander;
        }

        /// <summary>
        /// Set the font for the console window.
        /// <para>Can throw a TakonsoleException.</para>
        /// <para> The font must be:</para>
        /// <list type="bullet">
        /// <item>TrueType</item>
        /// <item>fixed-pitch</item>
        /// <item>non Italic</item>
        /// <item>with no negative A and/or C spaces.</item>
        /// </list>
        /// </summary>
        /// <param name="fontName">Font name.</param>
        /// <param name="fontSize">Font size.</param>
        public static void SetFont(string fontName, short? fontSize)
        {
            if (!HighLander) return;
            /* not supported
                Console.WriteLine($"\u001b[10mFONT 1\u001b[0m");
                Console.WriteLine($"\u001b[11mFONT 2\u001b[0m");
                Console.WriteLine($"\u001b[15mFONT 6\u001b[0m");
            */
            fontInfoEx.FaceName = fontName;
            // change the size if set, work with only Y            
            fontInfoEx.dwFontSize.Y = (fontSize <= 0 ? fontInfoEx.dwFontSize.Y : fontSize) ?? fontInfoEx.dwFontSize.Y;
            if (!SetCurrentConsoleFontEx(GetStdHandle(-11), false, ref fontInfoEx))
            {
                throw new TakonsoleException(Marshal.GetLastWin32Error(), TakonsoleException.ErrorCode.SetFont);
            }
            // update
            GetFont();
        }

        public static short FontWidth { get; private set; } = 0;
        private static FontInfoEx fontInfoEx;

        private static void GetFont()
        {
            if (HighLander)
            {
                fontInfoEx = new FontInfoEx();
                fontInfoEx.cbSize = (uint)Marshal.SizeOf(fontInfoEx);
                // load the whole struct of current font used
                if (!GetCurrentConsoleFontEx(GetStdHandle(-11), false, ref fontInfoEx))
                {
                    throw new TakonsoleException(Marshal.GetLastWin32Error(), TakonsoleException.ErrorCode.GetFont);
                }
                FontWidth = fontInfoEx.dwFontSize.X;
            }
        }

        /// <summary>
        /// Remove the console.
        /// </summary>
        public static void DeAlloc()
        {
            if (HighLander)
            {
                if (!FreeConsole())
                {
                    throw new TakonsoleException(Marshal.GetLastWin32Error(), TakonsoleException.ErrorCode.Free);
                }
                else
                {
                    HighLander = false;
                }
            }
        }

        /// <summary>
        /// Print out a test.
        /// <para>Throw a TakonsoleException if there is no console.</para>
        /// </summary>
        public static void Test()
        {
            if (!HighLander)
            {
                throw new TakonsoleException(TakonsoleException.ErrorCode.Unset);
            }
            //lock (theLock)
            {
                string buff;
                var tmpColors = Tuple.Create(NormalColor, BackgroundColor, InformationColor, WarningColor, ErrorColor);
                var arrayOfColors = (typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public));

                bool?[] tsF = { null, true, false };
                foreach (var item in tsF)
                {
                    TimeStampFormat.dtl = item;
                    Out("timestamped normal text");
                    Inf("timestamped information text");
                    Warn("timestamped warning text");
                    Err("timestamped error text");
                }

                Enumerable.Range(0, 255).ToList().ForEach(i => Write($"#", null, Color.FromArgb(i, 0, 0), null));
                Enumerable.Range(0, 255).Reverse().ToList().ForEach(i => Write($"#", null, Color.FromArgb(0, i, 0), null));
                Enumerable.Range(0, 255).ToList().ForEach(i => Write($"#", null, Color.FromArgb(0, 0, i), null));
                Enumerable.Range(0, 255).Reverse().ToList().ForEach(i => Write($"#", null, Color.FromArgb(i, i, 0), null));
                Enumerable.Range(0, 255).ToList().ForEach(i => Write($"#", null, Color.FromArgb(i, 0, i), null));
                Enumerable.Range(0, 255).Reverse().ToList().ForEach(i => Write($"#", null, Color.FromArgb(0, i, i), null));
                Enumerable.Range(0, 255).ToList().ForEach(i => Write($"#", null, Color.FromArgb(i, i, i), null));

                Console.WriteLine(Environment.NewLine);
                Console.WriteLine($"Array of colors (from System.Drawing.Color) = {arrayOfColors.Length}");
                foreach (var bColor in arrayOfColors)
                {
                    buff = "";
                    foreach (var fColor in arrayOfColors)
                    {
                        buff += $"{SetFontColor(Color.FromName(fColor.Name))}{SetBackgroundColor(Color.FromName(bColor.Name))}#{((Console.CursorLeft == Console.WindowWidth) ? Environment.NewLine : "")}";
                    }
                    Console.Write(buff);
                }
                (NormalColor, BackgroundColor, InformationColor, WarningColor, ErrorColor) = tmpColors;
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine($"done {SetFontColor(Color.BlueViolet)}{SetStyle(Style.Reverse)}DONE{UnSetStyle(Style.Reverse)}{SetFontColor(NormalColor)} done");
            }
        }

        /// <summary>
        /// Print to console
        /// <para>Text out format: "[timestamp] message"</para>
        /// <para>Text out colors: depending on "outputType"</para>
        /// </summary>
        /// <param name="str">message.</param>
        /// <param name="outputType">Type of output, the colors depend on this.</param>
        private static void OIWE(string str, OutType outputType)
        {
            var tmpColor = outputType switch
            {
                OutType.Information => InformationColor,
                OutType.Warning => WarningColor,
                OutType.Error => ErrorColor,
                _ => NormalColor,
            };

            Console.WriteLine(GenerateRawFromStructs(TimeStampFormat, new Message(str, tmpColor)));
        }

        public static TimeStamp TimeStampFormat { get; set; } = new TimeStamp();

        /// <summary>
        /// Same as Console.Clear()
        /// </summary>
        public static void CLS() { if (HighLander) Console.Clear(); }

        /// <summary>
        /// Write to console [TimeStamp] message
        /// <list type="bullet">
        /// <item>Timestamp format is set by TimeStampFormat</item>
        /// <item>Normal font style</item>
        /// <item>Timestamp color set by TimestampColor</item>
        /// <item>Message color set by NormalColor</item>
        /// <item>Bacground color set by BackgroundColor</item>
        /// </list>
        /// </summary>
        public static void Out(string message) { if (HighLander) OIWE(message, OutType.Normal); }

        /// <summary>
        /// Write to console [TimeStamp] message
        /// <list type="bullet">
        /// <item>Timestamp format is set by TimeStampFormat</item>
        /// <item>Normal font style</item>
        /// <item>Timestamp color set by TimestampColor</item>
        /// <item>Message color set by InformationColor</item>
        /// <item>Bacground color set by BackgroundColor</item>
        /// </list>
        /// </summary>
        public static void Inf(string message) { if (HighLander) OIWE(message, OutType.Information); }

        /// <summary>
        /// Write to console [TimeStamp] message
        /// <list type="bullet">
        /// <item>Timestamp format is set by TimeStampFormat</item>
        /// <item>Normal font style</item>
        /// <item>Timestamp color set by TimestampColor</item>
        /// <item>Message color set by WarningColor</item>
        /// <item>Bacground color set by BackgroundColor</item>
        /// </list>
        /// </summary>
        public static void Warn(string message) { if (HighLander) OIWE(message, OutType.Warning); }

        /// <summary>
        /// Write to console [TimeStamp] message
        /// <list type="bullet">
        /// <item>Timestamp format is set by TimeStampFormat</item>
        /// <item>Normal font style</item>
        /// <item>Timestamp color set by TimestampColor</item>
        /// <item>Message color set by ErrorColor</item>
        /// <item>Bacground color set by BackgroundColor</item>
        /// </list>
        /// </summary>
        public static void Err(string message) { if (HighLander) OIWE(message, OutType.Error); }

        /// <summary>
        /// Same as Console.Write(string) but it check if console is allocated;
        /// </summary>
        public static void Write(string str) { if (HighLander) Console.Write(str); }

        /// <summary>
        /// Output "message" to the console, without the timestamp, theres no newline.
        /// <para>Is possible to leave the parameters to null if not needed.</para>
        /// </summary>
        public static void Write(string str, Style? style, Color? fontColor, Color? backgroundColor)
        {
            if (HighLander)
                Console.Write(GenerateRawFromString(str, style, fontColor, backgroundColor));
        }

        /// <summary>
        /// Same as Console.WriteLine(string) but it check if console is allocated;
        /// </summary>
        public static void WriteLine() { if (HighLander) Console.WriteLine(); }

        /// <summary>
        /// Same as Console.WriteLine(string) but it check if console is allocated;
        /// </summary>
        public static void WriteLine(string str) { if (HighLander) Console.WriteLine(str); }

        /// <summary>
        /// Output "message" plus a newline to the console, without the timestamp.
        /// <para>Is possible to leave the parameters to null if not needed.</para>
        /// </summary>
        public static void WriteLine(string message, Style? style, Color? fontColor, Color? backgroundColor)
        {
            if (HighLander)
                Console.WriteLine(GenerateRawFromString(message, style, fontColor, backgroundColor));
        }
    }
}
