using System;
using System.IO;
using System.Threading;
using NAudio.Wave;
using System.Linq;
using TagLib;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MP3Jukebox
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        static void Main(string[] args)
        {
            //Force focus on current window
           // BringConsoleToFront();
            //Get system info
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            Console.ForegroundColor = ConsoleColor.Green;
            bool IsInAutoPlayMode = true;
            string searchText = "";
            Console.WriteLine("*****************************************************************");
            Console.WriteLine("*  MP3 Jukebox by Cyber Abyss running as " + Environment.UserName);
            Console.WriteLine("*****************************************************************");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Left Arrow: Pause/Play Song");
            Console.WriteLine("Right Arrow: Skip Song");
            Console.WriteLine("Volume Down: Arrow Down");
            Console.WriteLine("Volume Up: Arrow Up");
            Console.WriteLine("Press C to Cancel & Goto Main Menu");
            //Create an AudioFile Object that can hold all the data we need as we pass it thru the methods
            AudioFile audioFile = new AudioFile();
            //Put the list of available drives into our object as an array
            audioFile.DrivesAvailable = DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.Network || x.DriveType == DriveType.Fixed || x.DriveType == DriveType.Removable).Select(d => d.Name).ToArray();
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
            //Get system info
            DriveInfo[] drives = DriveInfo.GetDrives();

            if (string.IsNullOrEmpty(audioFile.DriveLetter))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("> Enter the Drive Letter to Search:");
                Console.ForegroundColor = ConsoleColor.Blue;
                tempDriveLetter = Console.ReadLine().ToUpper();

                if (string.IsNullOrEmpty(tempDriveLetter) || tempDriveLetter.Length > 1)
                {
                    Console.WriteLine("> Your Drive Letter Doesn't Look Right, Try Again. Single Character Only Please.");
                    GetDriveLetter(audioFile);
                }
                else
                {
                    tempDriveLetter = tempDriveLetter + ":\\";
                    bool exists = Array.Exists(audioFile.DrivesAvailable, element => element == tempDriveLetter);
                    if (exists)
                    {
                        audioFile.DriveLetter = tempDriveLetter;
                        return;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("That drive letter is not available.");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Try one of these: ");
                        // Iterate through each drive and print its name
                        foreach (DriveInfo drive in drives)
                        {
                            Console.Write(drive.Name + " ");
                        }
                        Console.WriteLine();
                        GetDriveLetter(audioFile);
                    }

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
                        Console.WriteLine(audioFile.AudioFileCollection.Length + " MP3 files found! Press Any Key to Continue");
                    }

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



        static void PlayMP3File(AudioFile audioFile) {
            string mP3Artist = MetadataExtractor.GetAlbumArtist(audioFile.CurrentMP3);  

            using (var ms = System.IO.File.OpenRead(audioFile.CurrentMP3))
            using (var rdr = new Mp3FileReader(ms))
            using (var wavStream = WaveFormatConversionStream.CreatePcmStream(rdr))
            using (var baStream = new BlockAlignReductionStream(wavStream))
            using (var waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
            {
                string currentSongLength = rdr.TotalTime.ToString();
                waveOut.Init(baStream);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("******************************************************************");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Now Playing @ Volume: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(audioFile.Volume.ToString());
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(audioFile.CurrentMP3);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("By: " + mP3Artist);
                Console.WriteLine("Length: " + currentSongLength);
                audioFile.LastFilePlayedLength = currentSongLength;
                waveOut.Play();
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

                        if (keyInfo.Key == ConsoleKey.C)
                        {
                            //Console.Clear();
                            waveOut.Stop();
                            ms.Close();
                            rdr.Close();
                            wavStream.Close();
                            baStream.Close();
                            waveOut.Dispose();
                            Main(null);
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
                            ms.Close();
                            rdr.Close();
                            wavStream.Close();
                            baStream.Close();
                            waveOut.Dispose();
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

                            if (waveOut.PlaybackState == 0)
                            {
                                audioFile.EndOfFile = true;
                                //SearchFile(audioFile);
                                SendKeys.SendWait("{RIGHT}");
                                //PlayMP3(audioFile);
                            }

                        }
                    } while (keyInfo.Key != ConsoleKey.Spacebar);

                while (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(100);
                }
                PlayMP3(audioFile);
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

            audioFile.CurrentMP3 = audioFile.AudioFileCollection[tempCounter].ToString();

            using (var ms = System.IO.File.OpenRead(audioFile.AudioFileCollection[tempCounter]))

            { 
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


                PlayMP3File(audioFile);
                //waveOut.Play();
                audioFile.EndOfFile = false;
                ConsoleKeyInfo keyInfo;
 

            }
        }

            static void BringConsoleToFront()
            {
                IntPtr handle = GetConsoleWindow();
                if (handle != IntPtr.Zero)
                {
                    SetForegroundWindow(handle);
                    Console.WriteLine("We have focus!");

                };
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
