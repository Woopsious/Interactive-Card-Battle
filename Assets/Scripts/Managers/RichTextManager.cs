using UnityEngine;
using static Woopsious.MapNodeData;
using static Woopsious.EntityData;
using NUnit;

namespace Woopsious
{
	public static class RichTextManager
	{
		public static string GetEncounterTypeTextColour(NodeEncounterType encounterType)
		{
			string text = "";
			switch (encounterType)
			{
				case NodeEncounterType.basicFight:
				text = "Basic Fight";
				break;
				case NodeEncounterType.eliteFight:
				text = "<color=#800080>Elite Fight</color>"; //Eldritch Purple
				break;
				case NodeEncounterType.bossFight:
				text = "<color=#C81919>Boss Fight</color>"; //Red
				break;
				case NodeEncounterType.eliteBossFight:
				text = "<color=#800080>Elite Boss Fight</color>"; //Eldritch Purple
				break;
				case NodeEncounterType.freeCardUpgrade:
				text = "<color=#00FFFF>Free Card Upgrade</color>"; //Cyan
				break;
			}

			return text;
		}

		public static string GetLandTypesTextColour(LandTypes landTypes)
		{
			string text = "";

			if (landTypes.HasFlag(LandTypes.grassland))
				text += "<color=green>Grasslands</color>, ";
			if (landTypes.HasFlag(LandTypes.hills))
				text += "<color=#7C9A61>Hills</color>, "; //Muted Green
			if (landTypes.HasFlag(LandTypes.forest))
				text += "<color=#006400>Forest</color>, "; //Dark Green
			if (landTypes.HasFlag(LandTypes.mountains))
				text += "<color=#B0B5B3>Mountains</color>, "; //Soft Grey
			if (landTypes.HasFlag(LandTypes.desert))
				text += "<color=#DCC38C>Desert</color>, "; //Golden Sand
			if (landTypes.HasFlag(LandTypes.tundra))
				text += "<color=#CDE2EA>Tundra</color>, "; //Icy Blue

			return RemoveLastComma(text);
		}

		public static string GetLandModifiersTextColour(LandModifiers landModifiers)
		{
			string text = "";

			if (landModifiers.HasFlag(LandModifiers.ruins))
				text += "<color=#00FFFF>Ruins</color>, "; //Cyan
			if (landModifiers.HasFlag(LandModifiers.town))
				text += "<color=#00FFFF>Town</color>, "; //Cyan
			if (landModifiers.HasFlag(LandModifiers.cursed))
				text += "<color=#00FFFF>Cursed</color>, "; //Cyan
			if (landModifiers.HasFlag(LandModifiers.volcanic))
				text += "<color=#00FFFF>Volcanic</color>, "; //Cyan
			if (landModifiers.HasFlag(LandModifiers.caves))
				text += "<color=#00FFFF>Caves</color>, "; //Cyan

			return RemoveLastComma(text);
		}

		public static string GetEnemyTypesTextColour(EnemyTypes enemyTypes)
		{
			string text = "";

			if (enemyTypes.HasFlag(EnemyTypes.slime))
				text += "<color=#90EE90>Slimes</color>, "; //light green
			if (enemyTypes.HasFlag(EnemyTypes.beast))
				text += "<color=#8B4513>Beasts</color>, "; //Earthy Brown
			if (enemyTypes.HasFlag(EnemyTypes.humanoid))
				text += "Humanoids, "; //default
			if (enemyTypes.HasFlag(EnemyTypes.construct))
				text += "<color=#2a3439>Constructs</color>, "; //Gun Metal
			if (enemyTypes.HasFlag(EnemyTypes.undead))
				text += "<color=#2F4F4F>Undead</color>, "; //Bloodless Gray
			if (enemyTypes.HasFlag(EnemyTypes.Abberration))
				text += "<color=#800080>Abberrations</color>, "; //Eldritch Purple

			return RemoveLastComma(text);
		}

		public static string RemoveLastComma(string input)
		{
			int lastCommaIndex = input.LastIndexOf(',');

			if (lastCommaIndex >= 0)
				return input.Remove(lastCommaIndex, 1);

			return input;
		}
	}
}
