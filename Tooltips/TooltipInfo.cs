using Microsoft.Xna.Framework.Graphics;

namespace Pandorai.Tooltips
{
	public class TooltipInfo
	{
		public string Title;
	}

	public class TextTooltip : TooltipInfo
	{
		public string Text; 
	}

	public class ImageTooltip : TooltipInfo
	{
		public Texture2D Image;
	}
}
