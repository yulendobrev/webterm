using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace ErlangVMA.TerminalEmulation
{
	public class TerminalStreamDecoder : ITerminalStreamDecoder
	{
		private TerminalInputState inputState;
		private List<byte> escapeCodeBuffer;
		private List<string> controlParameters;
		private Dictionary<byte, Action> specialSymbolActions;
		private ITerminalCommandInterpreter interpreter;

		public const byte EscapeCharacter = 0x1B;
        public const byte LeftBracketCharacter = 0x5B;
		public const byte ParameterDelimiterCharacter = 0x3B;
		public const byte ControlSequenceIntroducerCharacter = 0x9B;

		public const byte NullCharacter = 0x00;
		public const byte BellCharacter = 0x07;
		public const byte BackspaceCharacter = 0x08;
		public const byte HorizontalTabCharacter = 0x09;
		public const byte LineFeedCharacter = 0x0A;
		public const byte VerticalTabCharacter = 0x0B;
		public const byte FormFeedCharacter = 0x0C;
		public const byte CarriageReturnCharacter = 0x0D;
		public const byte ShiftOutCharacter = 0x0E; // Ctrl-N; Shift out (switches to the G0 charset)
		public const byte ShiftInCharacter = 0x0F; // Ctrl-O; Shift in (switches to the G1 charset)
		public const byte XonCharacter = 0x11;
		public const byte XoffCharacter = 0x13;
		public const byte CancelSequenceCharacter = 0x18; // Cancel Escape Sequence

		public TerminalStreamDecoder() :
			this(new TerminalScreen())
		{
		}

		public TerminalStreamDecoder(ITerminalCommandInterpreter interpreter)
		{
			this.interpreter = interpreter;

			Action ignoreSymbol = () => { };

			this.inputState = TerminalInputState.Normal;
			this.escapeCodeBuffer = new List<byte>();
			this.controlParameters = new List<string>();
			this.specialSymbolActions = new Dictionary<byte, Action>()
			{
				{ NullCharacter, ignoreSymbol },
				{ BellCharacter, () => interpreter.RingBell() },
				{ BackspaceCharacter, () => interpreter.MoveCursor(Direction.Backward, 1) },
				{ HorizontalTabCharacter, () => interpreter.MoveCursorToNextTabStop() },
				{ LineFeedCharacter, () => { interpreter.InsertNewLine(); interpreter.MoveCursorToColumn(0); } },
				{ VerticalTabCharacter, () => interpreter.InsertNewLine() },
				{ FormFeedCharacter, () => interpreter.InsertNewLine() },
				{ CarriageReturnCharacter, () => interpreter.MoveCursorToColumn(0) },
				{ ShiftOutCharacter, () => interpreter.SwitchToG1Charset() },
				{ ShiftInCharacter, () => interpreter.SwitchToG0Charset() },
				{ XonCharacter, ignoreSymbol },
				{ XoffCharacter, ignoreSymbol },
				{ CancelSequenceCharacter, ignoreSymbol },
			};
		}

		public void ProcessInput(IEnumerable<byte> bytes)
		{
			foreach (var @byte in bytes)
			{
				switch (inputState)
				{
					case TerminalInputState.Normal:
						ProcessNormalInput(@byte);
						break;

					case TerminalInputState.EscapeSequence:
						ProcessEscapeSequenceInput(@byte);
						break;

					case TerminalInputState.ControlSequence:
						ProcessControlSequenceInput(@byte);
						break;
				}
			}
		}

		private void ProcessNormalInput(byte @byte)
		{
			if (@byte == EscapeCharacter)
			{
				inputState = TerminalInputState.EscapeSequence;
			}
			else if (@byte == ControlSequenceIntroducerCharacter)
			{
				inputState = TerminalInputState.ControlSequence;
			}
			else
			{
				if (specialSymbolActions.ContainsKey(@byte))
				{
					var symbolHandler = specialSymbolActions[@byte];
					symbolHandler();
				}
				else
				{
					interpreter.OutputText(new [] { @byte });
				}
			}
		}

		private void ProcessEscapeSequenceInput(byte @byte)
		{
			if (@byte == LeftBracketCharacter && !escapeCodeBuffer.Any())
			{
				inputState = TerminalInputState.ControlSequence;
			}
			else if (0x20 <= @byte && @byte <= 0x2F)
			{
				AddCurrentControlParameterByte(@byte);
			}
			else if (0x30 <= @byte && @byte <= 0x7E)
			{
				AddCurrentControlParameterByte(@byte);

				string parameterValue = GetCurrentControlParameterValue();
				ProcessEscapeSequence(parameterValue);

				ClearCurrentControlParameterBuffer();

				inputState = TerminalInputState.Normal;
			}
			else
			{
				interpreter.OutputText(new byte[] { @byte });
				inputState = TerminalInputState.Normal;
			}
		}

		private void ProcessEscapeSequence(string parameter)
		{
			switch (parameter)
			{
				case "=":
					interpreter.ChangeMode(TerminalMode.AlternateKeypad);
					break;
				case ">":
					interpreter.ChangeMode(TerminalMode.NumericKeypad);
					break;
				case "(A":
					interpreter.ChangeG0CharacterSet(CharacterSetSlot.UK);
					break;
				case ")A":
					interpreter.ChangeG1CharacterSet(CharacterSetSlot.UK);
					break;
				case "(B":
					interpreter.ChangeG0CharacterSet(CharacterSetSlot.US);
					break;
				case ")B":
					interpreter.ChangeG1CharacterSet(CharacterSetSlot.US);
					break;
				case "(0":
					interpreter.ChangeG0CharacterSet(CharacterSetSlot.SpecialAndLineDrawing);
					break;
				case ")0":
					interpreter.ChangeG1CharacterSet(CharacterSetSlot.SpecialAndLineDrawing);
					break;
				case "(1":
					interpreter.ChangeG0CharacterSet(CharacterSetSlot.AlternateRom);
					break;
				case ")1":
					interpreter.ChangeG1CharacterSet(CharacterSetSlot.AlternateRom);
					break;
				case "(2":
					interpreter.ChangeG0CharacterSet(CharacterSetSlot.AlternateRomSpecial);
					break;
				case ")2":
					interpreter.ChangeG1CharacterSet(CharacterSetSlot.AlternateRomSpecial);
					break;
				case "E":
				case "G": // Exit Graphics Mode
					interpreter.MoveCursorToBeginningOfLineBelow(1);
					break;
				case "7":
					interpreter.SaveCursor();
					break;
				case "8":
					interpreter.RestoreCursor();
					break;
				case "H":
					interpreter.SetTabStop();
					break;
				case "c":
					interpreter.ResetToInitialState();
					break;
			}
		}

		private void ProcessControlSequenceInput(byte @byte)
		{
			if (0x30 <= @byte && @byte <= 0x3F)
			{
				if (@byte != ParameterDelimiterCharacter)
				{
					AddCurrentControlParameterByte(@byte);
				}
				else
				{
					AppendControlParameter();
				}
			}
			else if (0x40 <= @byte && @byte <= 0x7E)
			{
				AppendControlParameter();
				ProcessControlSequence(Encoding.ASCII.GetString(new [] { @byte }), controlParameters);
				ClearControlParameters();

				inputState = TerminalInputState.Normal;
			}
//			else if (0x20 <= @byte && @byte <= 0x2F)
//			{
//			}
			else
			{
				interpreter.OutputText(new [] { @byte });

				escapeCodeBuffer.Clear();
				controlParameters.Clear();

				inputState = TerminalInputState.Normal;
			}
		}

		private void AppendControlParameter()
		{
			if (escapeCodeBuffer.Any())
			{
				string parameterValue = GetCurrentControlParameterValue();
				controlParameters.Add(parameterValue);
				escapeCodeBuffer.Clear();
			}
		}

		private void ClearControlParameters()
		{
			controlParameters.Clear();
		}

		private void AddCurrentControlParameterByte(byte @byte)
		{
			escapeCodeBuffer.Add(@byte);
		}

		private string GetCurrentControlParameterValue()
		{
			return Encoding.ASCII.GetString(escapeCodeBuffer.ToArray());
		}

		private void ClearCurrentControlParameterBuffer()
		{
			escapeCodeBuffer.Clear();
		}

		private void ProcessControlSequence(string id, List<string> parameters)
		{
			switch (id)
			{
			//cdprqXZ@
			//________
				case "m":
					interpreter.ChangeGraphicRendition(parameters.Select(p => (GraphicRendition)int.Parse(p)).ToList());
					break;
				case "A":
					interpreter.MoveCursor(Direction.Up, DecodeInteger(parameters, 0, 1));
					break;
				case "B":
					interpreter.MoveCursor(Direction.Down, DecodeInteger(parameters, 0, 1));
					break;
				case "C":
					interpreter.MoveCursor(Direction.Forward, DecodeInteger(parameters, 0, 1));
					break;
				case "D":
					interpreter.MoveCursor(Direction.Backward, DecodeInteger(parameters, 0, 1));
					break;
				case "E":
					interpreter.MoveCursorToBeginningOfLineBelow(DecodeInteger(parameters, 0, 1));
					break;
				case "F":
					interpreter.MoveCursorToBeginningOfLineAbove(DecodeInteger(parameters, 0, 1));
					break;
				case "G":
					interpreter.MoveCursorToColumn(DecodeInteger(parameters, 0, 1) - 1);
					break;
				case "H":
				case "f":
					interpreter.MoveCursorTo(new Point() { Row = DecodeInteger(parameters, 0, 1) - 1, Column = DecodeInteger(parameters, 1, 1) - 1 });
					break;
				case "K":
					interpreter.ClearLine((ClearDirection)DecodeInteger(parameters, 0));
					break;
				case "J":
					interpreter.ClearScreen((ClearDirection)DecodeInteger(parameters, 0));
					break;
				case "h":
					string parameter = parameters.FirstOrDefault();
					ProcessSetMode(parameter);
					break;
				case "l":
					parameter = parameters.FirstOrDefault();
					ProcessResetMode(parameter);
					break;
				case "g":
					switch (DecodeInteger(parameters, 0))
					{
						case 0:
							interpreter.ClearTabStop();
							break;
						case 3:
							interpreter.ResetTabStops();
							break;
					}
					break;
				case "i":
					// Print commands
					break;
				case "n":
					parameter = parameters.FirstOrDefault();
					if (parameter == "6")
					{
						var position = interpreter.GetCursorPosition();
						interpreter.OutputText(Encoding.ASCII.GetBytes(string.Format("{2}[{0};{1}R", position.Row + 1, position.Column + 1, EscapeCharacter)));
					}
					break;
				case "s":
					interpreter.SaveCursor();
					break;
				case "u":
					interpreter.RestoreCursor();
					break;
				case "P":
					interpreter.DeleteCharacters(DecodeInteger(controlParameters, 0, 1));
					break;
				case "L":
					interpreter.InsertLines(DecodeInteger(controlParameters, 0, 1));
					break;
				case "M":
					interpreter.DeleteLines(DecodeInteger(controlParameters, 0, 1));
					break;
				case "S":
					interpreter.ScrollPageUpwards(DecodeInteger(parameters, 0, 1));
					break;
				case "T":
					interpreter.ScrollPageDownwards(DecodeInteger(parameters, 0, 1));
					break;
				case "~":
					parameter = parameters.FirstOrDefault();
					if (parameter == "3")
					{
						interpreter.DeleteCharacters(1);
					}
					else if (parameter == "5")
					{
						interpreter.ScrollPageUpwards(1);
					}
					else if (parameter == "6")
					{
						interpreter.ScrollPageDownwards(1);
					}
					break;
			}
		}

		private void ProcessSetMode(string parameter)
		{
			switch (parameter)
			{
				case "2":
					// Keyboard Action - Locked
					//interpreter.ModeChanged();
					break;
				case "4":
					// Insert Mode
					//interpreter.ModeChanged();
					break;
				case "12":
					// Send-receive Off
					//interpreter.ModeChanged();
					break;
				case "20":
					interpreter.ChangeMode(TerminalMode.NewLine);
					break;
				case "?1":
					interpreter.ChangeMode(TerminalMode.CursorKeyToApplication);
					break;
//				case "?2":
//					interpreter.ModeChanged(TerminalMode.ANSI);
//					break;
				case "?3":
					interpreter.ChangeMode(TerminalMode.Columns132);
					break;
				case "?4":
					interpreter.ChangeMode(TerminalMode.SmoothScrolling);
					break;
				case "?5":
					interpreter.ChangeMode(TerminalMode.ReverseVideo);
					break;
				case "?6":
					interpreter.ChangeMode(TerminalMode.OriginIsRelative);
					break;
				case "?7":
					interpreter.ChangeMode(TerminalMode.LineWrap);
					break;
				case "?8":
					interpreter.ChangeMode(TerminalMode.AutoRepeat);
					break;
				case "?9":
					interpreter.ChangeMode(TerminalMode.Interlacing);
					break;
				case "?18":
					// Print Form Feed On
					//interpreter.ModeChanged(TerminalMode.);
					break;
				case "?19":
					// Print Extent - Full Screen
					//interpreter.ModeChanged(TerminalMode.);
					break;
				case "?25":
					interpreter.ChangeMode(TerminalMode.ShowCursor);
					break;
			}
		}

		private void ProcessResetMode(string parameter)
		{
			switch (parameter)
			{
				case "2":
					// Keyboard Action - Unlocked
					//interpreter.ModeChanged();
					break;
				case "4":
					// Replace Mode
					//interpreter.ModeChanged();
					break;
				case "12":
					// Send-receive On
					//interpreter.ModeChanged();
					break;
				case "20":
					interpreter.ChangeMode(TerminalMode.LineFeed);
					break;
				case "?1":
					interpreter.ChangeMode(TerminalMode.CursorKeyToCursor);
					break;
//				case "?2":
//					interpreter.ModeChanged(TerminalMode.VT52);
//					break;
				case "?3":
					interpreter.ChangeMode(TerminalMode.Columns80);
					break;
				case "?4":
					interpreter.ChangeMode(TerminalMode.JumpScrolling);
					break;
				case "?5":
					interpreter.ChangeMode(TerminalMode.NormalVideo);
					break;
				case "?6":
					interpreter.ChangeMode(TerminalMode.OriginIsAbsolute);
					break;
				case "?7":
					interpreter.ChangeMode(TerminalMode.DisableLineWrap);
					break;
				case "?8":
					interpreter.ChangeMode(TerminalMode.DisableAutoRepeat);
					break;
				case "?9":
					interpreter.ChangeMode(TerminalMode.DisableInterlacing);
					break;
				case "?18":
					// Print Form Feed Off
					//interpreter.ModeChanged(TerminalMode.);
					break;
				case "?19":
					// Print Extent - Scrolling Region
					//interpreter.ModeChanged(TerminalMode.);
					break;
				case "?25":
					interpreter.ChangeMode(TerminalMode.HideCursor);
					break;
			}
		}

		private int DecodeInteger(List<string> csiParameters, int parameterIndex)
		{
			return DecodeInteger(csiParameters, parameterIndex, 0);
		}

		private int DecodeInteger(List<string> csiParameters, int parameterIndex, int defaultValue)
		{
			if (parameterIndex >= csiParameters.Count)
				return defaultValue;

			int result;
			if (!int.TryParse(csiParameters[parameterIndex], out result))
				return defaultValue;

			return result;
		}

		private enum TerminalInputState
		{
			Normal,
			EscapeSequence,
			ControlSequence
		}
	}
}
