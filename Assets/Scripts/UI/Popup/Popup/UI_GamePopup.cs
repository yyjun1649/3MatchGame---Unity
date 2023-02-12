using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;


public class UI_GamePopup : UI_Popup
{
    enum GameObjects
    { 

    }


    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.UI.SetCanvas(gameObject, true);
        return true;
    }

    public void RefreshUI()
    {

    }

}

