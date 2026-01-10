using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Woopsious
{
	public class InteractiveMapHandler : MonoBehaviour, IDragHandler
	{
		public static InteractiveMapHandler Instance;

		public Camera mapCamera;
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

		//camera zoom size
		readonly float cameraBaseOrthoSize = 540;
		readonly float maxCameraOrthoSize = 540 * 2.5f;
		readonly float minCameraOrthoSize = 540 / 2.5f;
		readonly float zoomSpeed = 500;

		//camera move limits
		private Vector2 maxCameraPosition = new(Screen.width, Screen.height);
		private Vector2 minCameraPosition = new(-Screen.width, -Screen.height);

		[Header("Runtime Data")]
		private readonly System.Random systemRandom = new();
		public Dictionary<int, Dictionary<int, MapNode>> MapNodeTable { get; private set; } = new();
		public List<GameObject> MapNodes = new();
		public List<GameObject> MapNodeLinks = new();

		void Awake()
		{
			Instance = this;
			heightOfNodes = MapNodePrefab.GetComponent<RectTransform>().sizeDelta.y;
			widthOfNodes = MapNodePrefab.GetComponent<RectTransform>().sizeDelta.x;

			foreach (MapNodeData node in GameManager.instance.mapNodeDataTypes)
				totalMapNodeSpawnChance += node.nodeSpawnChance;

			GameManager.OnGenerateNewMap += GenerateNewMap;
			mapCamera.gameObject.SetActive(true);
		}

		void OnDestroy()
		{
			GameManager.OnGenerateNewMap -= GenerateNewMap;
			mapCamera.gameObject.SetActive(false);
		}

		private void Update()
		{
			MapZoom();
		}

		private void GenerateNewMap()
		{
			CleanUpOldMap();
			SetMapSizeAndViewConstraints();

			/*
			List<int> mapNodesToSpawnPerColumn = new()
			{
				1, 2, 2, 3, 6, 6, 5, 3, 3, 2
			};
			*/

			List<int> mapNodesToSpawnPerColumn = new();

			bool start = true;
			int number = 0;
			for (int i = 0; i < 10; i++)
			{
				if (start)
				{
					number = RandomizeRowsInEachColumn(i, 1);
					start = false;
				}
				else
					number = RandomizeRowsInEachColumn(i, number);

				mapNodesToSpawnPerColumn.Add(number);
			}

			GenerateMapNodes(mapNodesToSpawnPerColumn);
		}
		private void CleanUpOldMap()
		{
			MapNodeTable.Clear();

			for (int i = MapNodes.Count - 1; i >= 0; i--)
				Destroy(MapNodes[i]);

			for (int i = MapNodeLinks.Count - 1; i >= 0; i--)
				Destroy(MapNodeLinks[i]);
		}
		private void SetMapSizeAndViewConstraints()
		{
			int viewWidth = (int)(Screen.width * 1.25f);
			int viewHeight = (int)(Screen.width * 1.25f);

			int widthOffset = Screen.width / 2;
			int heightOffset = Screen.height / 2;

			interactiveMapSize = new(Screen.width * 3, Screen.height * 3);
			maxCameraPosition = new(viewWidth + widthOffset, viewHeight + heightOffset);
			minCameraPosition = new(-viewWidth + widthOffset, -viewHeight + heightOffset);
		}

		//randomize map layout
		private int RandomizeRowsInEachColumn(int column, int previousColumnRowCount)
		{
			int minRowCount = previousColumnRowCount / 2;
			if (minRowCount % 2 != 0 && minRowCount != 1)
				minRowCount++;

			int maxRowCount = previousColumnRowCount * 2;
			if (maxRowCount > 10)
				maxRowCount = 10;

			List<RowCountPossibilities> rowCountChances = MapNodeTableChances.RowCountPossibilities[column];
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

		//create map nodes
		private void GenerateMapNodes(List<int> mapNodesToSpawnPerColumn)
		{
			mapCamera.gameObject.SetActive(true);
			mapCamera.orthographicSize = cameraBaseOrthoSize;
			interactiveMapRectTransform.sizeDelta = interactiveMapSize;

			for (int columnIndex = 0; columnIndex < mapNodesToSpawnPerColumn.Count; columnIndex++)
			{
				Dictionary<int, MapNode> newColumn = GenerateMapColumn(columnIndex, mapNodesToSpawnPerColumn[columnIndex]);
				MapNodeTable.Add(columnIndex, newColumn);
				SetNodeColumnPositions(newColumn, mapNodesToSpawnPerColumn.Count, columnIndex);

				for (int rowIndex = 0; rowIndex < newColumn.Count; rowIndex++)
					newColumn[rowIndex].AddSiblingNodes(newColumn);

				if (columnIndex == 0) continue;
				LinkPreviousAndCurrentNodes(MapNodeTable[columnIndex - 1], MapNodeTable[columnIndex]);
			}

			DebugLogSpawnedNodesLandTypesCount();
		}
		private Dictionary<int, MapNode> GenerateMapColumn(int columnIndex, int nodesToSpawn)
		{
			Dictionary<int, MapNode> mapColumnNodes = new();

			for (int i = 0; i < nodesToSpawn; i++)
				mapColumnNodes.Add(i, GenerateMapNode(columnIndex, $"C{columnIndex + 1}R{i + 1}"));

			return mapColumnNodes;
		}
		private MapNode GenerateMapNode(int columnIndex, string Id)
		{
			MapNode mapNode = Instantiate(MapNodePrefab, gameObject.transform).GetComponent<MapNode>();
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
		private MapNodeData GetWeightedMapNode()
		{
			float roll = (float)(systemRandom.NextDouble() * totalMapNodeSpawnChance);
			float cumulativeChance = 0;

			foreach (MapNodeData mapNodeData in GameManager.instance.mapNodeDataTypes)
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
			Dictionary<MapNodeData.LandTypes, int> landTypeCount = new();

			foreach (var columnPair in MapNodeTable)
			{
				foreach (var nodePair in columnPair.Value)
				{
					MapNode node = nodePair.Value;

					// Replace this with your actual node type field/property
					MapNodeData.LandTypes type = node.landTypes;

					if (!landTypeCount.ContainsKey(type))
						landTypeCount[type] = 0;

					landTypeCount[type]++;
				}
			}

			foreach (var kvp in landTypeCount)
				Debug.LogError($"Land Type: {kvp.Key}, Count: {kvp.Value}");
		}

		//create map node links
		private void LinkPreviousAndCurrentNodes(Dictionary<int, MapNode> previousColumn, Dictionary<int, MapNode> currentColumn)
		{
			int rowDifference = previousColumn.Count - currentColumn.Count;

			if (rowDifference < 0)
				HandleExtraRows(previousColumn, currentColumn);
			else if (rowDifference > 0)
				HandleLessRows(previousColumn, currentColumn);
			else
				HandleSameRows(previousColumn, currentColumn);
		}
		private void HandleExtraRows(Dictionary<int, MapNode> previousColumn, Dictionary<int, MapNode> currentColumn)
		{
			int nodeOffset = 0;
			int extraNodesToLink = currentColumn.Count - previousColumn.Count;
			float chanceForNoLink = 50;

			//link nodes in previous column to nodes in current column
			for (int prevColumnRow = 0; prevColumnRow < previousColumn.Count; prevColumnRow++)
			{
				previousColumn[prevColumnRow].AddLinkToNextNode(currentColumn[nodeOffset]);
				SpawnNodeLinkObject(previousColumn[prevColumnRow], currentColumn[nodeOffset]);
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
				previousColumn[prevColumnRow].AddLinkToNextNode(currentColumn[nodeOffset]);
				SpawnNodeLinkObject(previousColumn[prevColumnRow], currentColumn[nodeOffset]);
				nodeOffset++;
			}
		}
		private void HandleLessRows(Dictionary<int, MapNode> previousColumn, Dictionary<int, MapNode> currentColumn)
		{
			int nodeOffset = 0;
			int nodesToDoubleLink = previousColumn.Count - currentColumn.Count;
			float chanceForNoLink = 50;

			//link nodes in current column to nodes in previous column
			for (int curColumnRow = 0; curColumnRow < currentColumn.Count; curColumnRow++)
			{
				currentColumn[curColumnRow].AddLinkToPreviousNode(previousColumn[nodeOffset]);
				SpawnNodeLinkObject(currentColumn[curColumnRow], previousColumn[nodeOffset]);
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
				currentColumn[curColumnRow].AddLinkToPreviousNode(previousColumn[nodeOffset]);
				SpawnNodeLinkObject(currentColumn[curColumnRow], previousColumn[nodeOffset]);
				nodeOffset++;
			}
		}
		private void HandleSameRows(Dictionary<int, MapNode> previousColumn, Dictionary<int, MapNode> currentColumn)
		{
			for (int columnRow = 0; columnRow < currentColumn.Count; columnRow++)
			{
				previousColumn[columnRow].AddLinkToNextNode(currentColumn[columnRow]);
				SpawnNodeLinkObject(previousColumn[columnRow], currentColumn[columnRow]);
			}
		}
		private void SpawnNodeLinkObject(MapNode prevMapNode, MapNode mapNode)
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

		//adjust map node positions
		private void SetNodeColumnPositions(Dictionary<int, MapNode> nodesInColumn, int columnsCount, int columnIndex)
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

		//map drag and zoom
		private void MapZoom()
		{
			mapCamera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
			mapCamera.orthographicSize = Mathf.Clamp(mapCamera.orthographicSize, minCameraOrthoSize, maxCameraOrthoSize);
		}
		public void OnDrag(PointerEventData eventData)
		{
			MapDrag(eventData);
		}
		private void MapDrag(PointerEventData eventData)
		{
			Vector3 movement = new Vector3(eventData.delta.x, eventData.delta.y, 0) * 2;
			Vector3 cameraMovePos = mapCamera.transform.position - movement;

			cameraMovePos.x = Mathf.Clamp(cameraMovePos.x, minCameraPosition.x, maxCameraPosition.x);
			cameraMovePos.y = Mathf.Clamp(cameraMovePos.y, minCameraPosition.y, maxCameraPosition.y);

			mapCamera.transform.position = cameraMovePos;
		}
	}
}
