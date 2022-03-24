using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Pandorai.UI;

namespace Pandorai.Dialogues
{
	public class Trivia
	{
		private List<string> _lines = new List<string>();

		private string _fileName;

		private Random _rng = new Random();

		private List<int> _lastTrivias = new List<int>();

		private static string _sidekickThought = @"\c[#c6bdc9]...";

		public Trivia(string fileName)
		{
			_fileName = fileName;
		}
		
		public void Load(Main game)
		{
			var path = Path.Combine(game.Content.RootDirectory, _fileName);
			using (var stream = TitleContainer.OpenStream(path))
			{
				using (var reader = new StreamReader(stream))
				{
					string line;
					while((line = reader.ReadLine()) != null)
					{
						line = line.Insert(0, $"{_sidekickThought} ");
						line = line.Insert(line.Length, $" {_sidekickThought}");
						_lines.Add(line);
					}
				}
			}
		}

		public string GetRandomTrivia()
		{
			int index = _rng.Next(0, _lines.Count);
			while (_lastTrivias.Contains(index))
			{
				index = _rng.Next(0, _lines.Count);
			}

			_lastTrivias.Add(index);
			if(_lastTrivias.Count > _lines.Count / 2) // lines.Count / 2 is an arbitrary choice, can set it to anything else lower than lines.Count
			{
				_lastTrivias.RemoveAt(0);
			}

			return _lines[index];
		}

		public void DisplayRandomTrivia(Main game)
		{
			if (game.TurnManager.TurnCount % 100 == 0)
			{
				MessageLog.DisplayMessage(GetRandomTrivia(), Color.Gray);
			}
		}
	}
}
