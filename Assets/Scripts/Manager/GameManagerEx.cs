using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static Define;

[Serializable]
public class PlayerState
{
	public bool dialogueEvent = false;
	public bool goHomeEvent = false;
}

public enum CollectionState
{
	None,
	Uncheck,
	Done
}

[Serializable]
public class GameData
{
	public string Name;

	public CollectionState[] Collections = new CollectionState[MAX_COLLECTION_COUNT];

}

public class GameManagerEx
{
	GameData _gameData = new GameData();

	public GameData SaveData { get { return _gameData; } set { _gameData = value; } }

    #region 재화

    #endregion

    #region 시간

    #endregion

    public void Init()
	{
		//StartData data = Managers.Data.Start;

		if (File.Exists(_path))
		{
			string fileStr = File.ReadAllText(_path);
			_gameData.Collections = JsonUtility.FromJson<GameData>(fileStr).Collections;
		}
	}

    #region Save & Load	
    public string _path = Application.persistentDataPath + "/SaveData.json";

	public void SaveGame()
	{
		string jsonStr = JsonUtility.ToJson(Managers.Game.SaveData);
		File.WriteAllText(_path, jsonStr);
		Debug.Log($"Save Game Completed : {_path}");
	}

	public bool LoadGame()
	{
		if (File.Exists(_path) == false)
			return false;

		string fileStr = File.ReadAllText(_path);
		GameData data = JsonUtility.FromJson<GameData>(fileStr);
		if (data != null)
		{
			Managers.Game.SaveData = data;
		}

		Debug.Log($"Save Game Loaded : {_path}");
		return true;
	}
	#endregion
}
