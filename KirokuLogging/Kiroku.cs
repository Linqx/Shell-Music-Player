// --------------------------------------------------------------------
//                             KirokuLogging.cs
// --------------------------------------------------------------------
// Author: Danny Guardado (Linqx)
// Created: 01/12/2021

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;

namespace KirokuLogging
{
	public static class Kiroku
	{
		public static bool ShowDate;
		public static LogText LogMethod;

		private static readonly Thread worker;
		private static readonly BlockingCollection<LogData> logs;

		static Kiroku()
		{
			LogMethod = Console.Write;

			Console.ForegroundColor = ConsoleColor.DarkGray;
			
			logs = new BlockingCollection<LogData>();

			worker = new Thread(Run);
			worker.Start();
		}

		private static void Run()
		{
			/*
			 * NOTE (Danny): Because the blocking collection is never completed,
			 * the loop should be indefinite.
			 */

			foreach (LogData log in logs.GetConsumingEnumerable())
			{
				if (log.ShowDateTime)
				{
					LogMethod($"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] ");
				}

				ConsoleColor oldColor = Console.ForegroundColor;
				
				if (!string.IsNullOrWhiteSpace(log.Label.Text))
				{
					Console.ForegroundColor = log.Label.Color;
					LogMethod($"[{log.Label.Text}] ");
				}

				Console.ForegroundColor = log.Data.Color;
				LogMethod($"{log.Data.Text} ");

				LogMethod("\n");

				Console.ForegroundColor = oldColor;
			}
		}

		public static void Log(string label, ConsoleColor labelColor, object text, ConsoleColor color)
		{
			LogData log = new LogData
			{
				Label = new LogNode
				{
					Text = label,
					Color = labelColor,
				},
				Data = new LogNode
				{
					Text = text.ToString(),
					Color = color
				},
				ShowDateTime = ShowDate
			};

			logs.Add(log);
		}
	}
}