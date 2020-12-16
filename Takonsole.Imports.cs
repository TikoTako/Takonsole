using System;
using System.Runtime.InteropServices;

namespace TikoTako
{
    /* All the dll imports here. */
    public static partial class Takonsole
    {
        //https://docs.microsoft.com/en-us/windows/console/allocconsole
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool AllocConsole();

        //https://docs.microsoft.com/en-us/windows/console/allocconsole
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool FreeConsole();

        // https://docs.microsoft.com/en-us/windows/console/getstdhandle
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        // https://docs.microsoft.com/en-us/windows/console/setconsolemode
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr nStdHandle, uint dwMode);

        // https://docs.microsoft.com/en-us/windows/console/getcurrentconsolefontex
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool GetCurrentConsoleFontEx(IntPtr conOutHandle, bool maximumWindow, ref FontInfoEx fontInfoEx);

        // https://docs.microsoft.com/en-us/windows/console/setcurrentconsolefontex
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool SetCurrentConsoleFontEx(IntPtr conOutHandle, bool maximumWindow, ref FontInfoEx fontInfoEx);
    }
}