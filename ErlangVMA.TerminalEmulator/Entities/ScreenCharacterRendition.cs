using System;

namespace ErlangVMA.TerminalEmulation
{
	public class ScreenCharacterRendition
	{
		private TerminalFontEffect fontEffects;
		private TerminalColor ForegroundColor;
		private TerminalColor BackgroundColor;

		public ScreenCharacterRendition ()
		{
		}

		public ScreenCharacterRendition Clone()
		{
			var clone = new ScreenCharacterRendition();
			return clone;
		}

		public void ChangeRendition(GraphicRendition command)
		{
			switch (command)
			{
				case GraphicRendition.Bold:
					fontEffects |= TerminalFontEffect.Bold;
					break;
				case GraphicRendition.Italic:
					fontEffects |= TerminalFontEffect.Italic;
					break;
				case GraphicRendition.Underline:
					fontEffects |= TerminalFontEffect.Underlined;
					break;
				case GraphicRendition.UnderlineDouble:
					fontEffects |= TerminalFontEffect.DoubleUnderlined;
					break;
				case GraphicRendition.NoUnderline:
					fontEffects &= ~(TerminalFontEffect.Underlined | TerminalFontEffect.DoubleUnderlined);
					break;
			}
		}

		public void ResetToDefault()
		{
		}
	}
}

