using System;
using System.Collections.Generic;
using UnityEngine;

namespace Woopsious
{
	public class SaveManager : MonoBehaviour
	{
		public static SaveManager instance;

		public static Action ReloadSaveSlotsUi;
		public static Action<PlayerData> ReloadPlayerData;
		public static Action ReloadGameData;

		public PlayerData PlayerData = new();
		public SlotData SlotData = new();
		public GameData GameData = new();

		void Awake()
		{
			instance = this;
		}

		//SAVING PLAYER DATA
		public static void SavePlayerData()
		{
			string directory = Application.persistentDataPath + "/PlayerData";
			string filePath = Application.persistentDataPath + "/PlayerData/data.json";

			if (!instance.DoesDirectoryExist(directory))
				System.IO.Directory.CreateDirectory(directory);

			instance.DeletePlayerData();

			PlayerData playerData = new()
			{
				audioVolume = AudioManager.AudioVolume,
			};

			string data = JsonUtility.ToJson(playerData);
			System.IO.File.WriteAllText(filePath, data);
			Debug.LogError("SAVE PLAYER DATA");
		}
		public static void LoadPlayerData()
		{
			string directory = Application.persistentDataPath + "/PlayerData";
			string filePath = Application.persistentDataPath + "/PlayerData/data.json";

			if (!instance.DoesDirectoryExist(directory)) return;
			if (!instance.DoesFileExist(directory, "/data.json")) return;

			string data = System.IO.File.ReadAllText(filePath);
			PlayerData playerData = JsonUtility.FromJson<PlayerData>(data);

			ReloadPlayerData?.Invoke(playerData);
			Debug.LogError("LOAD PLAYER DATA");
		}
		void DeletePlayerData()
		{
			string directory = Application.persistentDataPath + "/PlayerData";

			if (!DoesDirectoryExist(directory)) return;
			if (!DoesFileExist(directory, "/data.json")) return;

			System.IO.File.Delete(directory + "/data.json");
		}

		//SAVING GAME DATA
		public void SaveGameData(string directory)
		{
			if (DoesDirectoryExist(directory))
			{
				if (DoesFileExist(directory, "/GameData.json"))
					System.IO.File.Delete(directory + "/GameData.json");
			}
			else
				System.IO.Directory.CreateDirectory(directory);

			SaveGameDataToJson(directory, "/GameData.json");
			ReloadSaveSlotsUi?.Invoke();
		}
		public void LoadGameData(string directory)
		{
			if (!DoesDirectoryExist(directory)) return;
			if (!DoesFileExist(directory, "/GameData.json")) return;

			LoadGameDataToJson(directory, "/GameData.json");
			ReloadGameData?.Invoke();
		}
		public void DeleteGameData(string directory)
		{
			if (!DoesDirectoryExist(directory)) return;
			if (!DoesFileExist(directory, "/GameData.json")) return;

			DeleteGameDataJsonFile(directory, "/GameData.json");
			ReloadSaveSlotsUi?.Invoke();
		}

		//saving/loading/deleting game data json file
		void SaveGameDataToJson(string directory, string fileName)
		{
			string filePath = directory + fileName;
			string inventoryData = JsonUtility.ToJson(GameData);
			System.IO.File.WriteAllText(filePath, inventoryData);

			string slotPath = directory + "/SlotData.json";
			string slotData = JsonUtility.ToJson(SlotData);
			System.IO.File.WriteAllText(slotPath, slotData);
		}
		void LoadGameDataToJson(string directory, string fileName)
		{
			string filePath = directory + fileName;
			string inventoryData = System.IO.File.ReadAllText(filePath);
			GameData = JsonUtility.FromJson<GameData>(inventoryData);
		}
		void DeleteGameDataJsonFile(string directory, string fileName)
		{
			System.IO.File.Delete(directory + "/SlotData.json");
			System.IO.File.Delete(directory + fileName);
			System.IO.Directory.Delete(directory);
		}

		//bool checks
		bool DoesDirectoryExist(string path)
		{
			if (System.IO.Directory.Exists(path))
				return true;
			else
				return false;
		}
		bool DoesFileExist(string path, string file)
		{
			if (System.IO.File.Exists(path + file))
				return true;
			else
				return false;
		}
	}

	//PLAYER DATA
	public class PlayerData
	{
		public string keybindsData;

		public float audioVolume;
	}

	//GAME DATA
	public class SlotData
	{
		public string playerClass;
		public string date;
	}
	public class GameData
	{

	}
}