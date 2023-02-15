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
		Managers.UI.ShowPopupUI<UI_StartPopup>();
        Managers.Sound.Play(Define.Sound.Bgm, "Sound_BackGround", 1, 1);
		return true;
	}
}
