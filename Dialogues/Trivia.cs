using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Myra.Graphics2D.UI;

namespace Pandorai.Dialogues
{
	public class Trivia
	{
		private List<string> lines = new List<string>();

		private string fileName;

		private Random rng = new Random();

		private List<int> lastTrivias = new List<int>();

		public Trivia(string _fileName)
		{
			fileName = _fileName;
		}
		
		public void Load(Game1 game)
		{
			var path = Path.Combine(game.Content.RootDirectory, fileName);
			using (var stream = TitleContainer.OpenStream(path))
			{
				using (var reader = new StreamReader(stream))
				{
					string line;
					while((line = reader.ReadLine()) != null)
					{
						lines.Add(line);
					}
				}
			}
		}

		public string GetRandomTrivia()
		{
			int index = rng.Next(0, lines.Count);
			while (lastTrivias.Contains(index))
			{
				index = rng.Next(0, lines.Count);
			}

			lastTrivias.Add(index);
			if(lastTrivias.Count > lines.Count / 2) // lines.Count / 2 is an arbitrary choice, can set it to anything else lower than lines.Count
			{
				lastTrivias.RemoveAt(0);
			}

			return lines[index];
		}

		public void DisplayRandomTrivia(Game1 game)
		{
			if (game.TurnManager.TurnCount % 15 == 0 && !game.Player.IsInteractingWithSomeone)
			{
				DialogueManager.SetNameLabel("Hmm");
				DialogueManager.SetTextContent(GetRandomTrivia());
			}
		}
	}
}
