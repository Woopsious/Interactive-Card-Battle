using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Woopsious;

public class InteractiveMapHandler : MonoBehaviour, IDragHandler
{
	public Camera mapCamera;
	public RectTransform interactiveMapRectTransform;

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

	//runtime nodes
	public Dictionary<MapNode, int> columnOneStartingNodes = new();
	public Dictionary<MapNode, int> columnTwoNodes = new();
	public Dictionary<MapNode, int> columnThreeNodes = new();

	/// <summary>
	/// MAP NODE PLACEMENTS
	/// COLUMN 1: 
	///		ROW 1: one starting node
	/// COLUMN 2: 
	///		ROW 1: connects to starting node
	///		ROW 2: connects to starting node
	/// COLUMN 3:	
	/// 	ROW 1: connects to previous row 1 node
	///		ROW 2: connects to previous row 2 node
	/// COLUMN 4:
	/// 	ROW 1: connects to previous row 1 node
	///		ROW 2: connects to previous row 2 node
	/// COLUMN 5:
	/// 	ROW 1: connects to previous row 1 node
	///		ROW 2: connects to previous row 1 and 2 nodes
	///		ROW 3: connects to previous row 2 node
	/// COLUMN 6:
	/// 	ROW 1: connects to previous row 1 node
	///		ROW 2: connects to previous row 2 node
	///		ROW 3: connects to previous row 3 node
	/// COLUMN 7:
	/// 	ROW 1: connects to previous row 1 node
	///		ROW 2: connects to previous row 2 node
	///		ROW 3: connects to previous row 3 node
	/// COLUMN 8:
	/// 	ROW 1: connects to previous row 1 node
	///		ROW 2: connects to previous row 1 and 2 nodes
	///		ROW 3: connects to previous row 2 and 3 nodes
	///		ROW 4: connects to previous row 4 node
	/// COLUMN 9:
	/// 	ROW 1: connects to previous row 1 node
	///		ROW 2: connects to previous row 2 node
	///		ROW 3: connects to previous row 3 node
	///		ROW 4: connects to previous row 4 node
	/// COLUMN 10:
	/// 	ROW 1: connects to previous row 1 and 2 nodes
	///		ROW 2: connects to previous row 2 and 3 nodes
	///		ROW 3: connects to previous row 3 and 4 nodes
	/// </summary>

	void Awake()
	{
		heightOfNodes = MapNodePrefab.GetComponent<RectTransform>().sizeDelta.y;
		widthOfNodes = MapNodePrefab.GetComponent<RectTransform>().sizeDelta.x;

		foreach (MapNodeData node in mapNodeDataTypes)
			totalMapNodeSpawnChance += (int)node.nodeSpawnChance;
	}

	void Start()
	{
		SetupMapAndGenerateMap();
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
	void SetupMapAndGenerateMap()
	{
		mapCamera.gameObject.SetActive(true);
		mapCamera.orthographicSize = cameraBaseOrthoSize;
		interactiveMapRectTransform.sizeDelta = interactiveMapSize;

		int columnIndex = 0;

		while (columnIndex < 3)
		{
			GenerateMapColumn(columnIndex);
			columnIndex++;
		}

		List<Dictionary<MapNode, int>> listOfNodeColumns = new()
		{
				columnOneStartingNodes, columnTwoNodes, columnThreeNodes
		};

		columnIndex = 0;

		foreach (Dictionary<MapNode, int> nodeColumns in listOfNodeColumns)
		{
			SetMapNodePositions(nodeColumns, 3, columnIndex);
			columnIndex++;
		}
	}
	void GenerateMapColumn(int columnIndex)
	{
		switch (columnIndex)
		{
			case 0:
			GenerateMapNode(columnIndex, 0);
			break;
			case 1:
			GenerateMapNode(columnIndex, 0);
			GenerateMapNode(columnIndex, 1);
			break;
			case 2:
			GenerateMapNode(columnIndex, 0);
			GenerateMapNode(columnIndex, 1);
			break;
		}
	}
	void GenerateMapNode(int columnIndex, int rowIndex)
	{
		MapNode mapNode = Instantiate(MapNodePrefab, gameObject.transform).GetComponent<MapNode>();

		if (columnIndex == 0) //starting nodes
			mapNode.Initilize(GetWeightedMapNode(), true, false);
		else if (columnIndex == 2) //boss nodes
			mapNode.Initilize(GetWeightedMapNode(), false, true);
		else //standard nodes
			mapNode.Initilize(GetWeightedMapNode(), false, false);

		switch (columnIndex)
		{
			case 0:
			columnOneStartingNodes.Add(mapNode, rowIndex);
			break;
			case 1:
			columnTwoNodes.Add(mapNode, rowIndex);
			break;
			case 2:
			columnThreeNodes.Add(mapNode, rowIndex);
			break;
		}
	}
	MapNodeData GetWeightedMapNode()
	{
		float roll = UnityEngine.Random.Range(0, totalMapNodeSpawnChance);
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

	//adjust map node positions
	void SetMapNodePositions(Dictionary<MapNode, int> nodesInColumn, int columnsCount, int columnIndex)
	{
		float spacingX = (interactiveMapSize.x - columnsCount * widthOfNodes) / (columnsCount + 1);
		float spacingY = (interactiveMapSize.y - nodesInColumn.Count * heightOfNodes) / (nodesInColumn.Count + 1);

		int rowIndex = 0;

		foreach (MapNode mapNode in nodesInColumn.Keys)
		{
			SetMapNodePosition(mapNode.GetComponent<RectTransform>(), spacingX, spacingY, columnIndex + 1, rowIndex + 1);
			rowIndex++;
		}
	}
	void SetMapNodePosition(RectTransform rectTransform, float spacingX, float spacingY, int columnIndex, int rowIndex)
	{
		float offsetX = widthOfNodes * (columnIndex - 1);
		float offsetY = heightOfNodes * (rowIndex - 1);
		rectTransform.anchoredPosition = new Vector2(spacingX * columnIndex + offsetX, spacingY * rowIndex + offsetY);

		Debug.LogError("map node position: " + rectTransform.anchoredPosition);
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
