using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;


public class UI_StartPopup : UI_Popup
{
    enum Buttons
    {
        StartButton,
    }

    enum Texts
    {
        StartText,
    }
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButton(typeof(Buttons));
        BindText(typeof(Texts));

        GetButton((int)Buttons.StartButton).gameObject.BindEvent(OnClickStartButton);

        Managers.UI.SetCanvas(gameObject, true);
        return true;
    }

    void OnClickStartButton()
    {
        Managers.UI.ClosePopupUI(this);
        Managers.UI.ShowPopupUI<UI_BoardPopup>();
    }

}

