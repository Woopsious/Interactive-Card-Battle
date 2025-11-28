using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static Woopsious.DamageData;
using static Woopsious.EntityData;
using static Woopsious.MapNodeData;

namespace Woopsious
{
	public static class RichTextManager
	{   
		//encounter type colours
		private static readonly Dictionary<NodeEncounterType, string> encounterTypeColours = new()
		{
			{ NodeEncounterType.basicFight, "#006400" },		//defaultColor
			{ NodeEncounterType.eliteFight, "#CDE2EA" },		//eldritchPurple
			{ NodeEncounterType.bossFight, "#C81919" },			//red
			{ NodeEncounterType.eliteBossFight, "#CDE2EA" },	//eldritchPurple
			{ NodeEncounterType.freeCardUpgrade, "#00FFFF" }	//cyan
		};

		//land type colours
		private static readonly Dictionary<LandTypes, string> landTypeColours = new()
		{
			{ LandTypes.grassland, "#00FF00" }, //green
			{ LandTypes.hills, "#7C9A61" },		//mutedGreen
			{ LandTypes.forest, "#006400" },	//darkGreen
			{ LandTypes.mountains, "#B0B5B3" },	//softGrey
			{ LandTypes.desert, "#DCC38C" },	//goldenSand
			{ LandTypes.tundra, "#CDE2EA" }		//icyBlue
		};

		//land modifier colours (only 1 colour used)
		private const string cyan = "#00FFFF";

		//enemy type colours
		private static readonly Dictionary<EnemyTypes, string> enemyTypeColours = new()
		{
			{ EnemyTypes.slime, "#00FF00" },		//lightGreen
			{ EnemyTypes.beast, "#7C9A61" },		//earthyBrown
			{ EnemyTypes.humanoid, "#006400" },		//defaultColor
			{ EnemyTypes.construct, "#B0B5B3" },	//gunMetalGray
			{ EnemyTypes.undead, "#DCC38C" },		//bloodlessGray
			{ EnemyTypes.Abberration, "#CDE2EA" }	//eldritchPurple
		};

		//value type colours
		private static readonly Dictionary<ValueTypes, string> valueTypeColours = new()
		{
			{ ValueTypes.damages, "#8B0000" },		//crimsonRed
			{ ValueTypes.blocks, "#4682B4" },		//steelBlue
			{ ValueTypes.heals, "#006400" }		//darkGreen
		};

		//global colours
		public enum GlobalColours
		{
			white, black, blue, skyBlue
		}
		private static readonly Dictionary<GlobalColours, string> globalColours = new()
		{
			{ GlobalColours.white, "#ffffff" },
			{ GlobalColours.black, "#000000" },
			{ GlobalColours.blue, "#0000ff" },
			{ GlobalColours.skyBlue, "#0096FF" },
		};

		//applying colours to text
		public static string GetEncounterTypeTextColour(NodeEncounterType encounterType)
		{
			string text = "";

			foreach (var kvp in encounterTypeColours)
			{
				if (encounterType.HasFlag(kvp.Key))
				{
					// Capitalize first letter of land type for display
					string displayName = char.ToUpper(kvp.Key.ToString()[0]) + kvp.Key.ToString()[1..];
					text += $"<color={kvp.Value}>{displayName}</color>,";
				}
			}

			return text;
		}

		public static string GetLandTypesTextColour(LandTypes landTypes)
		{
			string text = "";

			foreach (var kvp in landTypeColours)
			{
				if (landTypes.HasFlag(kvp.Key))
				{
					// Capitalize first letter of land type for display
					string displayName = char.ToUpper(kvp.Key.ToString()[0]) + kvp.Key.ToString()[1..];
					text += $"<color={kvp.Value}>{displayName}</color>,";
				}
			}

			return RemoveLastComma(text);
		}

		public static string GetLandModifiersTextColour(LandModifiers landModifiers)
		{
			string text = "";

			if (landModifiers.HasFlag(LandModifiers.ruins))
				text += $"<color={cyan}>Ruins</color>,";
			if (landModifiers.HasFlag(LandModifiers.town))
				text += $"<color={cyan}>Town</color>,";
			if (landModifiers.HasFlag(LandModifiers.cursed))
				text += $"<color={cyan}>Cursed</color>,";
			if (landModifiers.HasFlag(LandModifiers.volcanic))
				text += $"<color={cyan}>Volcanic</color>,";
			if (landModifiers.HasFlag(LandModifiers.caves))
				text += $"<color={cyan}>Caves</color>,";

			return RemoveLastComma(text);
		}

		public static string GetEnemyTypesTextColour(EnemyTypes enemyTypes)
		{
			string text = "";

			foreach (var kvp in enemyTypeColours)
			{
				if (enemyTypes.HasFlag(kvp.Key))
				{
					// Capitalize first letter of land type for display
					string displayName = char.ToUpper(kvp.Key.ToString()[0]) + kvp.Key.ToString()[1..];
					text += $"<color={kvp.Value}>{displayName}</color>,";
				}
			}

			return RemoveLastComma(text);
		}

		public static string AddValueTypeColour(string text, ValueTypes valueType)
		{
			foreach (var kvp in valueTypeColours)
			{
				if (valueType.HasFlag(kvp.Key))
					text = $"<color={kvp.Value}>{text}</color>,";
			}
			return RemoveLastComma(text);
		}

		public static string AddColour(string text, GlobalColours colour)
		{
			if (globalColours.TryGetValue(colour, out var hex))
				return $"<color={hex}>{text}</color>";
			return text;
		}

		//adding linked text
		public static string AddTextLink(string text, GlobalColours colour)
		{
			if (globalColours.TryGetValue(colour, out var hex))
				return $"<link=TextLink><color={hex}>{text}</color></link>";
			return text;
		}

		//removing commas + trimming text
		public static string RemoveLastComma(string input)
		{
			int lastCommaIndex = input.LastIndexOf(',');

			if (lastCommaIndex >= 0)
				return input.Remove(lastCommaIndex, 1);

			return input.Trim();
		}
	}
}
