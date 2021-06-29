using System.Runtime.InteropServices;
using System.Text;

namespace NewsScroll
{
    class AppInterop
    {
        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
        public static extern int UrlMkSetSessionOption(UrlMonSessionOptions dwOption, string pBuffer, int dwBufferLength, int dwReserved);
        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
        public static extern int UrlMkGetSessionOption(UrlMonSessionOptions dwOption, StringBuilder pBuffer, int dwBufferLength, ref int pdwBufferLength, int dwReserved);
        public enum UrlMonSessionOptions : int
        {
            URLMON_OPTION_USERAGENT = 0x10000001,
            URLMON_OPTION_USERAGENT_REFRESH = 0x10000002,
            URLMON_OPTION_URL_ENCODING = 0x10000004
        }
    }
}