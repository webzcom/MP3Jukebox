using System;
using System.IO;
using System.Threading;
using NAudio.Wave;
using System.Linq;
using TagLib;
using System.Windows.Forms;

namespace MP3Jukebox
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            bool IsInAutoPlayMode = true;
            string searchText = "";
            Console.WriteLine("*****************************************************************");
            Console.WriteLine("*  MP3 Jukebox by Cyber Abyss running as " + Environment.UserName);
            Console.WriteLine("*****************************************************************");
            //Create an AudioFile Object that can hold all the data we need as we pass it thru the methods
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Left Arrow: Pause/Play Song");
            Console.WriteLine("Right Arrow: Skip Song");
            Console.WriteLine("Volume Down: Arrow Down");
            Console.WriteLine("Volume Up: Arrow Up");
            AudioFile audioFile = new AudioFile();
            audioFile.IsPlaying = false;
            audioFile.IsInAutoPlayMode = IsInAutoPlayMode;
            audioFile.AutoPlayCounter = 1;
            audioFile.CustomCollectionCounter = 0;
            //audioFile.DriveLetter = driveLetter;
            audioFile.CurrentUser = Environment.UserName;
            audioFile.UserHomeFolder = "C:\\Users\\" + Environment.UserName + "\\";
            GetVolume(audioFile);
            GetDriveLetter(audioFile);
            //Prompt user for a Category and Topic
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("> Hit Enter for Random Song or Search Term for Custom Playlist:");
            Console.ForegroundColor = ConsoleColor.Blue;
            searchText = Console.ReadLine();
            audioFile.SearchTerm = searchText;
            SearchFile(audioFile);
        }

        public static void CheckVolume(AudioFile audioFile)
        {
            if (audioFile.Volume > 1.00f)
            {
                audioFile.Volume = 1.00f;
            }
            else if (audioFile.Volume < 0.00f)
            {
                audioFile.Volume = 0.00f;
            }
        }


        public static void GetVolume(AudioFile audioFile)
        {
            float defaultVolume = 0.66f;
            audioFile.Volume = defaultVolume;
            string userVolume = "";

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("> Hit Enter for Default Volume: " + defaultVolume.ToString() + ". Range is float from 0.10 to 1.0");
            userVolume = Console.ReadLine();

            if (string.IsNullOrEmpty(userVolume))
            {
                audioFile.Volume = defaultVolume;
            }
            else
            {
                try
                {
                    audioFile.Volume = float.Parse(userVolume);
                    if (audioFile.Volume > 1.00)
                    {

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Setting to Max Value: 1.0");
                        Console.ForegroundColor = ConsoleColor.Blue;
                        //GetVolume(audioFile);
                        audioFile.Volume = 1.00f;
                    }
                }
                catch (Exception)
                {
                    //throw;
                    GetVolume(audioFile);
                }

            }

        }

        public static void GetDriveLetter(AudioFile audioFile)
        {
            string tempDriveLetter = "";

            if (string.IsNullOrEmpty(audioFile.DriveLetter))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("> Enter the Drive Letter to Search:");
                Console.ForegroundColor = ConsoleColor.Blue;
                tempDriveLetter = Console.ReadLine();
                if (string.IsNullOrEmpty(tempDriveLetter) || tempDriveLetter.Length > 1)
                {
                    Console.WriteLine("> Your Drive Letter Doesn't Look Right, Try Again. Single Character Only Please.");
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
                }

                if (audioFile.AudioFileCollection.Length > 0)
                {

                    fileFound = true;
                    if (audioFile.AutoPlayCounter < 2)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(audioFile.AudioFileCollection.Length + " MP3 files found!");
                    }

                    //Console.WriteLine("Pickin a Rando # " + randomNumber);
                    //audioFile.RandomNumber = randomNumber;
                    audioFile.IsPlaying = true;
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


        static void PlayMP3(AudioFile audioFile)
        {
            audioFile.EndOfFile = false;
            int tempCounter = 0;

            // Create an instance of the Random class
            Random random = new Random();
            // Generate a random number between 1 and 100
            int randomNumber = random.Next(1, audioFile.AudioFileCollection.Length); // The upper limit is exclusive, so we use 101
            audioFile.RandomNumber = randomNumber;


            //Play MP3
            //if search term was used, don't use the random number
            if (!string.IsNullOrEmpty(audioFile.SearchTerm))
            {
                tempCounter = audioFile.CustomCollectionCounter;
                if (tempCounter == audioFile.AudioFileCollection.Length)
                {
                    Main(null);
                }
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

                if (audioFile.AutoPlayCounter < 2)
                {
                    //Console.WriteLine("Volume: " + waveOut.Volume.ToString());
                    Console.WriteLine("Volume: " + audioFile.Volume.ToString());
                }

                waveOut.Init(rdr);
                //waveOut.PlaybackStopped += (sender, e) =>
                //{
                //    // This event is triggered when playback is stopped
                //    Console.WriteLine($"{ms} playback stopped.");
                //    PlayMP3(audioFile, audioFile.AudioAction);
                //    //SearchFile(audioFile);
                //    onPlaybackStopped?.Invoke();
                //};

                string mP3Artist = MetadataExtractor.GetAlbumArtist(audioFile.AudioFileCollection[tempCounter]);
                string currentSongLength = rdr.TotalTime.ToString();

                waveOut.Init(baStream);
                waveOut.Volume = audioFile.Volume;

                //Skip the song if we've just heard it, maybe come back and update this to an array
                if (audioFile.LastFilePlayedLength == currentSongLength)
                {
                    //Skip this song
                    waveOut.Stop();
                    Console.WriteLine("Skipping Duplicate Song!");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("******************************************************************");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Now Playing:" + audioFile.Volume.ToString());
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(audioFile.AudioFileCollection[tempCounter]);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("By: " + mP3Artist);
                    Console.WriteLine("Length: " + currentSongLength);
                    //waveOut.Play();
                    audioFile.LastFilePlayedLength = currentSongLength;
                }


                //if (audioFile.IsInAutoPlayMode) {
                //    audioFile.AutoPlayCounter = audioFile.AutoPlayCounter + 1;
                //}

                audioFile.AutoPlayCounter = audioFile.AutoPlayCounter + 1;

                //If we hit the end of the custom collection return the main menu and start again
                if (audioFile.CustomCollectionCounter == audioFile.AudioFileCollection.Length)
                {
                    Main(null);
                }

                //Increment the counter if we are using a custom colleciton
                if (!string.IsNullOrEmpty(audioFile.SearchTerm))
                {
                    audioFile.CustomCollectionCounter = audioFile.CustomCollectionCounter + 1;
                }

                while (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(100);
                    Console.Write(waveOut.GetPosition().ToString());

                }

                waveOut.Play();
                audioFile.EndOfFile = false;
                ConsoleKeyInfo keyInfo;
                do
                {
                    keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.UpArrow)
                    {
                        audioFile.Volume = audioFile.Volume + (audioFile.Volume * 0.10f);
                        CheckVolume(audioFile);
                        //Console.WriteLine("Volume Increased to " + audioFile.Volume.ToString());
                        waveOut.Volume = audioFile.Volume;
                    }

                    if (keyInfo.Key == ConsoleKey.DownArrow)
                    {
                        audioFile.Volume = audioFile.Volume - (audioFile.Volume * 0.10f);
                        CheckVolume(audioFile);
                        waveOut.Volume = audioFile.Volume;    
                    }

                    //Pause Play Key
                    if (keyInfo.Key == ConsoleKey.LeftArrow)
                    {
                        if (audioFile.IsPlaying)
                        {
                            audioFile.IsPlaying = false;
                        }
                        else
                        {
                            audioFile.IsPlaying = true;
                        }
                    }

                    //Skip Key
                    if (keyInfo.Key == ConsoleKey.RightArrow)
                    {
                        waveOut.Stop();
                        audioFile.IsPlaying = false;
                        SearchFile(audioFile);
                        //PlayMP3(audioFile);
                    }


                    while (!Console.KeyAvailable)
                    {
                        // Do something
                        if (audioFile.IsPlaying)
                        {
                            waveOut.Play();
                            audioFile.IsStopped = false;
                        }
                        else
                        {
                            waveOut.Pause();
                        }

                        if (waveOut.PlaybackState == 0) {
                            audioFile.EndOfFile = true;
                            //SearchFile(audioFile);
                            //SendKeys.SendWait("{RIGHT}");
                            PlayMP3(audioFile);
                        }

                    }
                } while (keyInfo.Key != ConsoleKey.Spacebar);
                //while (Console.ReadKey(true).Key != ConsoleKey.Spacebar);

                waveOut.Stop();
                SearchFile(audioFile);
                //PlayMP3(audioFile);

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
