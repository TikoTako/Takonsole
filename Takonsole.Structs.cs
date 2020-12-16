using System.Runtime.InteropServices;

namespace TikoTako
{
    /* Structs for the DLLImport Get/SetCurrentConsoleFontEx */
    public static partial class Takonsole
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct FontSize
        {
            public short X;
            public short Y;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct FontInfoEx
        {
            internal uint cbSize;
            internal uint nFont;
            internal FontSize dwFontSize;
            internal int FontFamily;
            internal int FontWeight;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string FaceName;
        }
    }
}