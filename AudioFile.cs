using System;
using System.Collections.Generic;
using System.Text;

namespace CDRipperExample
{
    class AudioFile
    {
        public string Title { get; set; }
        public string Volume { get; set; }
        public string DriveLetter { get; set; }
        public string FilePath { get; set; }
        public string CurrentUser { get; set; }
        public string UserHomeFolder { get; set; }
        public string[] AudioFileCollection { get; set; }
        public int RandomNumber { get; set; }
    }
}
