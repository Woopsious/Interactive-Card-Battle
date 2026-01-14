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

		private float totalMapNodeSpawnChance;

		//map size
		private Vector2 interactiveMapSize = new(Screen.width * 3, Screen.height * 3);

		[Header("Runtime Data")]
		private readonly System.Random systemRandom = new();
		public Dictionary<int, Dictionary<int, MapNodeController>> MapNodeTable { get; private set; } = new();
		public List<GameObject> MapNodes = new();
		public List<GameObject> MapNodeLinks = new();

		void Awake()
		{
			Instance = this;
			heightOfNodes = MapNodePrefab.GetComponent<RectTransform>().sizeDelta.y;
			widthOfNodes = MapNodePrefab.GetComponent<RectTransform>().sizeDelta.x;

			foreach (MapNodeDefinition node in GameManager.instance.mapNodeDataTypes)
				totalMapNodeSpawnChance += node.nodeSpawnChance;

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
			List<int> rowsPerColumnList = MapInstanceData.GenerateMapLayout();

			for (int columnIndex = 0; columnIndex < rowsPerColumnList.Count; columnIndex++)
			{
				Dictionary<int, MapNodeController> newColumn = GenerateMapColumn(columnIndex, rowsPerColumnList[columnIndex]);
				MapNodeTable.Add(columnIndex, newColumn);
				SetNodeColumnPositions(newColumn, rowsPerColumnList.Count, columnIndex);

				for (int rowIndex = 0; rowIndex < newColumn.Count; rowIndex++)
					newColumn[rowIndex].AddSiblingNodes(newColumn);

				if (columnIndex == 0) continue;

				List<NodeLinkData> nodesToLink = MapNodeLinkingLogic.CreateNodeLinkData(
					MapNodeTable[columnIndex - 1], MapNodeTable[columnIndex]);

				foreach (NodeLinkData node in nodesToLink)
				{
					SpawnNodeLinkObject(node.node, node.nodeToLinkTo);
					node.node.AddLinkToNextNode(node.nodeToLinkTo);
				}
			}

			DebugLogSpawnedNodesLandTypesCount();
		}
		private Dictionary<int, MapNodeController> GenerateMapColumn(int columnIndex, int nodesToSpawn)
		{
			Dictionary<int, MapNodeController> mapColumnNodes = new();

			for (int i = 0; i < nodesToSpawn; i++)
				mapColumnNodes.Add(i, GenerateMapNode(columnIndex, $"C{columnIndex}R{i}"));

			return mapColumnNodes;
		}
		private MapNodeController GenerateMapNode(int columnIndex, string Id)
		{
			MapNodeController mapNode = Instantiate(MapNodePrefab, gameObject.transform).GetComponent<MapNodeController>();
			mapNode.name = "MapNode" + Id;

			if (columnIndex == 0) //starting nodes
				mapNode.Initilize(columnIndex, GetWeightedMapNode(), true, false);
			else if (columnIndex == 9) //boss nodes
				mapNode.Initilize(columnIndex, GetWeightedMapNode(), false, true);
			else //standard nodes
				mapNode.Initilize(columnIndex, GetWeightedMapNode(), false, false);

			MapNodes.Add(mapNode.gameObject);
			return mapNode;
		}
		private MapNodeDefinition GetWeightedMapNode()
		{
			float roll = (float)(systemRandom.NextDouble() * totalMapNodeSpawnChance);
			float cumulativeChance = 0;

			foreach (MapNodeDefinition mapNodeData in GameManager.instance.mapNodeDataTypes)
			{
				cumulativeChance += mapNodeData.nodeSpawnChance;

				if (roll <= cumulativeChance)
					return mapNodeData;
			}

			Debug.LogError("Failed to get weighted map node to spawn, spawning default");
			return GameManager.instance.mapNodeDataTypes[0];
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
