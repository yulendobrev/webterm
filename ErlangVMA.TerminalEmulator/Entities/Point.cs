using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ErlangVMA.TerminalEmulation
{
	[JsonObject]
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

		[JsonProperty("c")]
		public int Column
		{
			get { return column; }
			set { column = value; }
		}

		[JsonProperty("r")]
		public int Row
		{
			get { return row; }
			set { row = value; }
		}
	}
}

