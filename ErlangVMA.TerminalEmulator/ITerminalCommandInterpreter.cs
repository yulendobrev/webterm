using System;
using System.Collections.Generic;

namespace ErlangVMA.TerminalEmulation
{
    public interface ITerminalCommandInterpreter
    {
        void ResetToInitialState();

        void OutputText(IEnumerable<byte> bytes);

        void SaveCursor();
        void RestoreCursor();

        TerminalWindowSize GetSize();

        void MoveCursor(Direction direction, int amount);
        void MoveCursorToBeginningOfLineBelow(int linesRelativeToCurrentLine);
        void MoveCursorToBeginningOfLineAbove(int linesRelativeToCurrentLine);
        void MoveCursorToColumn(int column);
        void MoveCursorTo(Point position);

        void DeleteCharacters(int n);
        void InsertLines(int n);
        void DeleteLines(int n);

        void ClearScreen(ClearDirection direction);
        void ClearLine(ClearDirection direction);
        void InsertNewLine();

        void ScrollPageUpwards(int linesToScroll);
        void ScrollPageDownwards(int linesToScroll);

        Point GetCursorPosition();

        void ChangeGraphicRendition(IEnumerable<GraphicRendition> commands);

        void ChangeMode(TerminalMode mode);
        void RingBell();

        void ChangeG0CharacterSet(CharacterSetSlot slot);
        void ChangeG1CharacterSet(CharacterSetSlot slot);
        void SwitchToG0Charset();
        void SwitchToG1Charset();

        void SetTabStop();
        void ClearTabStop();
        void ResetTabStops();
        void MoveCursorToNextTabStop();

        string GetDeviceCode();
        //DeviceStatus GetDeviceStatus();
        void ResizeWindow(TerminalWindowSize size);
        void MoveWindow(Point position);
    }
}

