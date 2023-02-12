using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameScene : BaseScene
{
	protected override bool Init()
	{
		if (base.Init() == false)
			return false;

		SceneType = Define.Scene.Game;
		Managers.UI.ShowPopupUI<UI_BoardPopup>();
		Debug.Log("Init");
		return true;
	}
}
