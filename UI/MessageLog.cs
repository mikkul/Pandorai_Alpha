using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;
using System.Collections.Generic;
namespace Pandorai.UI
{
	public static class MessageLog
	{
		private static Queue<string> previousMessages = new Queue<string>();
		private static int maxMessages = 7;
		private static VerticalStackPanel panel;

		public static void DisplayMessage(string message, Color? color = null)
		{
			previousMessages.Enqueue(message);

			panel.Widgets.Add(new Label
			{
				Text = message,
				TextColor = color ?? Color.White,
			});

			if(previousMessages.Count > maxMessages)
			{
				previousMessages.Dequeue();
				panel.Widgets.RemoveAt(0);
			}
		}

		public static void Clear()
		{
			previousMessages.Clear();
			panel.Widgets.Clear();
		}

		public static void Show()
		{
			panel.Visible = true;
		}

		public static void Hide()
		{
			panel.Visible = false;
		}

		public static Widget GUI()
		{
			panel = new VerticalStackPanel
			{
				Id = "MessageLog",
			};

			return panel;
		}
	}
}
