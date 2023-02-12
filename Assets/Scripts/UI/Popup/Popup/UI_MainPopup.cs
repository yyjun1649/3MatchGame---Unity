using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class UI_MainPopup : UI_Popup
{
    enum Buttons
    {
        GameStartButton,
    }


    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        Managers.Sound.Play(Sound.Bgm, "Sound_MainBGM", 0.3f);
        BindButton(typeof(Buttons));

        GetButton((int)Buttons.GameStartButton).gameObject.BindEvent(OnClickStartButton);


        Managers.UI.FindPopup<UI_MainPopup>().RefreshUI();
        Managers.UI.SetCanvas(gameObject, true);
        return true;
    }

    public void RefreshUI()
    {

    }

    void OnClickStartButton()
    {
        Managers.Sound.Play(Sound.Effect, "Sound_Hammer", 0.5f);
    }

}
