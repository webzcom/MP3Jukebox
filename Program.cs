using System;
using System.IO;
using System.Threading;
using NAudio.Wave;
using System.Linq;
using TagLib;

namespace MP3Jukebox
{
    class Program
    {
        static void Main(string[] args) 
        {
            string userVolume = "";
            string defaultVolume = "0.05";
            string driveLetter = "M:\\";
            string tempDriveLetter = "";
            string searchText = "";
            Console.WriteLine("MP3 Jukebox by Cyber Abyss running as " + Environment.UserName);
            Console.WriteLine("Enter the Volume: Range is float from 0.10 to 1.0 (Default: " + defaultVolume + ")");
            userVolume = Console.ReadLine();
            if (string.IsNullOrEmpty(userVolume)) {
                userVolume = defaultVolume;
            }
                   
            Console.WriteLine("Enter the Drive Letter to Search:");
            tempDriveLetter = Console.ReadLine();
            if (!string.IsNullOrEmpty(tempDriveLetter)) {
                tempDriveLetter = tempDriveLetter + ":\\";
                driveLetter = tempDriveLetter;
            }

            //Create an AudioFile Object that can hold all the data we need as we pass it thru the methods
            AudioFile audioFile = new AudioFile();
            audioFile.CustomCollectionCounter = 0;
            audioFile.Volume = userVolume;
            audioFile.DriveLetter = driveLetter;
            audioFile.CurrentUser = Environment.UserName;
            audioFile.UserHomeFolder = "C:\\Users\\" + Environment.UserName + "\\";
            //Prompt user for a Category and Topic
            Console.WriteLine("Hit Enter for Random Song or Enter Your Music Search Text:");
            searchText = Console.ReadLine();
            audioFile.SearchTerm = searchText;
            SearchFile(audioFile);
        }


        static void SearchFile(AudioFile audioFile)
        {
            bool fileFound = false;

            try
            {
                // Get all files in the directory and subdirectories.
                if (audioFile.AudioFileCollection == null || audioFile.AudioFileCollection.Length == 0)
                {
                    string[] files = Directory.GetFiles(audioFile.DriveLetter, "*.mp3", SearchOption.AllDirectories);
                    audioFile.AudioFileCollection = files;
                }
                if (!string.IsNullOrEmpty(audioFile.SearchTerm))
                {
                    string[] files = Directory.GetFiles(audioFile.DriveLetter, "*" + audioFile.SearchTerm + "*.mp3", SearchOption.AllDirectories);
                    audioFile.AudioFileCollection = files;
                }

                if (audioFile.AudioFileCollection.Length > 0)
                {
                    // Create an instance of the Random class
                    Random random = new Random();
                    // Generate a random number between 1 and 100
                    int randomNumber = random.Next(1, audioFile.AudioFileCollection.Length); // The upper limit is exclusive, so we use 101

                    fileFound = true;
                    Console.WriteLine(audioFile.AudioFileCollection.Length + " MP3 files found!");
                    //Console.WriteLine("Pickin a Rando # " + randomNumber);
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
                using (var ms = System.IO.File.OpenRead(matchingFile))
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
            int tempCounter = 0;
    
            //Play MP3
            //if search term was used, don't use the random number
            if (!string.IsNullOrEmpty(audioFile.SearchTerm))
            {
                tempCounter = audioFile.CustomCollectionCounter;
            }
            else
            {
                tempCounter = audioFile.RandomNumber - 1;
            }

            using (var ms = System.IO.File.OpenRead(audioFile.AudioFileCollection[tempCounter]))
            using (var rdr = new Mp3FileReader(ms))
            using (var wavStream = WaveFormatConversionStream.CreatePcmStream(rdr))
            using (var baStream = new BlockAlignReductionStream(wavStream))
            using (var waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))

            {
                float mp3Volume = 0.05f;
                if (!string.IsNullOrEmpty(audioFile.Volume))
                {
                    mp3Volume = float.Parse(audioFile.Volume);
                }
                else {
                    mp3Volume = 0.05f;
                }


                Console.WriteLine("Now Playing:" + audioFile.AudioFileCollection[tempCounter]);
                Console.WriteLine(MetadataExtractor.GetAlbumArtist(audioFile.AudioFileCollection[tempCounter]));
                waveOut.Init(baStream);
                waveOut.Volume = mp3Volume;
                Console.WriteLine("Volume: " + waveOut.Volume.ToString());
                waveOut.Play();
                while (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(100);
                }

                //If we hit the end of the custom collection return the main menu and start again
                if (audioFile.CustomCollectionCounter + 1 == audioFile.AudioFileCollection.Length) {
                    Main(null);
                }
                    

                //Increment the counter if we are using a custom colleciton
                if (!string.IsNullOrEmpty(audioFile.SearchTerm))
                {
                    audioFile.CustomCollectionCounter = audioFile.CustomCollectionCounter + 1;
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


        public class MetadataExtractor
        {
            public static string GetAlbumArtist(string filePath)
            {
                try
                {
                    // Load the file
                    var file = TagLib.File.Create(filePath);

                    // Check if album artists array is not null or empty
                    if (file.Tag.AlbumArtists != null && file.Tag.AlbumArtists.Length > 0)
                    {
                        // Return the first album artist found
                        return file.Tag.AlbumArtists[0];
                    }
                    else
                    {
                        // No album artist found, return a default message or handle accordingly
                        return "Album artist not found";
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions (e.g., file not found, access denied)
                    Console.WriteLine($"Error reading file: {ex.Message}");
                    return "Error retrieving album artist";
                }
            }
        }


    }
}
