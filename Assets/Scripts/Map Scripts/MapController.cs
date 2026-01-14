using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Woopsious
{
	public class MapController : MonoBehaviour
	{
		public static MapController Instance;

		public RectTransform interactiveMapRectTransform;
		public RectTransform nodeLinksRectTransform;

		public bool logLandTypeSpawns;

		[Header("Map Node Prefabs")]
		public GameObject MapNodePrefab;
		public GameObject MapNodeLinkPrefab;
		private float heightOfNodes;
		private float widthOfNodes;

		public float TotalNodeSpawnChance { get; private set; }

		//map size
		private Vector2 interactiveMapSize = new(Screen.width * 3, Screen.height * 3);

		[Header("Runtime Data")]
		public Dictionary<int, Dictionary<int, MapNodeController>> MapNodeTable { get; private set; } = new();
		public MapInstanceData mapInstanceData;
		public List<GameObject> MapNodes = new();
		public List<GameObject> MapNodeLinks = new();

		void Awake()
		{
			Instance = this;
			heightOfNodes = MapNodePrefab.GetComponent<RectTransform>().sizeDelta.y;
			widthOfNodes = MapNodePrefab.GetComponent<RectTransform>().sizeDelta.x;

			foreach (MapNodeDefinition node in GameManager.instance.mapNodeDataTypes)
				TotalNodeSpawnChance += node.nodeSpawnChance;

			GameManager.OnGenerateNewMap += GenerateNewMap;
		}

		void OnDestroy()
		{
			GameManager.OnGenerateNewMap -= GenerateNewMap;
		}

		private void GenerateNewMap()
		{
			CleanUpOldMap();

			interactiveMapSize = new(Screen.width * 3, Screen.height * 3);
			interactiveMapRectTransform.sizeDelta = interactiveMapSize;

			mapInstanceData = new MapInstanceData();
			mapInstanceData.GenerateMapLayout();

			GenerateMapNodes();
		}
		private void CleanUpOldMap()
		{
			for (int i = MapNodes.Count - 1; i >= 0; i--)
				Destroy(MapNodes[i]);

			for (int i = MapNodeLinks.Count - 1; i >= 0; i--)
				Destroy(MapNodeLinks[i]);

			MapNodeTable.Clear();
			MapNodes.Clear();
			MapNodeLinks.Clear();
		}

		//create map nodes
		private void GenerateMapNodes()
		{
			List<List<MapNodeInstanceData>> table = mapInstanceData.MapTable;
			for (int column = 0; column < mapInstanceData.MapTable.Count; column++)
			{
				Dictionary<int, MapNodeController> columnNodes = new();

				for (int row = 0; row < table[column].Count; row++)
				{
					MapNodeController mapNode = Instantiate(MapNodePrefab, gameObject.transform).GetComponent<MapNodeController>();
					mapNode.name = $"MapNode-C{column}R{row}";
					mapNode.Initilize(table[column][row]);

					columnNodes.Add(row, mapNode);
					MapNodes.Add(mapNode.gameObject);
				}

				MapNodeTable.Add(column, columnNodes);
				SetNodeColumnPositions(columnNodes, table.Count, column);

				foreach (MapNodeController node in columnNodes.Values)
					node.AddSiblingNodes(columnNodes);

				if (column == 0) continue;

				List<NodeLinkData> nodesToLink = MapNodeLinkingLogic.CreateNodeLinkData(
					MapNodeTable[column - 1], MapNodeTable[column]);

				foreach (NodeLinkData node in nodesToLink)
				{
					SpawnNodeLinkObject(node.node, node.nodeToLinkTo);
					node.node.AddLinkToNextNode(node.nodeToLinkTo);
				}
			}

			DebugLogSpawnedNodesLandTypesCount();
		}

		//debug
		private void DebugLogSpawnedNodesLandTypesCount()
		{
			if (!logLandTypeSpawns) return;
			Dictionary<MapNodeDefinition.LandTypes, int> landTypeCount = new();

			foreach (var columnPair in MapNodeTable)
			{
				foreach (var nodePair in columnPair.Value)
				{
					MapNodeController node = nodePair.Value;

					// Replace this with your actual node type field/property
					MapNodeDefinition.LandTypes type = node.instanceData.landTypes;

					if (!landTypeCount.ContainsKey(type))
						landTypeCount[type] = 0;

					landTypeCount[type]++;
				}
			}

			foreach (var kvp in landTypeCount)
				Debug.LogError($"Land Type: {kvp.Key}, Count: {kvp.Value}");
		}

		//adjust map node positions
		private void SetNodeColumnPositions(Dictionary<int, MapNodeController> nodesInColumn, int columnsCount, int columnIndex)
		{
			float spacingX = (interactiveMapSize.x - columnsCount * widthOfNodes) / (columnsCount + 1);
			float spacingY = (interactiveMapSize.y - nodesInColumn.Count * heightOfNodes) / (nodesInColumn.Count + 1);

			for (int rowIndex = 0; rowIndex < nodesInColumn.Count; rowIndex++)
				SetMapNodePosition(nodesInColumn[rowIndex].GetComponent<RectTransform>(), spacingX, spacingY, columnIndex + 1, rowIndex + 1);
		}
		private void SetMapNodePosition(RectTransform rectTransform, float spacingX, float spacingY, int columnIndex, int rowIndex)
		{
			float offsetX = widthOfNodes * (columnIndex - 1) - interactiveMapSize.x / 2;
			float offsetY = heightOfNodes * (rowIndex - 1) - interactiveMapSize.y / 2;
			rectTransform.anchoredPosition = new Vector2(spacingX * columnIndex + offsetX, spacingY * rowIndex + offsetY);
		}

		private void SpawnNodeLinkObject(MapNodeController prevMapNode, MapNodeController mapNode)
		{
			Vector2 posA = prevMapNode.GetComponent<RectTransform>().anchoredPosition;
			Vector2 posB = mapNode.GetComponent<RectTransform>().anchoredPosition;

			Vector2 direction = posB - posA;
			float distance = direction.magnitude;
			float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

			GameObject go = Instantiate(MapNodeLinkPrefab, nodeLinksRectTransform);
			MapNodeLinks.Add(go);
			RectTransform linkRect = go.GetComponent<RectTransform>();

			linkRect.sizeDelta = new Vector2(distance, linkRect.sizeDelta.y);
			linkRect.localRotation = Quaternion.Euler(0, 0, angle);
			linkRect.anchoredPosition = (posA + posB) / 2f;
		}
	}
}
