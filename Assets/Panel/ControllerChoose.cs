using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerChoose : MonoBehaviour
{
    private InPutManager _inPutManager => GameManager.Instance.inPutManager;
    public ControllerChoosePanel controllerChoosePanel;

    public bool isReady/* { get; private set; } */= false;
    public Button readyButton;
    public List<Button> controllerChooseButtons;
    public GamepadType choseGamepad {  get; private set; } = GamepadType.None;

    private void ButtonActive(Button button, bool isActive)
    {
        button.image.color = isActive ? controllerChoosePanel.c_pick : controllerChoosePanel.c_nonActive;
        button.enabled = isActive;
    }

    private void ChoseTheController(GamepadType gamepadType)
    {
        if (isReady) return;
        if (!controllerChoosePanel.CanChose(this, gamepadType)) return;
        choseGamepad = gamepadType;
        for (int i = 1; i < controllerChooseButtons.Count; i++)
        {
            controllerChooseButtons[i].image.color = controllerChoosePanel.c_nonActive;
        }
        Debug.Log(gamepadType.ToString());
        Debug.Log((int)gamepadType);

        controllerChooseButtons[(int)gamepadType].image.color = controllerChoosePanel.c_pick;
    }


    public void Button_ChooseMouse() => ChoseTheController(GamepadType.None);
    public void Button_ChooseSwitchController() => ChoseTheController(GamepadType.Switch);
    public void Button_ChooseXBoxController() => ChoseTheController(GamepadType.Xbox);
    public void Button_ChoosePsController() => ChoseTheController(GamepadType.PlayStation);

    public void Button_Ready()
    {
        isReady = !isReady;
        readyButton.image.color = isReady ? controllerChoosePanel.c_pick : controllerChoosePanel.c_nonActive;
        readyButton.image.sprite = isReady ? controllerChoosePanel.sp_Ready : controllerChoosePanel.sp_NonReady;
         controllerChoosePanel.AllReady();
    }

    public void UpdateCanUseControllers(List<GamepadType> gamepadTypeList)
    {
        for (int i = 1; i < controllerChooseButtons.Count; i++)
          ButtonActive(controllerChooseButtons[i], false);

        foreach (GamepadType gamepadType in gamepadTypeList)
        {
            int index = (int)gamepadType;
            ButtonActive(controllerChooseButtons[index], true);
        }

    }

}
