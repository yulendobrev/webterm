using System;
using System.Collections.Generic;

namespace ErlangVMA.TerminalEmulation
{
	public class Point
	{
		private int column;
		private int row;

		public Point()
		{
		}

		public Point(int column, int row)
		{
			Column = column;
			Row = row;
		}

		public Point Clone()
		{
			return new Point(column, row);
		}

		public int Column
		{
			get { return column; }
			set { column = value; }
		}

		public int Row
		{
			get { return row; }
			set { row = value; }
		}
	}
}

