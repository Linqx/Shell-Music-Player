using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShellMusicPlayer
{
    public class CommandHandler
    {
        public void Handle(string commandline)
        {
            string[] split = commandline.Split(' ');
            string command = split[0];
            string[] parameters = split.Where((_, i) => i != 0).ToArray();

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
                        Console.WriteLine("This command needs at least 1 parameter");
                        break;
                    }

                    string dir = string.Join(" ", split.Where((_, i) => i > 0).ToArray());

                    if (!Directory.Exists(dir))
                    {
                        Console.WriteLine("Invalid directory! Input a valid directory.");
                        break;
                    }

                    Program.CurrentDirectory = dir;
                    Console.WriteLine($"Directory set to: {Program.CurrentDirectory}");
                    break;
                case "play":
                case "p":
                    if (split.Length <= 1)
                    {
                        Console.WriteLine("This command needs at least 1 parameter");
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
            }
        }
    }
    }
