using UnityEngine;
using static Woopsious.MapNodeDefinition;
using static Woopsious.EntityData;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Woopsious
{
	public class MapNodeController : MonoBehaviour
	{
		[Header("Runtime data")]
		public MapNodeDefinition mapNodeData;
		public MapNodeInstanceData instanceData;

		[Header("Linked Nodes")]
		public List<MapNodeController> previousLinkedNodes = new();
		public List<MapNodeController> nextLinkedNodes = new();
		public List<MapNodeController> siblingNodes = new();

		[Header("Debug options")]
		public bool debugRuntimeData;
		public bool forceEliteFight;
		public LandModifiers forceLandModifier;
		public ForceEncounterType forceEncounterType;
		public enum ForceEncounterType
		{
			noForceEncounter, forceBossFight, forceCardUpgrade
		}

		public event Action InitilizeUi;
		public event Action<NodeState> NodeStateChange;

		public void Initilize(int columnIndex, MapNodeDefinition mapNodeData, bool startingNode, bool bossFightNode)
		{
			this.mapNodeData = mapNodeData;

			instanceData = new(mapNodeData);
			instanceData.RandomizeEncounterType(bossFightNode);
			instanceData.RandomizedLandModifiers();
			instanceData.CheckIfMapEndNode(columnIndex, bossFightNode);

			instanceData.CalculateEncounterDifficulty(columnIndex);
			instanceData.CalculateCardRewardValues();

			instanceData.CalculateEntityBudget();
			instanceData.CalculatePossibleEnemies();

			if (startingNode)
				UpdateNodeState(NodeState.canTravel);
			else
				UpdateNodeState(NodeState.locked);

			InitilizeUi?.Invoke();
		}

		//node linking
		public void AddSiblingNodes(Dictionary<int, MapNodeController> siblingNodes)
		{
			for (int i = 0; i < siblingNodes.Count; i++)
			{
				if (siblingNodes[i] == this) continue;
				this.siblingNodes.Add(siblingNodes[i]);
			}
		}
		public void AddLinkToNextNode(MapNodeController mapNode)
		{
			nextLinkedNodes.Add(mapNode);
			mapNode.previousLinkedNodes.Add(this);
		}
		public void AddLinkToPreviousNode(MapNodeController mapNode)
		{
			previousLinkedNodes.Add(mapNode);
			mapNode.nextLinkedNodes.Add(this);
		}

		//start encounter
		public void BeginEncounter()
		{
			UpdateNodeState(NodeState.currentlyAt);

			foreach (MapNodeController previousNode in previousLinkedNodes) //lock prev nodes
			{
				if (previousNode.instanceData.nodeState == NodeState.currentlyAt)
					previousNode.UpdateNodeState(NodeState.previouslyVisited);
				else
					previousNode.UpdateNodeState(NodeState.locked);
			}

			foreach (MapNodeController nextNode in nextLinkedNodes) //unlock next nodes
				nextNode.UpdateNodeState(NodeState.canTravel);

			foreach (MapNodeController siblingNode in siblingNodes) //lock sibling nodes
				siblingNode.UpdateNodeState(NodeState.locked);

			if (instanceData.nodeEncounterType == NodeEncounterType.freeCardUpgrade)
				GameManager.EnterCardCombatWin();
			else
				GameManager.EnterCardCombat(this);
		}

		//update node state
		public void UpdateNodeState(NodeState nodeState)
		{
			instanceData.nodeState = nodeState;
			NodeStateChange?.Invoke(nodeState);
		}
	}
}
