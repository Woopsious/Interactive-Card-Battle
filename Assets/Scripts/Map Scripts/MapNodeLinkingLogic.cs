using System.Collections.Generic;
using UnityEngine;

namespace Woopsious
{
	public class MapNodeLinkingLogic
	{
		private static readonly System.Random systemRandom = new();

		//create map node links
		public static List<NodeLinkData> CreateNodeLinkData(
			Dictionary<int, MapNodeController> previousColumn, Dictionary<int, MapNodeController> currentColumn)
		{
			int rowDifference = previousColumn.Count - currentColumn.Count;

			if (rowDifference < 0)
				return HandleExtraRows(previousColumn, currentColumn);
			else if (rowDifference > 0)
				return HandleLessRows(previousColumn, currentColumn);
			else
				return HandleSameRows(previousColumn, currentColumn);
		}
		private static List<NodeLinkData> HandleExtraRows(
			Dictionary<int, MapNodeController> previousColumn, Dictionary<int, MapNodeController> currentColumn)
		{
			List<NodeLinkData> nodeLinks = new();

			int nodeOffset = 0;
			int extraNodesToLink = currentColumn.Count - previousColumn.Count;
			float chanceForNoLink = 50;

			//link nodes in previous column to nodes in current column
			for (int prevColumnRow = 0; prevColumnRow < previousColumn.Count; prevColumnRow++)
			{
				nodeLinks.Add(new(previousColumn[prevColumnRow], currentColumn[nodeOffset]));
				nodeOffset++;

				if (extraNodesToLink == 0) continue;

				int nodesLeftToLink = previousColumn.Count - prevColumnRow;
				float roll;

				if (extraNodesToLink >= nodesLeftToLink)
					roll = 0f;
				else
					roll = (float)(systemRandom.NextDouble() * 100);

				if (roll > chanceForNoLink) continue;

				extraNodesToLink--;
				nodeLinks.Add(new(previousColumn[prevColumnRow], currentColumn[nodeOffset]));
				nodeOffset++;
			}

			return nodeLinks;
		}
		private static List<NodeLinkData> HandleLessRows(
			Dictionary<int, MapNodeController> previousColumn, Dictionary<int, MapNodeController> currentColumn)
		{
			List<NodeLinkData> nodeLinks = new();

			int nodeOffset = 0;
			int nodesToDoubleLink = previousColumn.Count - currentColumn.Count;
			float chanceForNoLink = 50;

			//link nodes in current column to nodes in previous column
			for (int curColumnRow = 0; curColumnRow < currentColumn.Count; curColumnRow++)
			{
				nodeLinks.Add(new(currentColumn[curColumnRow], previousColumn[nodeOffset]));
				nodeOffset++;

				if (nodesToDoubleLink == 0) continue;

				int nodesLeftToLink = currentColumn.Count - curColumnRow;
				float roll;

				if (nodesToDoubleLink >= nodesLeftToLink)
					roll = 0f;
				else
					roll = (float)(systemRandom.NextDouble() * 100);

				if (roll > chanceForNoLink) continue;

				nodesToDoubleLink--;
				nodeLinks.Add(new(currentColumn[curColumnRow], previousColumn[nodeOffset]));
				nodeOffset++;
			}

			return nodeLinks;
		}
		private static List<NodeLinkData> HandleSameRows(
			Dictionary<int, MapNodeController> previousColumn, Dictionary<int, MapNodeController> currentColumn)
		{
			List<NodeLinkData> nodeLinks = new();

			for (int columnRow = 0; columnRow < currentColumn.Count; columnRow++)
				nodeLinks.Add(new(previousColumn[columnRow], currentColumn[columnRow]));

			return nodeLinks;
		}
	}

	public class NodeLinkData
	{
		public MapNodeController node;
		public MapNodeController nodeToLinkTo;
		public NodeLinkData(MapNodeController node, MapNodeController nodeToLinkTo)
		{
			this.node = node;
			this.nodeToLinkTo = nodeToLinkTo;
		}
	}
}
