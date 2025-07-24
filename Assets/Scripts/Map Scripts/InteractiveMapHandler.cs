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

		[Header("Map Node Scriptable Objects")]
		public List<MapNodeData> mapNodeDataTypes = new();
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

			foreach (MapNodeData node in mapNodeDataTypes)
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
				1, 2, 2, 3, 6, 8, 8, 11
			};

			mapCamera.gameObject.SetActive(true);
			mapCamera.orthographicSize = cameraBaseOrthoSize;
			interactiveMapRectTransform.sizeDelta = interactiveMapSize;

			for (int columnIndex = 0; columnIndex < mapNodesToSpawnPerColumn.Count; columnIndex++)
			{
				Dictionary<int, MapNode> newColumn = GenerateMapColumn(columnIndex, mapNodesToSpawnPerColumn[columnIndex]);
				mapNodeTable.Add(columnIndex, newColumn);
				SetNodeColumnPositions(newColumn, mapNodesToSpawnPerColumn.Count, columnIndex);

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
				mapNode.Initilize(GetWeightedMapNode(), true, false);
			else if (columnIndex == 2) //boss nodes
				mapNode.Initilize(GetWeightedMapNode(), false, true);
			else //standard nodes
				mapNode.Initilize(GetWeightedMapNode(), false, false);

			return mapNode;
		}
		MapNodeData GetWeightedMapNode()
		{
			float roll = (float)(systemRandom.NextDouble() * totalMapNodeSpawnChance);
			float cumulativeChance = 0;

			foreach (MapNodeData mapNodeData in mapNodeDataTypes)
			{
				cumulativeChance += mapNodeData.nodeSpawnChance;

				if (roll <= cumulativeChance)
					return mapNodeData;
			}

			Debug.LogError("Failed to get weighted map node to spawn, spawning default");
			return mapNodeDataTypes[0];
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

		//link previous and next nodes
		void LinkPreviousAndCurrentNodes(Dictionary<int, MapNode> previousColumn, Dictionary<int, MapNode> currentColumn)
		{
			int rowDifference = currentColumn.Count - previousColumn.Count;
			int rowRemainder = previousColumn.Count % currentColumn.Count;

			if (rowDifference == 0)
			{
				HandleSameRows(previousColumn, currentColumn);
			}
			else if (rowRemainder == 0 || rowDifference == 1 || rowDifference == -1)
			{
				HandleExtraOrDoubleRows(previousColumn, currentColumn, rowDifference);
				//Debug.LogError("Handling Extra/Double Row");
			}
			else
			{
				HandleRemainderRows(previousColumn, currentColumn, rowDifference);
				//Debug.LogError("Handling Remainder Rows");
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
		void HandleExtraOrDoubleRows(Dictionary<int, MapNode> previousColumn, Dictionary<int, MapNode> currentColumn, int rowDifference)
		{
			if (rowDifference > 0)
			{
				int nodeLinks = currentColumn.Count / previousColumn.Count;

				for (int prevColumnRow = 0; prevColumnRow < previousColumn.Count; prevColumnRow++)
				{
					int nodeOffset = nodeLinks * prevColumnRow;

					previousColumn[prevColumnRow].AddLinkToNextNode(currentColumn[nodeOffset]);
					SpawnNodeLinkObject(previousColumn[prevColumnRow], currentColumn[nodeOffset]);

					nodeOffset += 1;

					previousColumn[prevColumnRow].AddLinkToNextNode(currentColumn[nodeOffset]);
					SpawnNodeLinkObject(previousColumn[prevColumnRow], currentColumn[nodeOffset]);
				}
			}
			else
			{

			}
		}
		void HandleRemainderRows(Dictionary<int, MapNode> previousColumn, Dictionary<int, MapNode> currentColumn, int rowDifference)
		{
			if (rowDifference > 0)
			{
				int nodeOffset = 0;
				int extraNodesToLink = currentColumn.Count - previousColumn.Count;
				float chanceForNoLink = ((float)currentColumn.Count - previousColumn.Count) / previousColumn.Count * 100;

				for (int prevColumnRow = 0; prevColumnRow < previousColumn.Count; prevColumnRow++)
				{
					previousColumn[prevColumnRow].AddLinkToNextNode(currentColumn[nodeOffset]);
					SpawnNodeLinkObject(previousColumn[prevColumnRow], currentColumn[nodeOffset]);
					nodeOffset++;

					if (extraNodesToLink == 0) continue;

					float roll = (float)(systemRandom.NextDouble() * totalMapNodeSpawnChance);
					int nodesLeftToLink = previousColumn.Count - prevColumnRow - 1;

					if (extraNodesToLink >= nodesLeftToLink)
						roll = 0f;

					if (roll > chanceForNoLink) continue;

					extraNodesToLink--;
					previousColumn[prevColumnRow].AddLinkToNextNode(currentColumn[nodeOffset]);
					SpawnNodeLinkObject(previousColumn[prevColumnRow], currentColumn[nodeOffset]);
					nodeOffset++;
				}
			}
			else
			{

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
