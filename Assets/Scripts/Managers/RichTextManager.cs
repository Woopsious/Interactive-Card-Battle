using UnityEngine;
using static Woopsious.MapNodeData;
using static Woopsious.EntityData;

namespace Woopsious
{
	public static class RichTextManager
	{
		//shared colours
		public static string cyan = "#00FFFF";   // land modifiers, Free Card Upgrade
		public static string eldritchPurple = "#800080";  // Elite Fight, Elite Boss Fight, Abberrations

		//encounter type colours
		public static string red = "#C81919";             // Boss Fight

		//land type colours
		public static string green = "green";         // Grasslands
		public static string mutedGreen = "#7C9A61";  // Hills
		public static string darkGreen = "#006400";   // Forest
		public static string softGrey = "#B0B5B3";    // Mountains
		public static string goldenSand = "#DCC38C";  // Desert
		public static string icyBlue = "#CDE2EA";     // Tundra

		//land modifier colours

		//enemy type colours
		public static string lightGreen = "#90EE90";   // Slimes
		public static string earthyBrown = "#8B4513";  // Beasts
		public static string defaultColor = "#FFFFFF"; // Humanoids (default, no color applied)
		public static string gunMetalGray = "#2a3439";     // Constructs
		public static string bloodlessGray = "#2F4F4F"; // Undead

		//other colours
		public static string crimsonRed = "#8B0000";
		public static string steelBlue = "#4682B4";

		public static string GetEncounterTypeTextColour(NodeEncounterType encounterType)
		{
			string text = "";
			switch (encounterType)
			{
				case NodeEncounterType.basicFight:
				text = "Basic Fight";
				break;
				case NodeEncounterType.eliteFight:
				text = $"<color={eldritchPurple}>Elite Fight</color>";
				break;
				case NodeEncounterType.bossFight:
				text = $"<color={red}>Boss Fight</color>";
				break;
				case NodeEncounterType.eliteBossFight:
				text = $"<color={eldritchPurple}>Elite Boss Fight</color>";
				break;
				case NodeEncounterType.freeCardUpgrade:
				text = $"<color={cyan}>Free Card Upgrade</color>";
				break;
			}

			return text;
		}

		public static string GetLandTypesTextColour(LandTypes landTypes)
		{
			string text = "";

			if (landTypes.HasFlag(LandTypes.grassland))
				text += $"<color={green}>Grasslands</color>, ";
			if (landTypes.HasFlag(LandTypes.hills))
				text += $"<color={mutedGreen}>Hills</color>, ";
			if (landTypes.HasFlag(LandTypes.forest))
				text += $"<color={darkGreen}>Forest</color>, ";
			if (landTypes.HasFlag(LandTypes.mountains))
				text += $"<color={softGrey}>Mountains</color>, ";
			if (landTypes.HasFlag(LandTypes.desert))
				text += $"<color={goldenSand}>Desert</color>, ";
			if (landTypes.HasFlag(LandTypes.tundra))
				text += $"<color={icyBlue}>Tundra</color>, ";

			return RemoveLastComma(text);
		}

		public static string GetLandModifiersTextColour(LandModifiers landModifiers)
		{
			string text = "";

			if (landModifiers.HasFlag(LandModifiers.ruins))
				text += $"<color={cyan}>Ruins</color>, ";
			if (landModifiers.HasFlag(LandModifiers.town))
				text += $"<color={cyan}>Town</color>, ";
			if (landModifiers.HasFlag(LandModifiers.cursed))
				text += $"<color={cyan}>Cursed</color>, ";
			if (landModifiers.HasFlag(LandModifiers.volcanic))
				text += $"<color={cyan}>Volcanic</color>, ";
			if (landModifiers.HasFlag(LandModifiers.caves))
				text += $"<color={cyan}>Caves</color>, ";

			return RemoveLastComma(text);
		}

		public static string GetEnemyTypesTextColour(EnemyTypes enemyTypes)
		{
			string text = "";

			if (enemyTypes.HasFlag(EnemyTypes.slime))
				text += $"<color={lightGreen}>Slimes</color>, ";
			if (enemyTypes.HasFlag(EnemyTypes.beast))
				text += $"<color={earthyBrown}>Beasts</color>, ";
			if (enemyTypes.HasFlag(EnemyTypes.humanoid))
				text += $"Humanoids, ";
			if (enemyTypes.HasFlag(EnemyTypes.construct))
				text += $"<color={gunMetalGray}>Constructs</color>, ";
			if (enemyTypes.HasFlag(EnemyTypes.undead))
				text += $"<color={bloodlessGray}>Undead</color>, ";
			if (enemyTypes.HasFlag(EnemyTypes.Abberration))
				text += $"<color={eldritchPurple}>Abberrations</color>, ";

			return RemoveLastComma(text);
		}

		public static string AddColour(string text, string hexColour)
		{
			return $"<color={hexColour}>{text}</color>";
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
