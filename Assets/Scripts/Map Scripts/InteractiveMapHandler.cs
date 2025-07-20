using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Woopsious
{
	public class InteractiveMapHandler : MonoBehaviour, IDragHandler
	{
		public Camera mapCamera;
		public RectTransform interactiveMapRectTransform;

		public bool logLandTypeSpawns;

		[Header("Map Node Spawn Data")]
		public GameObject MapNodePrefab;
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
		void GenerateMapNodes(int mapColumnsCount = 10)
		{
			mapCamera.gameObject.SetActive(true);
			mapCamera.orthographicSize = cameraBaseOrthoSize;
			interactiveMapRectTransform.sizeDelta = interactiveMapSize;

			for (int columnIndex = 0; columnIndex < mapColumnsCount; columnIndex++)
			{
				Dictionary<int, MapNode> newColumn = GenerateMapColumn(columnIndex);
				mapNodeTable.Add(columnIndex, newColumn);
				SetNodeColumnPositions(newColumn, mapColumnsCount, columnIndex);
			}
		}
		Dictionary<int, MapNode> GenerateMapColumn(int columnIndex)
		{
			Dictionary<int, MapNode> mapColumnNodes = new();

			if (columnIndex == 0)
				mapColumnNodes.Add(0, GenerateMapNode(columnIndex));
			else if (columnIndex > 0 && columnIndex <= 3)
			{
				mapColumnNodes.Add(0, GenerateMapNode(columnIndex));
				mapColumnNodes.Add(1, GenerateMapNode(columnIndex));
			}
			else if (columnIndex > 3 && columnIndex <= 6)
			{
				mapColumnNodes.Add(0, GenerateMapNode(columnIndex));
				mapColumnNodes.Add(1, GenerateMapNode(columnIndex));
				mapColumnNodes.Add(2, GenerateMapNode(columnIndex));
			}
			else if (columnIndex > 6 && columnIndex <= 8)
			{
				mapColumnNodes.Add(0, GenerateMapNode(columnIndex));
				mapColumnNodes.Add(1, GenerateMapNode(columnIndex));
				mapColumnNodes.Add(2, GenerateMapNode(columnIndex));
				mapColumnNodes.Add(3, GenerateMapNode(columnIndex));
			}
			else if (columnIndex > 8)
			{
				mapColumnNodes.Add(0, GenerateMapNode(columnIndex));
				mapColumnNodes.Add(1, GenerateMapNode(columnIndex));
			}

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
		void LinkNodesTogether()
		{

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
			float offsetX = widthOfNodes * (columnIndex - 1);
			float offsetY = heightOfNodes * (rowIndex - 1);
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
