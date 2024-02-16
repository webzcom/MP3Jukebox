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
            Console.ForegroundColor = ConsoleColor.Green;
            bool IsInAutoPlayMode = false;
            string userVolume = "";
            string defaultVolume = "0.05";
            string searchText = "";
            Console.WriteLine("MP3 Jukebox by Cyber Abyss running as " + Environment.UserName);
            Console.WriteLine("Enter the Volume: Range is float from 0.10 to 1.0 (Default: " + defaultVolume + ")");
            Console.ForegroundColor = ConsoleColor.Blue;
            userVolume = Console.ReadLine();
            if (string.IsNullOrEmpty(userVolume)) {
                userVolume = defaultVolume;
            };

            //Create an AudioFile Object that can hold all the data we need as we pass it thru the methods
            AudioFile audioFile = new AudioFile();
            audioFile.IsInAutoPlayMode = IsInAutoPlayMode;
            audioFile.AutoPlayCounter = 0;
            audioFile.CustomCollectionCounter = 0;
            audioFile.Volume = userVolume;
            //audioFile.DriveLetter = driveLetter;
            audioFile.CurrentUser = Environment.UserName;
            audioFile.UserHomeFolder = "C:\\Users\\" + Environment.UserName + "\\";
            GetDriveLetter(audioFile);
            //Prompt user for a Category and Topic
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Hit Enter for Random Song or Enter Your Music Search Text:");
            Console.ForegroundColor = ConsoleColor.Blue;
            searchText = Console.ReadLine();
            audioFile.SearchTerm = searchText;
            SearchFile(audioFile);
        }


        public static void GetDriveLetter(AudioFile audioFile){
            string tempDriveLetter = "";

            if (string.IsNullOrEmpty(audioFile.DriveLetter)){
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Enter the Drive Letter to Search:");
                Console.ForegroundColor = ConsoleColor.Blue;
                tempDriveLetter = Console.ReadLine();
                if (string.IsNullOrEmpty(tempDriveLetter) || tempDriveLetter.Length > 1)
                {
                    Console.WriteLine("Your Drive Letter Doesn't Look Right, Try Again. Single Character Only Please.");
                    GetDriveLetter(audioFile);
                }
                else
                {
                    audioFile.DriveLetter = tempDriveLetter + ":\\";
                    return;
                }
            }                 
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
                    audioFile.IsInAutoPlayMode = true;
                }

                if (audioFile.AudioFileCollection.Length > 0)
                {
                    // Create an instance of the Random class
                    Random random = new Random();
                    // Generate a random number between 1 and 100
                    int randomNumber = random.Next(1, audioFile.AudioFileCollection.Length); // The upper limit is exclusive, so we use 101

                    fileFound = true;
                    if (audioFile.AutoPlayCounter < 2) {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(audioFile.AudioFileCollection.Length + " MP3 files found!");
                    }
                    
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

                Console.WriteLine("Volume: " + waveOut.Volume.ToString());
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Now Playing:");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(audioFile.AudioFileCollection[tempCounter]);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("By: " + MetadataExtractor.GetAlbumArtist(audioFile.AudioFileCollection[tempCounter]));
                Console.WriteLine("Length: " + rdr.TotalTime.ToString());
                waveOut.Init(baStream);
                waveOut.Volume = mp3Volume;
                
                waveOut.Play();

                if (audioFile.IsInAutoPlayMode) {
                    audioFile.AutoPlayCounter = audioFile.AutoPlayCounter + 1;
                }

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
