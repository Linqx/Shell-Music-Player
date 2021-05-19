// --------------------------------------------------------------------
//                             DiscordManager.cs
// --------------------------------------------------------------------
// Author: Danny Guardado (Linqx)
// Created: 09/01/2020

using System;
using SharpPresence;

namespace ShellMusicPlayer
{
	public class DiscordManager
	{

		#region Constructor

		public DiscordManager()
		{
			Discord.Initialize(DISCORD_APPLICATION_ID, _handlers);

			Details = PAUSED;
			LargeImageKey = DK_BONGO;
			LargeImageText = NONE;
			SmallImageKey = BANANA;
			SmallImageText = NO_FILTER;
		}

		#endregion

		#region Public

		public const string PAUSED = "Paused";
		public const string LISTENING = "Listening To:";
		public const string NO_FILTER = "No Filter Set";
		public const string FILTER = "Filter";
		public const string DIDDY = "diddy";
		public const string DK_BONGO = "monsters";
		public const string BANANA = "";
		public const string NONE = "None";

		public static bool Enabled = true;

		public string LargeImageKey
		{
			get => _largeImageKey;
			set
			{
				_largeImageKey = value;
				_presence.largeImageKey = _largeImageKey;
			}
		}

		public string LargeImageText
		{
			get => _largeImageText;
			set
			{
				_largeImageText = value;
				_presence.largeImageText = _largeImageText;
			}
		}

		public string SmallImageKey
		{
			get => _smallImageKey;
			set
			{
				_smallImageKey = value;
				_presence.smallImageKey = _smallImageKey;
			}
		}

		public string SmallImageText
		{
			get => _smallImageText;
			set
			{
				_smallImageText = value;
				_presence.smallImageText = _smallImageText;
			}
		}

		public string State
		{
			get => _state;
			set
			{
				_state = value;
				_presence.state = _state;
			}
		}

		public string Details
		{
			get => _details;
			set
			{
				_details = value;
				_presence.details = _details;
			}
		}

		public long StartTimeStamp
		{
			get => _startTimeStamp;
			set
			{
				_startTimeStamp = value;
				_presence.startTimestamp = _startTimeStamp;
			}
		}

		#endregion

		#region Private

		private const string DISCORD_APPLICATION_ID = "493530807114399754";

		private Discord.RichPresence _presence;
		private readonly Discord.RichPresence _emptyPresence = new Discord.RichPresence();
		private readonly Discord.EventHandlers _handlers = new Discord.EventHandlers();

		private long _pausedTimeStamp;

		private string _largeImageKey;
		private string _largeImageText;

		private string _smallImageKey;
		private string _smallImageText;

		private string _state;
		private string _details;
		private long _startTimeStamp;

		#endregion

		#region Public Methods

		public void SetDiscord()
		{
			if (Enabled)
			{
				Discord.UpdatePresence(_presence);
			}
			else
			{
				Discord.UpdatePresence(_emptyPresence);
			}
		}

		public void UpdatePresence()
		{
			Discord.UpdatePresence(_presence);
		}

		public void Pause()
		{
			_pausedTimeStamp = DateTimeOffset.Now.ToUnixTimeSeconds() - _presence.startTimestamp;

			Details = PAUSED;
			StartTimeStamp = 0;

			UpdatePresence();
		}

		public void Resume()
		{
			Details = LISTENING;
			StartTimeStamp = DateTimeOffset.Now.ToUnixTimeSeconds() - _pausedTimeStamp;

			_pausedTimeStamp = 0;

			UpdatePresence();
		}

		#endregion

	}
}