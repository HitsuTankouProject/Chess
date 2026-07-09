using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControllerChoosePanel : MonoBehaviour
{
    private GameManager _gameManager => GameManager.Instance;
    private ResourcesData _resourcesData => _gameManager.resourcesData;
    private InPutManager _inPutManager => _gameManager.inPutManager;

    public Image[] canUseControllers;
    [Header("White Player 01")]
    public ControllerChoose player01Choose;
    [Header("Black Player 02")]
    public ControllerChoose player02Choose;

    public readonly Color c_pick = Color.white;
    public readonly Color c_nonActive = new Color(0.5f, 0.5f, 0.5f);

    public Sprite sp_Ready;
    public Sprite sp_NonReady;

    private List<GamepadType> canUseGameTypeList = new();

    private void UpdateCanUseControllers()
    {
        //canUseGameTypeList.Clear();

        //for (int i = 0; i < canUseControllers.Length; i++)
        //{
        //    int enumValue = i + 1;

        //    if (!Enum.IsDefined(typeof(GamepadType), enumValue))
        //    {
        //        Debug.LogError($"Invalid GamepadType value: {enumValue}");
        //        continue;
        //    }

        //    GamepadType gamepadType = (GamepadType)enumValue;

        //    bool haveGamePad =
        //        _inPutManager.recodingGamePads.ContainsKey(gamepadType);

        //    canUseControllers[i].color =
        //        haveGamePad ? c_pick : c_nonActive;

        //    if (haveGamePad)
        //    {
        //        canUseGameTypeList.Add(gamepadType);
        //    }
        //}

        //player01Choose.UpdateCanUseControllers(canUseGameTypeList);
        //player02Choose.UpdateCanUseControllers(canUseGameTypeList);
    }

    public void Init()
    {
        UpdateCanUseControllers();
    }

    public void AllReady()
    {
        //if (player01Choose.isReady && player02Choose.isReady)
        //{
        //    _gameManager.EndControllerChoose(player01Choose.choseGamepad, player02Choose.choseGamepad);
        //}
            
    }

    public bool CanChose(ControllerChoose self, GamepadType gamepadType)
    {
        ControllerChoose others = self == player01Choose ? player02Choose : player01Choose;
        if (!others.isReady) return true;

        return gamepadType != others.choseGamepad;
    }

    private void Update()
    {
        if(_inPutManager.recodingGamePads.Count!= canUseGameTypeList.Count)
        {
            UpdateCanUseControllers();
        }
        

    }

}
