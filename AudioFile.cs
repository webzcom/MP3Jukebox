using System;
using System.Collections.Generic;
using System.Text;

namespace MP3Jukebox
{
    class AudioFile
    {
        public string Title { get; set; }
        public float Volume { get; set; }
        public string DriveLetter { get; set; }
        public string FilePath { get; set; }
        public string CurrentUser { get; set; }
        public string UserHomeFolder { get; set; }
        public string[] AudioFileCollection { get; set; }
        public string[] MetaDataCollection { get; set; }
        public int RandomNumber { get; set; }
        public string SearchTerm { get; set; }
        public int CustomCollectionCounter { get; set; }
        public bool IsInAutoPlayMode { get; set; }
        public int AutoPlayCounter { get; set; }
    }
}
