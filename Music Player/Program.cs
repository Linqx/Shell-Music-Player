// --------------------------------------------------------------------
//                             Program.cs
// --------------------------------------------------------------------
// Author: Danny Guardado (Linqx)
// Created: 09/01/2020

using System;
using System.IO;
using System.Linq;
using System.Text;
using KirokuLogging;
using WMPLib;

namespace ShellMusicPlayer
{
	internal class Program
	{

		private static DiscordManager Discord;
		private static CommandHandler CommandHandler;

		private static WindowsMediaPlayer SoundPlayer;
		private static readonly Random Random = new Random(Environment.TickCount);

		private static string Title => $"Danny's Music Player - {CurrentSong}";
		private static string Title_Paused => $"Danny's Music Player - Paused";
		private static bool ContinueRandom;
		private static bool WaitingForReady;
		private static string Filter = "";
		private static string CurrentPosition = "";
		private static string Tag = "";
		
		public static string CurrentSong = "";
		public static string CurrentSongPath = "";
		public static string CurrentDirectory = null;
		public static bool Running = true;

		private static void Main(string[] args)
		{
			Console.Title = "Danny's Music Player";
			Console.OutputEncoding = Encoding.UTF8;

			Kiroku.Log("Music Player", ConsoleColor.DarkCyan, 
				"Started with Discord Presence: Enabled", ConsoleColor.Cyan);
			
			Discord = new DiscordManager();
			CommandHandler = new CommandHandler();

			SoundPlayer = new WindowsMediaPlayer();
			SoundPlayer.PlayStateChange += SoundPlayer_PlayStateChange;

			TagsManager.Initialize();
			
			SetDiscord();
			SetVolume(10);

			while (Running)
			{
				try
				{
					CommandHandler.Handle(Console.ReadLine());
				}
				catch (Exception ex)
				{
					Kiroku.Log("Failure", ConsoleColor.DarkRed, ex, ConsoleColor.Red);
				}
			}
		}


		public static void SetDiscord()
		{
			Discord.SetDiscord();
		}

		public static void UpdatePresence()
		{
			if (!string.IsNullOrEmpty(Filter))
			{
				Discord.SmallImageText = $"Filter: {Filter}";
			}
			else
			{
				Discord.SmallImageText = "No Filter Set";
			}

			Discord.UpdatePresence();
		}

		public static void List()
		{
			if (!Directory.Exists(CurrentDirectory))
			{
				Kiroku.Log("Commands.List", ConsoleColor.DarkYellow,
					"There is no directory selected. Please select a directory first and try again", ConsoleColor.Yellow);
				return;
			}

			string[] songs = GetSongs();

			foreach (string i in songs)
			{
				Kiroku.Log(null, ConsoleColor.White,
					Path.GetFileName(i), ConsoleColor.Gray);
			}

			Kiroku.Log("Commands.List", ConsoleColor.White,
				$"{songs.Length} songs found", ConsoleColor.Gray);
		}

		public static void SetFilter(string[] parameters)
		{
			if (!Directory.Exists(CurrentDirectory))
			{
				Kiroku.Log("Commands.Filter", ConsoleColor.DarkYellow,
					"There is no directory selected. Please select a directory first and try again", ConsoleColor.Yellow);
				return;
			}

			if (!string.IsNullOrEmpty(Filter) && parameters.Length == 0 ||
			    parameters.Length == 1 && parameters[0] == "")
			{
				Kiroku.Log("Commands.Filter", ConsoleColor.White,
					$"Filter: {Filter} was removed.", ConsoleColor.Gray);
				
				Filter = "";
				UpdatePresence();
				return;
			}

			if (parameters.Length == 0)
			{
				Kiroku.Log("Commands.Filter", ConsoleColor.DarkYellow,
					$"To set a filter, a parameter is required.", ConsoleColor.Yellow);
				return;
			}

			string filter = string.Join(" ", parameters);
			string[] songs = GetSongs(filter);

			if (songs.Length > 0)
			{
				Filter = filter;
				
				Kiroku.Log("Commands.Filter", ConsoleColor.DarkGreen, 
					$"Filter set to: {Filter}", ConsoleColor.Green);
			}
			else
			{
				Kiroku.Log("Commands.Filter", ConsoleColor.DarkYellow,
					$"No songs were found that contain the filter: {string.Join(" ", parameters)}. Filter was not set.", ConsoleColor.Yellow);
			}

			UpdatePresence();
		}

		public static void ShowFilter()
		{
			if (!string.IsNullOrEmpty(Filter))
			{
				Kiroku.Log("Commands.ShowFilter", ConsoleColor.White,
					$"The current filter is: {Filter}", ConsoleColor.Gray);
			}
			else
			{
				Kiroku.Log("Commands.ShowFilter", ConsoleColor.White,
					$"There is currently no filter.", ConsoleColor.Gray);
			}
		}

		public static void PlayRandom()
		{
			if (string.IsNullOrWhiteSpace(Tag) && !Directory.Exists(CurrentDirectory))
			{
				Kiroku.Log("Commands.Random", ConsoleColor.DarkYellow,
					"There is no directory or tag selected. Please select a directory or tag first and try again", ConsoleColor.Yellow);
				return;
			}

			string[] songs = GetSongs();
			string song = songs[Random.Next(songs.Length)];

			ContinueRandom = true;
			
			PlaySong(song);
		}

		public static void Play(string resource)
		{
			if (!Directory.Exists(CurrentDirectory))
			{
				Kiroku.Log("Commands.Play", ConsoleColor.DarkYellow,
					"There is no directory selected. Please select a directory first and try again", ConsoleColor.Yellow);
				return;
			}

			string dir = $"{CurrentDirectory}\\{resource}";
			if (File.Exists(dir))
			{
				ContinueRandom = false;
				
				PlaySong(dir);
			}
			else
			{
				Kiroku.Log("Commands.Play", ConsoleColor.DarkRed,
					$"File: {CurrentDirectory}\\{resource} was not found!", ConsoleColor.Red);
			}
		}

		private static void PlaySong(string song)
		{
			SoundPlayer.URL = song;

			CurrentPosition = "";
			SoundPlayer.controls.play();

			CurrentSong = Path.GetFileNameWithoutExtension(SoundPlayer.URL);
			CurrentSongPath = SoundPlayer.URL;
			Console.Title = Title;	
			
			if (DiscordManager.Enabled)
			{
				Discord.LargeImageText = Path.GetFileNameWithoutExtension(song);
				Discord.State = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Path.GetFileNameWithoutExtension(song)));
				Discord.Details = "Listening To:";
				Discord.StartTimeStamp = DateTimeOffset.Now.ToUnixTimeSeconds();

				UpdatePresence();
			}
		}
		
		public static void Pause()
		{
			if (DiscordManager.Enabled)
			{
				Discord.Pause();
			}

			if (SoundPlayer.playState == WMPPlayState.wmppsPlaying)
			{
				CurrentPosition = SoundPlayer.controls.currentPositionString;
				SoundPlayer.controls.pause();
				Console.Title = Title_Paused;
			}
		}


		public static void Resume()
		{
			if (DiscordManager.Enabled)
			{
				Discord.Resume();
			}

			if (SoundPlayer.playState == WMPPlayState.wmppsPaused)
			{
				SoundPlayer.controls.play();
				Console.Title = Title;
			}
		}

		public static void SetVolume(int volume)
		{
			SoundPlayer.settings.volume = volume;
		}

		public static void Replay()
		{
			SoundPlayer.controls.currentPosition = 0;
		}

		public static void Speed(double speed)
		{
			SoundPlayer.settings.rate = speed;
		}

		private static void SoundPlayer_PlayStateChange(int NewState)
		{
			if (NewState == (int) WMPPlayState.wmppsPlaying)
			{
				if (!string.IsNullOrWhiteSpace(CurrentPosition))
				{
					Kiroku.Log("Music Player", ConsoleColor.DarkCyan,
						$"Now playing: {Path.GetFileName(SoundPlayer.URL)} ({CurrentPosition} / {SoundPlayer.controls.currentItem.durationString})", ConsoleColor.Cyan);
				}
				else
				{
					Kiroku.Log("Music Player", ConsoleColor.DarkCyan,
						$"Now playing: {Path.GetFileName(SoundPlayer.URL)} ({SoundPlayer.controls.currentItem.durationString})", ConsoleColor.Cyan);	
				}
			}
			
			if (NewState == (int) WMPPlayState.wmppsMediaEnded && ContinueRandom)
			{
				WaitingForReady = true;
				Kiroku.Log("Music Player", ConsoleColor.DarkMagenta,
					$"{Path.GetFileName(SoundPlayer.URL)} has ended", ConsoleColor.Magenta);
			}
			else if (WaitingForReady && NewState == (int) WMPPlayState.wmppsStopped && ContinueRandom)
			{
				WaitingForReady = false;

				Action action = PlayRandom;
				action.BeginInvoke(action.EndInvoke, null);
			}
		}

		public static void SetTag(string tag = null)
		{
			if (string.IsNullOrWhiteSpace(Tag) && string.IsNullOrWhiteSpace(tag))
			{
				Kiroku.Log("Commands.Tags", ConsoleColor.DarkYellow, "Missing parameter to set tag.", ConsoleColor.Yellow);
				return;
			}

			if (!string.IsNullOrWhiteSpace(Tag) && string.IsNullOrWhiteSpace(tag))
			{
				Kiroku.Log("Commands.Tags", ConsoleColor.DarkGreen, $"The tag: {Tag} has been removed.", ConsoleColor.Green);
				Tag = "";
				return;
			}

			tag = tag.ToLower();
			
			if (!string.IsNullOrWhiteSpace(tag) && TagsManager.TaggedSongs.ContainsKey(tag))
			{
				Kiroku.Log("Commands.Tags", ConsoleColor.DarkGreen, $"Tag set to: {tag}", ConsoleColor.Green);
				Tag = tag;
			}
			else
			{
				Kiroku.Log("Commands.Tags", ConsoleColor.DarkRed, $"The tag: {tag} was not found in the database.", ConsoleColor.Red);
			}
		}
		
		private static string[] GetSongs(string filter = null)
		{
			string[] songs = new string[0];
			
			if (!string.IsNullOrWhiteSpace(Tag))
			{
				if (TagsManager.TaggedSongs.Keys.Contains(Tag))
				{
					songs = TagsManager.TaggedSongs[Tag].ToArray();
				}
			}
			else
			{
				if (string.IsNullOrWhiteSpace(filter))
				{
					filter = Filter;
				}

				songs = Directory.EnumerateFiles(CurrentDirectory, "*.mp3", SearchOption.AllDirectories)
					.Concat(Directory.EnumerateFiles(CurrentDirectory, "*.m4a", SearchOption.AllDirectories))
					.ToArray();

				if (!string.IsNullOrEmpty(filter))
				{
					songs = songs.Where(_ => _.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) != -1)
						.ToArray();
				}
			}

			return songs;
		}
	}
}