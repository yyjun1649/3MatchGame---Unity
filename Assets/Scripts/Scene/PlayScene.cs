using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayScene : BaseScene
{
	protected override bool Init()
	{
		if (base.Init() == false)
			return false;

		SceneType = Define.Scene.Game;
		Managers.UI.ShowPopupUI<UI_GamePopup>();
		Debug.Log("Init");
		return true;
	}
}