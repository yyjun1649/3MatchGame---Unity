using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;


public class UI_StartPopup : UI_Popup
{
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

    void OnClickStartButton()
    {
        Managers.UI.ClosePopupUI(this);
        Managers.UI.ShowPopupUI<UI_MainPopup>();
    }

}

