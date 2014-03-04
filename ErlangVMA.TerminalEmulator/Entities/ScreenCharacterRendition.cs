using System;
using System.Text.RegularExpressions;

namespace ErlangVMA.TerminalEmulation
{
	public class ScreenCharacterRendition
	{
		private TerminalFontEffect fontEffects;
		private TerminalColor foregroundColor;
		private TerminalColor backgroundColor;

		public ScreenCharacterRendition()
		{
			ResetToDefault();
		}

		public TerminalFontEffect FontEffects
		{
			get { return fontEffects; }
		}

		public TerminalColor Foreground
		{
			get { return foregroundColor; }
		}

		public TerminalColor Background
		{
			get { return backgroundColor; }
		}

		public ScreenCharacterRendition Clone()
		{
			var clone = new ScreenCharacterRendition();
			clone.fontEffects = fontEffects;
			clone.foregroundColor = foregroundColor;
			clone.backgroundColor = backgroundColor;

			return clone;
		}

		public void ChangeRendition(GraphicRendition command)
		{
			switch (command)
			{
				case GraphicRendition.Reset:
					ResetToDefault();
					return;

				case GraphicRendition.Bold:
					fontEffects |= TerminalFontEffect.Bold;
					return;
				case GraphicRendition.Italic:
					fontEffects |= TerminalFontEffect.Italic;
					return;
				case GraphicRendition.Underline:
					fontEffects |= TerminalFontEffect.Underlined;
					return;
				case GraphicRendition.UnderlineDouble:
					fontEffects |= TerminalFontEffect.DoubleUnderlined;
					return;
				case GraphicRendition.NoUnderline:
					fontEffects &= ~(TerminalFontEffect.Underlined | TerminalFontEffect.DoubleUnderlined);
					return;

				case GraphicRendition.Faint:
				case GraphicRendition.BlinkSlow:
				case GraphicRendition.BlinkRapid:
				case GraphicRendition.Inverse:
				case GraphicRendition.Conceal:
				case GraphicRendition.Font1:
				case GraphicRendition.NormalIntensity:
				case GraphicRendition.NoBlink:
				case GraphicRendition.Positive:
				case GraphicRendition.Reveal:
					return;
			}

			SetTerminalColor(command);
		}

		public void ResetToDefault()
		{
			fontEffects = TerminalFontEffect.None;
			foregroundColor = TerminalColor.White;
			backgroundColor = TerminalColor.Black;
		}

		private void SetTerminalColor(GraphicRendition rendition)
		{
			string name = Enum.GetName(typeof(GraphicRendition), rendition);
			string colorName = Regex.Replace(name, "(Foreground|Background)(Normal|Bright)(.*)", "$3");
			bool isForeground = name.StartsWith("F");

			TerminalColor color;
			if (colorName == "Reset")
			{
				color = isForeground ? TerminalColor.White : TerminalColor.Black;
			}
			else
			{
				color = (TerminalColor)Enum.Parse(typeof(TerminalColor), colorName);
			}

			if (isForeground)
			{
				foregroundColor = color;
			}
			else
			{
				backgroundColor = color;
			}
		}
	}
}

