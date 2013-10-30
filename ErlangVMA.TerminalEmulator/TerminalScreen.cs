using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ErlangVMA.TerminalEmulation
{
	public class TerminalScreen : ITerminalCommandInterpreter, ITerminalDisplay
	{
		private List<TerminalScreenCharacter[]> screen;
		private int currentLine;
		private Point savedCursorPosition;
		private Point cursorPosition;
		private TerminalWindowSize size;
		private CharacterSetSlot[] characterSets;
		private ScreenCharacterRendition currentRendition;
		private int currentCharacterSet;
		private Decoder currentDecoder;
		private List<int> tabStops;

		public TerminalScreen()
		{
			size = new TerminalWindowSize(80, 24);
			screen = new List<TerminalScreenCharacter[]>(size.Rows);

			for (int i = 0; i < size.Rows; ++i)
			{
				screen.Add(InitializeEmptyLine());
			}

			currentLine = 0;
			cursorPosition = new Point(0, 0);
			characterSets = new CharacterSetSlot[2] { CharacterSetSlot.US, CharacterSetSlot.US };
			SwitchToCharacterSet(0);
			tabStops = new List<int>();
			currentRendition = new ScreenCharacterRendition();
		}

		public void ResetToInitialState()
		{
			savedCursorPosition = null;
			ChangeG0CharacterSet(CharacterSetSlot.US);
			ChangeG1CharacterSet(CharacterSetSlot.US);
			SwitchToCharacterSet(0);
			ResetTabStops();

			RaiseScreenUpdated();
		}

		public void OutputText(IEnumerable<byte> bytes)
		{
			var symbols = DecodeCharacters(bytes);

			foreach (char symbol in symbols)
			{
				screen[currentLine + cursorPosition.Row][cursorPosition.Column] = new TerminalScreenCharacter(symbol, currentRendition.Clone());
				++cursorPosition.Column;

				if (cursorPosition.Column >= size.Columns)
				{
					cursorPosition.Column = 0;
					InsertNewLine();
				}
				else
				{
					RaiseScreenUpdated(cursorPosition.Column - 1, cursorPosition.Row, 1, 1);
				}
			}
		}

		public void InsertNewLine()
		{
			if (++cursorPosition.Row >= size.Rows)
			{
				ScrollPageUpwards(1);
				cursorPosition.Row = size.Rows - 1;
				screen.Add(InitializeEmptyLine());
				RaiseScreenUpdated();
			}
			else
			{
				RaiseCursorPositionChanged();
			}
		}

		private TerminalScreenCharacter[] InitializeEmptyLine()
		{
			var line = new TerminalScreenCharacter[size.Columns];

			for (int i = 0; i < line.Length; ++i)
			{
				line[i] = new TerminalScreenCharacter(' ', new ScreenCharacterRendition());
			}

			return line;
		}
		
		private char[] DecodeCharacters(IEnumerable<byte> bytes)
		{
			var byteBuffer = bytes.ToArray();
			int charCount = currentDecoder.GetCharCount(byteBuffer, 0, byteBuffer.Length);
			var charBuffer = new char[charCount];
			currentDecoder.GetChars(byteBuffer, 0, byteBuffer.Length, charBuffer, 0);

			return charBuffer;
		}

		public void SaveCursor()
		{
			savedCursorPosition = cursorPosition.Clone();
		}

		public void RestoreCursor()
		{
			if (savedCursorPosition != null)
			{
				cursorPosition = savedCursorPosition.Clone();
				RaiseCursorPositionChanged();
			}
		}

		public TerminalWindowSize GetSize()
		{
			return size;
		}

		public void MoveCursor(Direction direction, int amount)
		{
			switch (direction)
			{
				case Direction.Forward:
					cursorPosition.Column += amount;
					EnsureColumnInScreenBounds();
					break;

				case Direction.Backward:
					cursorPosition.Column -= amount;
					EnsureColumnInScreenBounds();
					break;

				case Direction.Up:
					cursorPosition.Row -= amount;
					EnsureRowInScreenBounds();
					break;

				case Direction.Down:
					cursorPosition.Row += amount;
					EnsureRowInScreenBounds();
					break;
			}
			RaiseCursorPositionChanged();
		}

		private void EnsureColumnInScreenBounds()
		{
			if (cursorPosition.Column < 0)
			{
				cursorPosition.Column = 0;
			}
			else if (cursorPosition.Column >= size.Columns)
			{
				cursorPosition.Column = size.Columns - 1;
			}
		}

		private void EnsureRowInScreenBounds()
		{
			if (cursorPosition.Row < 0)
			{
				cursorPosition.Row = 0;
			}
			else if (cursorPosition.Row >= size.Rows)
			{
				cursorPosition.Row = size.Rows - 1;
			}
		}

		public void MoveCursorToBeginningOfLineBelow(int linesRelativeToCurrentLine)
		{
			MoveCursorToColumn(0);
			MoveCursor(Direction.Down, linesRelativeToCurrentLine);
		}

		public void MoveCursorToBeginningOfLineAbove(int linesRelativeToCurrentLine)
		{
			MoveCursorToColumn(0);
			MoveCursor(Direction.Up, linesRelativeToCurrentLine);
		}

		public void MoveCursorToColumn(int column)
		{
			cursorPosition.Column = column;
			RaiseCursorPositionChanged();
		}

		public void MoveCursorTo(Point position)
		{
			cursorPosition = position.Clone();
			RaiseCursorPositionChanged();
		}

		public void MoveCursorToNextTabStop()
		{
			int currentColumn = cursorPosition.Column;
			int defaultNextTabStop = (currentColumn / 8 + 1) * 8;

			int customTabStop = tabStops.Where(s => currentColumn < s && s <= defaultNextTabStop).Min();
			if (customTabStop != 0)
			{
				MoveCursorToColumn(customTabStop);
			}
			else
			{
				MoveCursorToColumn(defaultNextTabStop);
			}
		}

		public void DeleteCharacters(int n)
		{
			var currentLineContents = screen[currentLine + cursorPosition.Row];
			var lastCharacterRendition = currentLineContents[size.Columns - 1].Rendition;

			for (int i = cursorPosition.Column; i < Math.Min(cursorPosition.Column + n, size.Columns); ++i)
			{
				currentLineContents[i] = currentLineContents[i + n];
			}

			for (int i = cursorPosition.Column + n; i < size.Columns; ++i)
			{
				currentLineContents[i] = new TerminalScreenCharacter(' ', lastCharacterRendition.Clone());
			}

			RaiseScreenUpdated(cursorPosition.Column, cursorPosition.Row, size.Columns - cursorPosition.Column, 1);
		}

		public void InsertLines(int n)
		{
			for (int i = currentLine + size.Rows - 1 - n; i >= currentLine + cursorPosition.Row; --i)
			{
				screen[i + n] = screen[i];
			}

			for (int i = currentLine + cursorPosition.Row; i < currentLine + Math.Min(cursorPosition.Row + n, size.Rows); ++i)
			{
				screen[i] = InitializeEmptyLine();
			}

			RaiseScreenUpdated(0, cursorPosition.Row, size.Columns, size.Rows - cursorPosition.Row);
		}

		public void DeleteLines(int n)
		{
			for (int i = currentLine + cursorPosition.Row; i < currentLine + size.Rows - 1 - n; ++i)
			{
				screen[i] = screen[i + n];
			}

			for (int i = currentLine + Math.Max(cursorPosition.Row, size.Rows - 1 - n); i < currentLine + size.Rows - 1; ++i)
			{
				screen[i] = InitializeEmptyLine();
			}

			RaiseScreenUpdated(0, cursorPosition.Row, size.Columns, size.Rows - cursorPosition.Row);
		}

		public void ClearScreen(ClearDirection direction)
		{
			int startRow = 0;
			int endRow = size.Rows;

			if (direction == ClearDirection.Forward)
			{
				ClearLine(ClearDirection.Forward);
				startRow = cursorPosition.Row + 1;
			}
			else if (direction == ClearDirection.Backward)
			{
				ClearLine(ClearDirection.Backward);
				endRow = cursorPosition.Row;
			}

			for (int i = startRow; i < endRow; ++i)
			{
				ClearLine(ClearDirection.Both, i);
			}

			RaiseScreenUpdated();
		}

		public void ClearLine(ClearDirection direction)
		{
			ClearLine(direction, cursorPosition.Row);
			RaiseScreenUpdated(0, cursorPosition.Row, size.Columns, 1);
		}
		
		private void ClearLine(ClearDirection direction, int row)
		{
			var currentLineContents = screen[currentLine + row];
			int start = 0;
			int end = size.Columns;

			if (direction == ClearDirection.Forward)
			{
				start = cursorPosition.Column;
			}
			else if (direction == ClearDirection.Backward)
			{
				end = cursorPosition.Column + 1;
			}

			for (int i = start; i < end; ++i)
			{
				ClearCharacter(currentLineContents, i);
			}
		}

		private void ClearCharacter(TerminalScreenCharacter[] terminalScreenCharacters, int i)
		{
			terminalScreenCharacters[i] = new TerminalScreenCharacter(' ', currentRendition.Clone());
		}

		public void ScrollPageUpwards(int linesToScroll)
		{
			currentLine -= linesToScroll;
			if (currentLine < 0)
			{
				currentLine = 0;
			}
			RaiseScreenUpdated();
		}

		public void ScrollPageDownwards(int linesToScroll)
		{
			currentLine += linesToScroll;

			int linesToAdd = currentLine + size.Rows - screen.Count;
			for (int i = 0; i < linesToAdd; ++i)
			{
				screen.Add(InitializeEmptyLine());
			}
			RaiseScreenUpdated();
		}

		public Point GetCursorPosition()
		{
			return cursorPosition.Clone();
		}

		public void ChangeGraphicRendition(IEnumerable<GraphicRendition> commands)
		{
			foreach (var command in commands)
			{
				currentRendition.ChangeRendition(command);
			}
			RaiseScreenUpdated(cursorPosition.Column, cursorPosition.Row, 1, 1);
		}

		public void ChangeMode(TerminalMode mode)
		{
		}

		public void ChangeG0CharacterSet(CharacterSetSlot slot)
		{
			ChangeCharacterSet(0, slot);
		}

		public void ChangeG1CharacterSet(CharacterSetSlot slot)
		{
			ChangeCharacterSet(1, slot);
		}

		private void ChangeCharacterSet(int i, CharacterSetSlot slot)
		{
			characterSets[i] = slot;

			if (currentCharacterSet == i)
			{
				SwitchToCharacterSet(currentCharacterSet);
			}
		}
		
		public void SwitchToG0Charset()
		{
			SwitchToCharacterSet(0);
		}

		public void SwitchToG1Charset()
		{
			SwitchToCharacterSet(1);
		}

		private void SwitchToCharacterSet(int set)
		{
			currentCharacterSet = set;
			switch (characterSets[currentCharacterSet])
			{
				case CharacterSetSlot.UK:
					break;
				case CharacterSetSlot.US:
					break;
				case CharacterSetSlot.SpecialAndLineDrawing:
					break;
				case CharacterSetSlot.AlternateRom:
					break;
				case CharacterSetSlot.AlternateRomSpecial:
					break;
			}
			currentDecoder = Encoding.UTF8.GetDecoder();
		}

		public void SetTabStop()
		{
			int column = cursorPosition.Column;
			if (!tabStops.Contains(column))
			{
				tabStops.Add(column);
			}
		}

		public void ClearTabStop()
		{
			tabStops.Remove(cursorPosition.Column);
		}

		public void ResetTabStops()
		{
			tabStops.Clear();
		}

		public string GetDeviceCode()
		{
			return string.Empty;
		}

		public void ResizeWindow(TerminalWindowSize size)
		{
		}

		public void MoveWindow(Point position)
		{
		}

		public void RingBell()
		{
			//System.Media.SystemSounds.Beep.Play();
			Console.Beep(220, 400);
		}


		public event Action<ScreenData> ScreenUpdated;

		public ScreenData GetWholeScreen()
		{
			var screen = GetScreenData(0, 0, size.Columns, size.Rows);
			return screen;
		}

		private void RaiseCursorPositionChanged()
		{
			RaiseScreenUpdated(cursorPosition.Column, cursorPosition.Row, 1, 1);
		}

		private void RaiseScreenUpdated()
		{
			RaiseScreenUpdated(0, 0, size.Columns, size.Rows);
		}

		private void RaiseScreenUpdated(int x, int y, int width, int height)
		{
			var screenData = GetScreenData(x, y, width, height);

			var screenUpdatedHandler = ScreenUpdated;
			if (screenUpdatedHandler != null)
			{
				screenUpdatedHandler(screenData);
			}
		}

		private ScreenData GetScreenData(int x, int y, int width, int height)
		{
			var screenData = new ScreenData()
			{
				X = x,
				Y = y,
				Width = width,
				Height = height,
				CursorPosition = cursorPosition.Clone(),
				Data = new char[width * height]
			};

			int destination = 0;
			for (int row = y; row < y + height; ++row)
			{
				for (int column = x; column < x + width; ++column)
				{
					screenData.Data[destination] = screen[currentLine + row][column].Character;
					++destination;
				}
			}

			return screenData;
		}
	}
}
