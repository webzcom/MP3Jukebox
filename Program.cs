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
            string userVolume = "0.21";
            string driveLetter = "M:\\";
            string tempDriveLetter = "";            
            Console.WriteLine("MP3 Finder by Cyber Abyss running as" + Environment.UserName);
            Console.WriteLine("Enter the Volume: Range is float from 0.10 to 1.0 (Defaut: " + userVolume + ")");
            userVolume = Console.ReadLine();
            Console.WriteLine("Enter the Drive Letter to Search:");
            tempDriveLetter = Console.ReadLine();
            if (!string.IsNullOrEmpty(tempDriveLetter)) {
                tempDriveLetter = tempDriveLetter + ":\\";
                driveLetter = tempDriveLetter;
            }

            //Create an AudioFile Object that can hold all the data we need as we pass it thru the methods
            AudioFile audioFile = new AudioFile();
            audioFile.Volume = userVolume;
            audioFile.DriveLetter = driveLetter;
            audioFile.CurrentUser = Environment.UserName;
            audioFile.UserHomeFolder = "C:\\Users\\" + Environment.UserName + "\\"; 
            FindMp3(audioFile);
        }

        public static string FindMp3(AudioFile audioFile)
        {            
            // Example usage
            string searchText = "";
            string searchParameter = searchText;
            string folderPath = audioFile.DriveLetter;

            //Prompt user for a Category and Topic
            Console.WriteLine("Hit Enter for Random Song or Enter Your Music Search Text:");
            searchText = Console.ReadLine();
            if (searchText == "")
            {
                //SearchFile(folderPath, "", tempAudioFile.Volume);
                SearchFile(audioFile);
            }

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

            if (!string.IsNullOrEmpty(matchingFile))
            {
                //using (var ms = File.OpenRead("c:\\temp\\insane.mp3"))
                using (var ms = File.OpenRead(matchingFile))
                using (var rdr = new Mp3FileReader(ms))
                using (var wavStream = WaveFormatConversionStream.CreatePcmStream(rdr))
                using (var baStream = new BlockAlignReductionStream(wavStream))
                using (var waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))

                {
                    Console.WriteLine("Now Playing:" + matchingFile);
                    Console.WriteLine(waveOut.Volume.ToString());
                    waveOut.Init(baStream);
                    waveOut.Play();                    
                    while (waveOut.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(100);
                    }
                }
            }

            return matchingFile; // This will be null if no matching file is found
        }

        static void PlayMP3(AudioFile audioFile) {
            //Play MP3
            using (var ms = File.OpenRead(audioFile.AudioFileCollection[audioFile.RandomNumber]))
            using (var rdr = new Mp3FileReader(ms))
            using (var wavStream = WaveFormatConversionStream.CreatePcmStream(rdr))
            using (var baStream = new BlockAlignReductionStream(wavStream))
            using (var waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))

            {
                float mp3Volume = 0.15f;
                if (audioFile.Volume != "")
                {
                    mp3Volume = float.Parse(audioFile.Volume);
                }
                
                Console.WriteLine("Now Playing:" + audioFile.AudioFileCollection[audioFile.RandomNumber]);
                waveOut.Init(baStream);
                waveOut.Volume = mp3Volume;
                Console.WriteLine("Volume: " + waveOut.Volume.ToString());
                waveOut.Play();
                while (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(100);
                }

                Console.WriteLine("Press ESC to stop");
                do
                {
                    while (!Console.KeyAvailable)
                    {
                        // Do something
                        SearchFile(audioFile);
                    }
                } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
                
            }
            
        }       

        static void SearchFile(AudioFile audioFile)
        {
            bool fileFound = false;
            //int iCounter = 0;
            
            try
            {
                // Get all files in the directory and subdirectories.
                string[] files = Directory.GetFiles(audioFile.DriveLetter, "*.mp3", SearchOption.AllDirectories);
                audioFile.AudioFileCollection = files;
                if (files.Length > 0)
                {
                    // Create an instance of the Random class
                    Random random = new Random();
                    // Generate a random number between 1 and 100
                    int randomNumber = random.Next(1, files.Length); // The upper limit is exclusive, so we use 101

                    fileFound = true;
                    Console.WriteLine(files.Length + " MP3 files found!");
                    Console.WriteLine("Pickin a Rando # " + randomNumber);
                    audioFile.RandomNumber = randomNumber;
                    PlayMP3(audioFile);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("You do not have permission to access one or more directories.");
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("The specified directory does not exist.");
            }

            if (!fileFound)
            {
                Console.WriteLine("No files found matching the specified name.");
            }
        }
    }
}
