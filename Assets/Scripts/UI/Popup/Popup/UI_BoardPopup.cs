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

    enum Texts
    {
        MoveText,
        ObjectText,
    }


    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));


        GetObject((int)GameObjects.BoardController).GetComponent<BoardController>().Init();

        RefreshUI();

        Managers.UI.SetCanvas(gameObject, true);
        return true;
    }

    public void RefreshUI()
    {
        GetText((int)Texts.MoveText).text = GetObject((int)GameObjects.BoardController).GetComponent<BoardController>().CanMove.ToString();
        GetText((int)Texts.ObjectText).text = GetObject((int)GameObjects.BoardController).GetComponent<BoardController>().Score.ToString();
        if(GetObject((int)GameObjects.BoardController).GetComponent<BoardController>().Score == 0)
        {
            Managers.UI.ClosePopupUI(this);
            Managers.UI.ShowPopupUI<UI_ClearPopup>();
        }
    }

}

