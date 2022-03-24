using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;
using System.Collections.Generic;

namespace Pandorai.UI
{
	public static class MessageLog
	{
		private static Queue<string> _previousMessages = new Queue<string>();
		private static int _maxMessages = 7;
		private static VerticalStackPanel _panel;

		public static void DisplayMessage(string message, Color? color = null)
		{
			_previousMessages.Enqueue(message);

			_panel.Widgets.Add(new Label
			{
				Text = message,
				TextColor = color ?? Color.White,
			});

			if(_previousMessages.Count > _maxMessages)
			{
				_previousMessages.Dequeue();
				_panel.Widgets.RemoveAt(0);
			}
		}

		public static void Clear()
		{
			_previousMessages.Clear();
			_panel.Widgets.Clear();
		}

		public static void Show()
		{
			_panel.Visible = true;
		}

		public static void Hide()
		{
			_panel.Visible = false;
		}

		public static Widget GUI()
		{
			_panel = new VerticalStackPanel
			{
				Id = "MessageLog",
			};

			return _panel;
		}
	}
}
