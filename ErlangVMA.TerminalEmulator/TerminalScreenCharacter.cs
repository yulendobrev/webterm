using System;
using Newtonsoft.Json;

namespace ErlangVMA.TerminalEmulation
{
	[JsonObject]
	public class TerminalScreenCharacter
	{
		private char character;
		private ScreenCharacterRendition rendition;

		public TerminalScreenCharacter(char character, ScreenCharacterRendition rendition)
		{
			this.character = character;
			this.rendition = rendition;
		}

		[JsonProperty("c")]
		public char Character
		{
			get { return character; }
		}

		[JsonProperty("r")]
		public ScreenCharacterRendition Rendition
		{
			get { return rendition; }
		}
	}
}
