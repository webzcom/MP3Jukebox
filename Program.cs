using System;
using System.IO;
using System.Threading;
using NAudio.Wave;
using System.Linq;

namespace CDRipperExample
{
    class Program
    {
        static void Main(string[] args)
        {
            FindMp3();
    }

        public static string FindMp3() {

            string searchText = "";
            //Prompt user for a Category and Topic
            Console.WriteLine("Enter Your Music Search Text:");
            searchText = Console.ReadLine();
            if (searchText == "")
            {
                searchText = "insane";
            }


            // Example usage
            string searchParameter = searchText;
            string folderPath = @"M:\old mp3s";

            var filePath = FindMp3FileByName(searchParameter, folderPath);

            if (!string.IsNullOrEmpty(filePath))
            {
                Console.WriteLine($"File found: {filePath}");
            }
            else
            {
                Console.WriteLine("No file found matching the search parameter.");
            }
            return filePath;
        }

        /// <summary>
        /// Searches for an MP3 file in the specified folder that contains the given search parameter in its name.
        /// </summary>
        /// <param name="searchParameter">The string to search for in the file name.</param>
        /// <param name="folderPath">The path to the folder containing the MP3 files.</param>
        /// <returns>The path to the first matching MP3 file found, or null if no match is found.</returns>
        public static string FindMp3FileByName(string searchParameter, string folderPath)
        {
            // Ensure the folder exists
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("The specified folder does not exist.");
                return null;
            }

            // Get all MP3 files in the specified folder
            var files = Directory.GetFiles(folderPath, "*.mp3");

            // Search for the first file that contains the search parameter in its name
            var matchingFile = files.FirstOrDefault(file => Path.GetFileName(file).Contains(searchParameter, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(matchingFile)) {
                //using (var ms = File.OpenRead("c:\\temp\\insane.mp3"))
                using (var ms = File.OpenRead(matchingFile))
                using (var rdr = new Mp3FileReader(ms))
                using (var wavStream = WaveFormatConversionStream.CreatePcmStream(rdr))
                using (var baStream = new BlockAlignReductionStream(wavStream))
                using (var waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))

                {
                    Console.WriteLine("Now Playing:" + matchingFile);
                    waveOut.Init(baStream);
                    waveOut.Play();
                    while (waveOut.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            FindMp3();
            return matchingFile; // This will be null if no matching file is found
        }
    }
}
