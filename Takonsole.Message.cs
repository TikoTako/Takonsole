using System;
using System.Drawing;

#nullable enable

namespace TikoTako
{
    public static partial class Takonsole
    {
        #region public class TimeStamp
        public class TimeStamp
        {
            public bool? dtl;
            public Style? style;
            public Color? foreground;
            public Color? background;
            public string customFormat;

            internal TimeStamp()
            {
                dtl = null;
                style = null;
                foreground = null;
                background = null;
                customFormat = "";
            }

            /// <summary>
            /// TimeStamp.            
            /// <para>dtl = null -> DateTime.Now.ToLocalTime()</para>
            /// <para>dtl = true -> DateTime.Now.ToShortDateString()</para>
            /// <para>dtl = false -> DateTime.Now.ToShortTimeString()</para>
            /// </summary>
            /// <param name="dtl">Determine the DateTime output format.</param>
            /// <param name="style">Style.</param>
            /// <param name="foreground">Font color.</param>
            /// <param name="background">Background color.</param>
            public TimeStamp(bool? dtl, Style? style, Color? foreground, Color? background)
            {
                this.dtl = dtl;
                this.style = style;
                this.foreground = foreground;
                this.background = background;
                customFormat = "";
            }
        }
        #endregion

        #region public struct Message
        public struct Message
        {
            public string message;
            public Style? style;
            public Color? foreground;
            public Color? background;

            /// <summary>
            /// Message with no style, colors set by Takonsole properties.
            /// </summary>
            public Message(string message) : this()
            {
                style = null;
                foreground = null;
                background = null;
                this.message = message;
            }

            /// <summary>
            /// Message with a style, colors set by Takonsole properties.
            /// </summary>
            public Message(string message, Style? style) : this(message)
            {
                this.style = style;
            }

            /// <summary>
            /// Message with a font color, no stlye, and background color set by Takonsole properties.
            /// </summary>
            public Message(string message, Color? foreground) : this(message)
            {
                this.foreground = foreground;
            }

            /// <summary>
            /// Message with a font color and a stlye, background color set by Takonsole properties.
            /// </summary>
            public Message(string message, Style? style, Color? foreground) : this(message, style)
            {
                this.foreground = foreground;
            }

            /// <summary>
            /// Message with style, font and background color.
            /// </summary>
            public Message(string message, Style? style, Color? foreground, Color? background) : this(message, style)
            {
                this.foreground = foreground;
                this.background = background;
            }
        }
        #endregion

        #region Style -- SetStyle(Style? style) -- UnSetStyle(Style? style) -- ActualSetStyle(Style style, string[] actualStylesList)
        public enum Style { Normal = 0b0001, Bold = 0b0010, Underline = 0b0100, Reverse = 0b1000 }

        /// <summary>
        /// Set the font style, Normal, Bold, Underline, Reverse
        /// <code>Bold|Reverse</code>
        /// <code>Bold|Underline</code>
        /// <code>Underline|Reverse</code>
        /// <code>Bold|Reverse|Underline</code>
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        public static string SetStyle(Style? style)
        {
            return style.HasValue ? ActualSetStyle((Style)style, BURN) : String.Empty;
        }

        public static string UnSetStyle(Style? style)
        {
            return style.HasValue ? ActualSetStyle((Style)style, BUR) : String.Empty;
        }

        private static string ActualSetStyle(Style style, string[] asL/*actualStylesList*/)
        {
            //string[] sL = actualStylesList;
            return style switch
            {
                // Tested working (italic, crossed, etc... dont work)
                Style.Bold => asL[0],
                Style.Underline => asL[1],
                Style.Reverse => asL[2],
                Style.Normal => asL[3],
                Style.Bold | Style.Underline => $"{asL[0]}{asL[1]}",
                Style.Bold | Style.Reverse => $"{asL[0]}{asL[2]}",
                Style.Reverse | Style.Underline => $"{asL[2]}{asL[1]}",
                Style.Bold | Style.Reverse | Style.Underline => $"{asL[0]}{asL[2]}{asL[1]}",
                _ => throw new NotImplementedException()
            };
        }
        #endregion 

        #region SetFontColor(Color? color) -- SetBackgroundColor(Color? color) -- ActualSetColor(bool isBackground, Color color)
        public static string SetFontColor(Color? color)
        {
            return color.HasValue ? ActualSetColor(false, (Color)color) : ""; //ActualSetColor(false, color.HasValue ? (Color)color : NormalColor);
        }

        public static string SetBackgroundColor(Color? color)
        {
            return color.HasValue ? ActualSetColor(true, (Color)color) : ""; // ActualSetColor(true, color.HasValue ? (Color)color : BackgroundColor);
        }

        private static string ActualSetColor(bool isBackground, Color color)
        {
            return $"\x1b[{(isBackground ? "48" : "38")};2;{color.R};{color.G};{color.B}m";
        }
        #endregion 

        public static string GenerateRawFromStructs(TimeStamp? timeStamp, Message message)
        {
            string tS = "", mS;
            if (timeStamp != null)
            {
                DateTime dtN = DateTime.Now;
                // ignore timeStamp.dtl if customFormat is set
                if (timeStamp.customFormat.Equals(""))
                {
                    tS = GenerateRawFromString($"[{(timeStamp.dtl.HasValue ? (((bool)timeStamp.dtl) ? dtN.ToShortDateString() : dtN.ToShortTimeString()) : dtN.ToLocalTime().ToString())}] ", timeStamp.style, timeStamp.foreground ?? TimestampColor, timeStamp.background);
                }
                else
                {                    
                    tS = GenerateRawFromString($"[{dtN.ToString(timeStamp.customFormat)}] ", timeStamp.style, timeStamp.foreground ?? TimestampColor, timeStamp.background);
                }
            }
            mS = GenerateRawFromString(message.message, message.style, message.foreground, message.background);
            return $"{tS}{mS}";
        }

        public static string GenerateRawFromString(string str, Style? style, Color? fontColor, Color? backgroundColor)
        {
            bool uF = fontColor.HasValue && !fontColor.Equals(NormalColor);
            bool uB = backgroundColor.HasValue && !backgroundColor.Equals(BackgroundColor);
            return $"{SetStyle(style)}{SetFontColor(fontColor)}{SetBackgroundColor(backgroundColor)}{str}{(uB ? SetBackgroundColor(BackgroundColor) : "")}{(uF ? SetFontColor(NormalColor) : "")}{UnSetStyle(style)}";
        }
    }
}