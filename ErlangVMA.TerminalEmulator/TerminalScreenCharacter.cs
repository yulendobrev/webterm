using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ErlangVMA.TerminalEmulation
{
    [JsonObject]
    [DataContract]
    public class TerminalScreenCharacter
    {
        private char character;
        private ScreenCharacterRendition rendition;

        public TerminalScreenCharacter()
        { }

        public TerminalScreenCharacter(char character, ScreenCharacterRendition rendition)
        {
            this.character = character;
            this.rendition = rendition;
        }

        [JsonProperty("c")]
        [DataMember]
        public char Character
        {
            get { return character; }
            set { character = value; }
        }

        [JsonProperty("r")]
        [DataMember]
        public ScreenCharacterRendition Rendition
        {
            get { return rendition; }
            set { rendition = value; }
        }
    }
}
