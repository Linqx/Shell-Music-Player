using System;
using System.IO;
using System.Linq;
using WMPLib;

namespace ShellMusicPlayer
{
    class Program
    { 
        #region Private        

        private static DiscordManager Discord;
        private static CommandHandler CommandHandler;

        private static WindowsMediaPlayer SoundPlayer;                
        private static Random Random = new Random(Environment.TickCount);
        
        private static bool ContinueRandom = false;
        private static bool WaitingForReady = false;
        private static bool Paused = false;
        private static string Filter = "";

        #endregion

        #region Public

        public static string CurrentDirectory = null;
        public static bool Running = true;

        #endregion

        #region Main

        static void Main(string[] args)
        {
            Console.Title = "Danny's Music Player";

            Discord = new DiscordManager();
            CommandHandler = new CommandHandler();

            SetDiscord();

            SoundPlayer = new WindowsMediaPlayer();
            SoundPlayer.PlayStateChange += SoundPlayer_PlayStateChange;

            while (Running)
                CommandHandler.Handle(Console.ReadLine());
        }

        #endregion

        #region Discord

        public static void SetDiscord() => Discord.SetDiscord();

        public static void UpdatePresence()
        {
            if (!string.IsNullOrEmpty(Filter))
                Discord.SmallImageText = $"Filter: {Filter}";
            else
                Discord.SmallImageText = "No Filter Set";

            Discord.UpdatePresence();
        }

        #endregion

        #region Functions

        public static void List()
        {
            if (!Directory.Exists(CurrentDirectory))
            {
                Console.WriteLine("The current direction does not exist! Please set the directory and try again");
                return;
            }

            var songs = Directory.EnumerateFiles(CurrentDirectory, "*.mp3", SearchOption.AllDirectories).ToArray();

            if (!string.IsNullOrEmpty(Filter))
                songs = songs.Where(_ => _.IndexOf(Filter, StringComparison.CurrentCultureIgnoreCase) != -1).ToArray();

            foreach (var i in songs)
                Console.WriteLine(Path.GetFileName(i));

            Console.WriteLine($"{songs.Length} songs found");
        }

        public static void SetFilter(string[] parameters)
        {
            if (!Directory.Exists(CurrentDirectory))
            {
                Console.WriteLine("The current direction does not exist! Please set the directory and try again");
                return;
            }

            if (!string.IsNullOrEmpty(Filter) && parameters.Length == 0 || (parameters.Length == 1 && parameters[0] == ""))
            {
                Filter = "";
                Console.WriteLine("Filter removed!");

                UpdatePresence();
                return;
            }

            if (parameters.Length == 0)
            {
                Console.WriteLine("This command needs at least 1 parameter");
                return;
            }

            var songs = Directory.EnumerateFiles(CurrentDirectory, "*.mp3", SearchOption.AllDirectories).ToArray();

            if (!string.IsNullOrEmpty(string.Join(" ", parameters)))
                songs = songs.Where(_ => _.IndexOf(string.Join(" ", parameters), StringComparison.CurrentCultureIgnoreCase) != -1).ToArray();

            if (songs.Length > 0)
                Filter = string.Join(" ", parameters);
            else
                Console.WriteLine($"There are no songs that contain the filter: {string.Join(" ", parameters)}. Filter not set");

            UpdatePresence();
        }

        public static void ShowFilter()
        {
            if (!string.IsNullOrEmpty(Filter))
                Console.WriteLine($"The current filter is: {Filter}");
            else
                Console.WriteLine("There is no filter set");
        }

        public static void PlayRandom()
        {
            if (!Directory.Exists(CurrentDirectory))
            {
                Console.WriteLine("The current direction does not exist! Please set the directory and try again");
                return;
            }

            Paused = false;

            var songs = Directory.EnumerateFiles(CurrentDirectory, "*.mp3", SearchOption.AllDirectories).ToArray();

            if (!string.IsNullOrEmpty(Filter))
                songs = songs.Where(_ => _.IndexOf(Filter, StringComparison.CurrentCultureIgnoreCase) != -1).ToArray();

            var song = songs[Random.Next(songs.Length)];

            ContinueRandom = true;
            SoundPlayer.URL = song;
            SoundPlayer.controls.play();

            if (DiscordManager.Enabled)
            {
                Discord.LargeImageText = Path.GetFileNameWithoutExtension(song);
                Discord.State = Path.GetFileNameWithoutExtension(song);
                Discord.Details = "Listening To:";
                Discord.StartTimeStamp = DateTimeOffset.Now.ToUnixTimeSeconds();

                UpdatePresence();
            }
        }

        public static void Play(string resource)
        {
            if (!Directory.Exists(CurrentDirectory))
            {
                Console.WriteLine("The current direction does not exist! Please set the directory and try again");
                return;
            }

            string dir = $"{CurrentDirectory}\\{resource}";

            if (File.Exists(dir))
            {               
                Paused = false;
                ContinueRandom = false;

                SoundPlayer.URL = dir;
                SoundPlayer.controls.play();

                if (DiscordManager.Enabled)
                {
                    Discord.LargeImageText = Path.GetFileNameWithoutExtension(dir);
                    Discord.State = Path.GetFileNameWithoutExtension(dir);
                    Discord.Details = "Listening To:";
                    Discord.StartTimeStamp = DateTimeOffset.Now.ToUnixTimeSeconds();

                    UpdatePresence();
                }
            }
            else
                Console.WriteLine($"File: {CurrentDirectory}\\{resource} was not found!");
        }

        public static void Pause()
        {
            if (DiscordManager.Enabled)
                Discord.Pause();

            Paused = true;

            if (SoundPlayer.playState == WMPPlayState.wmppsPlaying)
                SoundPlayer.controls.pause();
        }


        public static void Resume()
        {
            if (DiscordManager.Enabled)
                Discord.Resume();

            if (SoundPlayer.playState == WMPPlayState.wmppsPaused)
                SoundPlayer.controls.play();
        }

        #endregion

        #region Events

        private static void SoundPlayer_PlayStateChange(int NewState)
        {
            //Console.WriteLine($"Play State Changed: {((WMPPlayState)NewState).ToString()}");

            if (NewState == (int)WMPPlayState.wmppsPlaying && !Paused)
                Console.WriteLine($"Now playing: {Path.GetFileName(SoundPlayer.URL)} ({SoundPlayer.controls.currentItem.durationString})");

            if (NewState == (int)WMPPlayState.wmppsMediaEnded && ContinueRandom)
            {
                Paused = false;
                WaitingForReady = true;
                Console.WriteLine($"{Path.GetFileName(SoundPlayer.URL)} has ended");
            }
            else if (WaitingForReady && NewState == (int)WMPPlayState.wmppsStopped && ContinueRandom)
            {
                WaitingForReady = false;

                Action action = () => PlayRandom();
                action.BeginInvoke(action.EndInvoke, null);
            }
        }

        #endregion
    }
}
