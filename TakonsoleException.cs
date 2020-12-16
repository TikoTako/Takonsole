using System;

namespace TikoTako
{
    /// <summary>
    /// Takonsole Exception class.
    /// <para>Use the ErrorCode enum to localize the message.</para>
    /// </summary>
    public class TakonsoleException : Exception
    {
        public enum ErrorCode { Alloc, Free, GetHandle, SetMode, GetFont, SetFont, Unset, Unknown }

        public ErrorCode errorCode { get; set; }

        public TakonsoleException() : base() { }

        public TakonsoleException(string message) : base(message) { }

        public TakonsoleException(ErrorCode eCode) : base() { errorCode = eCode; }

        public TakonsoleException(string message, ErrorCode eCode) : base(message) { errorCode = eCode; }

        public TakonsoleException(int lastMessage, ErrorCode eCode) : base($"0x{lastMessage:4X}") { errorCode = eCode; }
    }
}