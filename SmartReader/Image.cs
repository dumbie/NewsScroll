using System;

namespace SmartReader
{
    public class Image
    {
        public Uri Source { get; set; } = null;
        public long Size { get; set; } = 0;
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
    }
}