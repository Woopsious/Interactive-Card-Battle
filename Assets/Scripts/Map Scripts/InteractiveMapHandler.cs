using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Woopsious
{
	public class InteractiveMapHandler : MonoBehaviour, IDragHandler
	{
		public Camera mapCamera;
		public RectTransform interactiveMapRectTransform;
		public RectTransform nodeLinksRectTransform;

		public bool logLandTypeSpawns;

		[Header("Map Node Prefabs")]
		public GameObject MapNodePrefab;
		public GameObject MapNodeLinks;
		private float heightOfNodes;
		private float widthOfNodes;

		private int totalMapNodeSpawnChance;

		//map size
		readonly Vector2 interactiveMapSize = new(1920 * 4, 1080 * 4); //map size consistant between screen sizes

		//camera zoom size
		readonly float cameraBaseOrthoSize = 540;
		readonly float maxCameraOrthoSize = 540 * 4;
		readonly float minCameraOrthoSize = 540 / 4;
		readonly float zoomSpeed = 500;

		//camera move limits
		readonly Vector2 maxCameraPosition = new(Screen.width * 2.5f, Screen.height * 2.5f);
		readonly Vector2 minCameraPosition = new(-Screen.width * 2.5f, -Screen.height * 2.5f);

		//runtime node table
		private readonly System.Random systemRandom = new();
		Dictionary<int, Dictionary<int, MapNode>> mapNodeTable = new();

		void Awake()
		{
			heightOfNodes = MapNodePrefab.GetComponent<RectTransform>().sizeDelta.y;
			widthOfNodes = MapNodePrefab.GetComponent<RectTransform>().sizeDelta.x;

			foreach (MapNodeData node in GameManager.instance.mapNodeDataTypes)
				totalMapNodeSpawnChance += (int)node.nodeSpawnChance;
		}

		void Start()
		{
			GenerateMapNodes();
			DebugLogSpawnedNodesLandTypesCount();
		}

		void OnEnable()
		{
			mapCamera.gameObject.SetActive(true);
		}
		void OnDisable()
		{
			mapCamera.gameObject.SetActive(false);
		}

		private void Update()
		{
			MapZoom();
		}

		//set up and generate map
		void GenerateMapNodes()
		{
			List<int> mapNodesToSpawnPerColumn = new()
			{
				1, 2, 2, 3, 6, 6, 5, 3, 3, 2
			};

			mapCamera.gameObject.SetActive(true);
			mapCamera.orthographicSize = cameraBaseOrthoSize;
			interactiveMapRectTransform.sizeDelta = interactiveMapSize;

			for (int columnIndex = 0; columnIndex < mapNodesToSpawnPerColumn.Count; columnIndex++)
			{
				Dictionary<int, MapNode> newColumn = GenerateMapColumn(columnIndex, mapNodesToSpawnPerColumn[columnIndex]);
				mapNodeTable.Add(columnIndex, newColumn);
				SetNodeColumnPositions(newColumn, mapNodesToSpawnPerColumn.Count, columnIndex);

				for (int rowIndex = 0; rowIndex < newColumn.Count; rowIndex++)
					newColumn[rowIndex].AddSiblingNodes(newColumn);

				if (columnIndex == 0) continue;
				LinkPreviousAndCurrentNodes(mapNodeTable[columnIndex - 1], mapNodeTable[columnIndex]);
			}
		}
		Dictionary<int, MapNode> GenerateMapColumn(int columnIndex, int nodesToSpawn)
		{
			Dictionary<int, MapNode> mapColumnNodes = new();

			for (int i = 0; i < nodesToSpawn; i++)
				mapColumnNodes.Add(i, GenerateMapNode(columnIndex));

			return mapColumnNodes;
		}
		MapNode GenerateMapNode(int columnIndex)
		{
			MapNode mapNode = Instantiate(MapNodePrefab, gameObject.transform).GetComponent<MapNode>();

			if (columnIndex == 0) //starting nodes
				mapNode.Initilize(columnIndex, GetWeightedMapNode(), true, false);
			else if (columnIndex == 9) //boss nodes
				mapNode.Initilize(columnIndex, GetWeightedMapNode(), false, true);
			else //standard nodes
				mapNode.Initilize(columnIndex, GetWeightedMapNode(), false, false);

			return mapNode;
		}
		MapNodeData GetWeightedMapNode()
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
		void DebugLogSpawnedNodesLandTypesCount()
		{
			if (!logLandTypeSpawns) return;
			Dictionary<MapNodeData.LandTypes, int> landTypeCount = new();

			foreach (var columnPair in mapNodeTable)
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

		//link nodes + handle row changes
		void LinkPreviousAndCurrentNodes(Dictionary<int, MapNode> previousColumn, Dictionary<int, MapNode> currentColumn)
		{
			int rowDifference = previousColumn.Count - currentColumn.Count;

			if (rowDifference < 0)
				HandleExtraRows(previousColumn, currentColumn);
			else if (rowDifference > 0)
				HandleLessRows(previousColumn, currentColumn);
			else
				HandleSameRows(previousColumn, currentColumn);
		}
		void HandleExtraRows(Dictionary<int, MapNode> previousColumn, Dictionary<int, MapNode> currentColumn)
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
		void HandleLessRows(Dictionary<int, MapNode> previousColumn, Dictionary<int, MapNode> currentColumn)
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
		void HandleSameRows(Dictionary<int, MapNode> previousColumn, Dictionary<int, MapNode> currentColumn)
		{
			for (int columnRow = 0; columnRow < currentColumn.Count; columnRow++)
			{
				previousColumn[columnRow].AddLinkToNextNode(currentColumn[columnRow]);
				SpawnNodeLinkObject(previousColumn[columnRow], currentColumn[columnRow]);
			}
		}

		//spawn visual links in
		void SpawnNodeLinkObject(MapNode prevMapNode, MapNode mapNode)
		{
			Vector2 posA = prevMapNode.GetComponent<RectTransform>().anchoredPosition;
			Vector2 posB = mapNode.GetComponent<RectTransform>().anchoredPosition;

			Vector2 direction = posB - posA;
			float distance = direction.magnitude;
			float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

			GameObject go = Instantiate(MapNodeLinks, nodeLinksRectTransform);
			RectTransform linkRect = go.GetComponent<RectTransform>();

			linkRect.sizeDelta = new Vector2(distance, linkRect.sizeDelta.y);
			linkRect.localRotation = Quaternion.Euler(0, 0, angle);
			linkRect.anchoredPosition = (posA + posB) / 2f;
		}

		//adjust map node positions
		void SetNodeColumnPositions(Dictionary<int, MapNode> nodesInColumn, int columnsCount, int columnIndex)
		{
			float spacingX = (interactiveMapSize.x - columnsCount * widthOfNodes) / (columnsCount + 1);
			float spacingY = (interactiveMapSize.y - nodesInColumn.Count * heightOfNodes) / (nodesInColumn.Count + 1);

			for (int rowIndex = 0; rowIndex < nodesInColumn.Count; rowIndex++)
				SetMapNodePosition(nodesInColumn[rowIndex].GetComponent<RectTransform>(), spacingX, spacingY, columnIndex + 1, rowIndex + 1);
		}
		void SetMapNodePosition(RectTransform rectTransform, float spacingX, float spacingY, int columnIndex, int rowIndex)
		{
			float offsetX = widthOfNodes * (columnIndex - 1) - interactiveMapSize.x / 2;
			float offsetY = heightOfNodes * (rowIndex - 1) - interactiveMapSize.y / 2;
			rectTransform.anchoredPosition = new Vector2(spacingX * columnIndex + offsetX, spacingY * rowIndex + offsetY);
		}

		//map drag and zoom
		void MapZoom()
		{
			mapCamera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
			mapCamera.orthographicSize = Mathf.Clamp(mapCamera.orthographicSize, minCameraOrthoSize, maxCameraOrthoSize);
		}
		public void OnDrag(PointerEventData eventData)
		{
			MapDrag(eventData);
		}
		void MapDrag(PointerEventData eventData)
		{
			Vector3 movement = new Vector3(eventData.delta.x, eventData.delta.y, 0) * 2;
			Vector3 cameraMovePos = mapCamera.transform.position - movement;

			cameraMovePos.x = Mathf.Clamp(cameraMovePos.x, minCameraPosition.x, maxCameraPosition.x);
			cameraMovePos.y = Mathf.Clamp(cameraMovePos.y, minCameraPosition.y, maxCameraPosition.y);

			mapCamera.transform.position = cameraMovePos;
		}
	}
}
