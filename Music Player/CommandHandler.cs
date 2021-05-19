// --------------------------------------------------------------------
//                             CommandHandler.cs
// --------------------------------------------------------------------
// Author: Danny Guardado (Linqx)
// Created: 09/01/2020

using System;
using System.IO;
using System.Linq;
using KirokuLogging;

namespace ShellMusicPlayer
{
	public class CommandHandler
	{
		public void Handle(string commandline)
		{
			string[] split = commandline.Split(' ');
			string command = split[0];
			string[] parameters = split.Where((_, i) => i != 0).ToArray();
			string params_joined = string.Join(" ", parameters);

			switch (command)
			{
				case "discord":
					DiscordManager.Enabled = !DiscordManager.Enabled;
					Program.SetDiscord();
					break;
				case "d":
				case "dir":
				case "directory":
					if (split.Length <= 1)
					{
						Kiroku.Log("Commands.Directory", ConsoleColor.DarkYellow,
							"Expected 1 parameter, received none.", ConsoleColor.Yellow);
						break;
					}

					string dir = string.Join(" ", parameters);

					if (!Directory.Exists(dir))
					{
						Kiroku.Log("Commands.Directory", ConsoleColor.DarkRed, 
							$"\"{dir}\" is not a valid directory.", ConsoleColor.Red);
						break;
					}

					Program.CurrentDirectory = dir;
					Kiroku.Log("Commands.Directory", ConsoleColor.DarkGreen, 
						$"Directory set to: \"{Program.CurrentDirectory}\"", ConsoleColor.Green);
					break;
				case "play":
				case "p":
					if (split.Length <= 1)
					{
						Kiroku.Log("Commands.Play", ConsoleColor.DarkYellow,
							"Expected 1 parameter, received none.", ConsoleColor.Yellow);
						break;
					}

					Program.Play(string.Join(" ", parameters));
					break;
				case "r":
				case "ran":
				case "random":
				case "rand":
					Program.PlayRandom();
					break;
				case "stop":
				case "pause":
					Program.Pause();
					break;
				case "resume":
				case "continue":
					Program.Resume();
					break;
				case "exit":
					Program.Running = false;
					break;
				case "list":
					Program.List();
					break;
				case "filter":
				case "f":
				case "fil":
					Program.SetFilter(parameters);
					break;
				case "showfilter":
				case "sf":
					Program.ShowFilter();
					break;
				case "volume":
					if (!int.TryParse(parameters[0], out int volume))
					{
						Kiroku.Log("Commands.Volume", ConsoleColor.DarkRed, 
							$"Failure when parsing volume: \"{parameters[0]}\"", ConsoleColor.Red);
						break;
					}

					Program.SetVolume(volume);
					break;
				case "replay":
					Program.Replay();
					break;
				case "speed":
					if (split.Length <= 1)
					{
						Kiroku.Log("Commands.Speed", ConsoleColor.DarkYellow,
							"Expected 1 parameter, received none.", ConsoleColor.Yellow);
						break;
					}

					double speed = double.Parse(parameters[0]);
					Program.Speed(speed);
					break;
				case "playlist":
				{
					// Example: (tag) -add peaceful

					string[] args = params_joined.Split('-').Where((_, i) => i > 0).ToArray();
					HandleTag(args);
					break;
				}
			}
		}

		private void HandleTag(string[] args)
		{
			foreach (string arg in args)
			{
				string[] data = arg.Split(' ');
				string command = data[0];
				string value = "";

				if (data.Length > 1)
				{
					value = data[1];
				}
				
				switch (command)
				{
					/* NOTE (Danny): Sets the current tag. */
					case "set":
					case "s":
						Program.SetTag(value);
						break;
					
					/* NOTE (Danny): Adds a tag to the current song. */
					case "add":
					case "a":
						if (string.IsNullOrEmpty(Program.CurrentSong))
						{
							Kiroku.Log("Commands.Tags", ConsoleColor.DarkYellow,
								"There is currently no song that's selected. Tag was not set.", ConsoleColor.Yellow);
							return;
						}
						
						TagsManager.AddTag(value);
						break;

					/* NOTE (Danny): Removes a tag from the current song. */
					case "remove":
					case "r":
						if (string.IsNullOrEmpty(Program.CurrentSong))
						{
							Kiroku.Log("Commands.Tags", ConsoleColor.DarkYellow,
								"There is currently no song that's selected. Tag was not set.", ConsoleColor.Yellow);
							return;
						}
						TagsManager.RemoveTag(value);
						break;

					/* NOTE (Danny): Displays all the tags of the current song */
					
					default:
						TagsManager.ShowTags();
						break;
				}
			}
		}
	}
}