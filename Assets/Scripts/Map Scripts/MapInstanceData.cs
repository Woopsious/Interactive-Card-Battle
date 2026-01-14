using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Woopsious
{
	[System.Serializable]
	public class MapInstanceData
	{
		private static readonly System.Random systemRandom = new();
		private static readonly List<List<RowCountPossibilities>> RowCountPossibilities = new()
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

		//public List<MapColumnData> MapTableData = new();
		public List<List<MapNodeInstanceData>> MapTable = new();

		public void GenerateMapLayout()
		{
			List<int> columnRowCounts = new();

			for (int column = 0; column < 10; column++)
			{
				if (column == 0)
					columnRowCounts.Add(RandomizeRowsInEachColumn(column, 1));
				else
					columnRowCounts.Add(RandomizeRowsInEachColumn(column, columnRowCounts[^1]));
			}

			GenerateNodeInstances(columnRowCounts);
		}
		private void GenerateNodeInstances(List<int> columnRowCounts)
		{
			for (int column = 0; column < 10; column++)
			{
				List<MapNodeInstanceData> columnNodeInstances = new();

				for (int row = 0; row < columnRowCounts[column]; row++)
					columnNodeInstances.Add(new(column));

				MapTable.Add(columnNodeInstances);
			}
		}
		private static int RandomizeRowsInEachColumn(int column, int previousColumnRowCount)
		{
			int minRowCount = previousColumnRowCount / 2;
			if (minRowCount % 2 != 0 && minRowCount != 1)
				minRowCount++;

			int maxRowCount = previousColumnRowCount * 2;
			if (maxRowCount > 10)
				maxRowCount = 10;

			List<RowCountPossibilities> rowCountChances = RowCountPossibilities[column];
			List<RowCountPossibilities> validRowCounts = new();
			float totalChance = 0;

			foreach (RowCountPossibilities rowCountPossibilities in rowCountChances)
			{
				if (rowCountPossibilities.RowCount > minRowCount && rowCountPossibilities.RowCount <= maxRowCount)
				{
					validRowCounts.Add(rowCountPossibilities);
					totalChance += rowCountPossibilities.Chance;
				}
			}

			float roll = (float)systemRandom.NextDouble() * totalChance;
			float cumulativeChance = 0;

			for (int i = 0; i < validRowCounts.Count; i++)
			{
				cumulativeChance += validRowCounts[i].Chance;

				if (roll <= cumulativeChance)
					return validRowCounts[i].RowCount;
			}

			return validRowCounts[^1].RowCount;
		}
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
