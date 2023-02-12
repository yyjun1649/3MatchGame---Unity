using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;


public class UI_BoardPopup : UI_Popup
{
    enum GameObjects
    {
        BoardController,
    }


    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));

        GetObject((int)GameObjects.BoardController).GetComponent<BoardController>().Init();

        Managers.UI.SetCanvas(gameObject, true);
        return true;
    }

    public void RefreshUI()
    {

    }

}

