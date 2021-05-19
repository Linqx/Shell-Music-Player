// --------------------------------------------------------------------
//                             TagsManager.cs
// --------------------------------------------------------------------
// Author: Danny Guardado (Linqx)
// Created: 01/12/2021

using System;
using System.Collections.Generic;
using System.IO;
using KirokuLogging;
using Newtonsoft.Json;

namespace ShellMusicPlayer
{
	public class TagsManager
	{
		public static Dictionary<string, List<string>> TaggedSongs;

		private static string TagsDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}//Danny's Sound Player";
		private static string TagsPath = $"{TagsDirectory}//tags.json";
		
		public static void Initialize()
		{
			if (!File.Exists(TagsPath))
			{
				if (!Directory.Exists(TagsDirectory))
				{
					Directory.CreateDirectory(TagsDirectory).Refresh();
				}
				
				Kiroku.Log("Tags", ConsoleColor.White, "Tags database not found. Generating database.", ConsoleColor.Gray);
				
				File.WriteAllText(TagsPath, "");
			}

			try
			{
				string data = File.ReadAllText(TagsPath);
				TaggedSongs = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(data);

				if (TaggedSongs == null)
				{
					TaggedSongs = new Dictionary<string, List<string>>();
				}
			}
			catch
			{
				TaggedSongs = new Dictionary<string, List<string>>();
			}
		}
		
		public static void AddTag(string tag)
		{
			tag = tag.ToLower();
			
			if (!TaggedSongs.TryGetValue(tag, out List<string> songs))
			{
				songs = new List<string>();
			}

			if (songs.Contains(Program.CurrentSongPath))
			{
				Kiroku.Log("Tags", ConsoleColor.DarkYellow, $"The song: {Program.CurrentSong} has already been tagged with: {tag}", ConsoleColor.Yellow);
				return;
			}
			
			songs.Add(Program.CurrentSongPath);
			TaggedSongs[tag] = songs;
			
			FlushTags();
		}

		public static void RemoveTag(string tag)
		{
			tag = tag.ToLower();
			
			if (!TaggedSongs.TryGetValue(tag, out List<string> songs))
			{
				Kiroku.Log("Tags", ConsoleColor.DarkRed, 
					$"Tag: {tag} was not found.", ConsoleColor.Red);
				return;
			}

			if (songs.Contains(Program.CurrentSongPath))
			{
				songs.Remove(Program.CurrentSongPath);
			}
			else
			{
				if (songs.Contains(Program.CurrentSongPath))
				{
					Kiroku.Log("Tags", ConsoleColor.DarkYellow, $"The song: {Program.CurrentSong} was not tagged with: {tag}", ConsoleColor.Yellow);
					return;
				}
			}
			
			if (songs.Count == 0)
			{
				TaggedSongs.Remove(tag);
			}

			FlushTags();
		}

		public static void ShowTags()
		{
			
		}

		private static void FlushTags()
		{
			string json = JsonConvert.SerializeObject(TaggedSongs, Formatting.Indented);
			File.WriteAllText(TagsPath, json);
		}
	}
}