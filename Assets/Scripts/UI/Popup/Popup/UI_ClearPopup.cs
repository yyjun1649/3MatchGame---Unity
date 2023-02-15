using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;


public class UI_ClearPopup : UI_Popup
{

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.Sound.Stop(Define.Sound.Bgm);
        Managers.Sound.Play(Define.Sound.Effect, "Sound_Clear");
        Managers.UI.SetCanvas(gameObject, true);
        return true;
    }

}

