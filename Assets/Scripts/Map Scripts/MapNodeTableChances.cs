using System.Collections.Generic;
using UnityEngine;

namespace Woopsious
{
	public static class MapNodeTableChances
	{
		//int = column row count, float = chance of column to have x amount of rows

		public static List<List<RowCountPossibilities>> RowCountPossibilities = new()
		{
			//column 1
			new List<RowCountPossibilities>
			{
				new(1, 0.9f),
				new(2, 0.6f),
				new(3, 0.3f),
				new(4, 0.0f),
				new(5, 0.0f),
				new(6, 0.0f),
				new(7, 0.0f),
				new(8, 0.0f),
				new(9, 0.0f),
				new(10, 0.0f),
			},

			//column 2
			new List<RowCountPossibilities>
			{
				new(1, 0.0f),
				new(2, 0.7f),
				new(3, 0.9f),
				new(4, 0.8f),
				new(5, 0.6f),
				new(6, 0.4f),
				new(7, 0.2f),
				new(8, 0.1f),
				new(9, 0.0f),
				new(10, 0.0f),
			},

			//column 3
			new List<RowCountPossibilities>
			{
				new(1, 0.0f),
				new(2, 0.3f),
				new(3, 0.5f),
				new(4, 0.9f),
				new(5, 0.9f),
				new(6, 0.5f),
				new(7, 0.5f),
				new(8, 0.3f),
				new(9, 0.3f),
				new(10, 0.1f),
			},

			//column 4
			new List<RowCountPossibilities>
			{
				new(1, 0.0f),
				new(2, 0.3f),
				new(3, 0.5f),
				new(4, 0.9f),
				new(5, 0.9f),
				new(6, 0.5f),
				new(7, 0.5f),
				new(8, 0.3f),
				new(9, 0.3f),
				new(10, 0.1f),
			},

			//column 5
			new List<RowCountPossibilities>
			{
				new(1, 0.0f),
				new(2, 0.1f),
				new(3, 0.3f),
				new(4, 0.5f),
				new(5, 0.7f),
				new(6, 0.9f),
				new(7, 0.7f),
				new(8, 0.7f),
				new(9, 0.5f),
				new(10, 0.3f),
			},

			//column 6
			new List<RowCountPossibilities>
			{
				new(1, 0.0f),
				new(2, 0.1f),
				new(3, 0.3f),
				new(4, 0.5f),
				new(5, 0.7f),
				new(6, 0.9f),
				new(7, 0.7f),
				new(8, 0.7f),
				new(9, 0.5f),
				new(10, 0.3f),
			},

			//column 7
			new List<RowCountPossibilities>
			{
				new(1, 0.0f),
				new(2, 0.3f),
				new(3, 0.5f),
				new(4, 0.9f),
				new(5, 0.9f),
				new(6, 0.5f),
				new(7, 0.5f),
				new(8, 0.3f),
				new(9, 0.3f),
				new(10, 0.1f),
			},

			//column 8
			new List<RowCountPossibilities>
			{
				new(1, 0.0f),
				new(2, 0.5f),
				new(3, 0.7f),
				new(4, 0.9f),
				new(5, 0.7f),
				new(6, 0.7f),
				new(7, 0.5f),
				new(8, 0.3f),
				new(9, 0.1f),
				new(10, 0.0f),
			},

			//column 9
			new List<RowCountPossibilities>
			{
				new(1, 0.0f),
				new(2, 0.7f),
				new(3, 0.9f),
				new(4, 0.7f),
				new(5, 0.5f),
				new(6, 0.3f),
				new(7, 0.0f),
				new(8, 0.0f),
				new(9, 0.0f),
				new(10, 0.0f),
			},

			//column 10
			new List<RowCountPossibilities>
			{
				new(1, 0.0f),
				new(2, 0.9f),
				new(3, 0.6f),
				new(4, 0.3f),
				new(5, 0.0f),
				new(6, 0.0f),
				new(7, 0.0f),
				new(8, 0.0f),
				new(9, 0.0f),
				new(10, 0.0f),
			},
		};
	}

	public class RowCountPossibilities
	{
		public int RowCount { get; private set; }
		public float Chance { get; private set; }

		public RowCountPossibilities(int rowCount, float chance)
		{
			RowCount = rowCount;
			Chance = chance;
		}
	}
}
