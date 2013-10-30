using System;

namespace ErlangVMA.TerminalEmulation
{
	public class TerminalScreenCharacter
	{
		private char character;
		private ScreenCharacterRendition rendition;

		public TerminalScreenCharacter(char character, ScreenCharacterRendition rendition)
		{
			this.character = character;
			this.rendition = rendition;
		}

		public char Character
		{
			get { return character; }
		}

		public ScreenCharacterRendition Rendition
		{
			get { return rendition; }
		}
	}
}
