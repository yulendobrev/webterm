using System;
using System.Collections.Generic;

namespace ErlangVMA.TerminalEmulation
{
	public class TerminalWindowSize
	{
		private int columns;
		private int rows;

		public TerminalWindowSize() : this(0, 0)
		{
		}

		public TerminalWindowSize(int columns, int rows)
		{
			this.columns = columns;
			this.rows = rows;
		}

		public int Columns
		{
			get { return columns; }
		}

		public int Rows
		{
			get { return rows; }
		}
	}
}
