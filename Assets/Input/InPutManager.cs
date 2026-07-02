using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GamepadType { None = 0, PlayStation, Xbox, Switch, }

public class InPutManager : MonoBehaviour
{

    private bool IsPlayerReady() => _player01Input != null && _player02Input != null;


    /*NEW ONE*/
    private GameManager _gameManager => GameManager.Instance;
    private PlayerInPut _player01Input => _gameManager.player01.playerInPut;
    private PlayerInPut _player02Input => _gameManager.player02.playerInPut;

    private int gameBoardAndChessLayerMask = -1;
    public int CanHitLayerMask() => gameBoardAndChessLayerMask;

    public Dictionary<GamepadType, Gamepad> recodingGamePads { get; private set; } = new();
    public Gamepad GetGamepad(GamepadType gamepadType)
    {
        if(!recodingGamePads.TryGetValue(gamepadType,out Gamepad gamepad)) return null;
        else return gamepad;
    }

    private HashSet<GamepadType> removedGamepad = new();
    public int oldConnectingGamePad = 0;

    private GamepadType GetControllerType(Gamepad gamepad)
    {
        if (gamepad == null) return GamepadType.None;

        string name = gamepad.name?.ToLower() ?? "";
        string interfaceName = gamepad.description.interfaceName?.ToLower() ?? "";
        string manufacturer = gamepad.description.manufacturer?.ToLower() ?? "";
        string product = gamepad.description.product?.ToLower() ?? "";

        if (name.Contains("dual") || manufacturer.Contains("sony") || product.Contains("wireless controller"))
            return GamepadType.PlayStation;

        if (name.Contains("xinput") || interfaceName.Contains("xinput"))
            return GamepadType.Xbox;

        if (name.Contains("switch") || manufacturer.Contains("nintendo") || product.Contains("pro controller"))
            return GamepadType.Switch;

        return GamepadType.None;
    }

    private void RecodeGamePad()
    {
        recodingGamePads.Clear();
        foreach (Gamepad gamepad in Gamepad.all)
        {
            GamepadType controllerType = GetControllerType(gamepad);
            if (controllerType == GamepadType.None|| recodingGamePads.ContainsKey(controllerType)) continue;
            recodingGamePads[controllerType] = gamepad;
        }
    }

    private void CurrentGamepadWatcher()
    {
        if (oldConnectingGamePad == Gamepad.all.Count) return;
        Debug.Log(Gamepad.all.Count);
        oldConnectingGamePad = Gamepad.all.Count;
        RecodeGamePad();
    }

    #region Choose InPut

    public void ChooseInput(PlayerInPut playerInput, GamepadType choseInput)
    {
        playerInput.SetUseGamepadType(choseInput);
        playerInput.StartInput();
    }


    #endregion
    public void PlayerInputStage(ChessColor player, InputStage stage)
    {
        if (player == ChessColor.White)
        {
            _player01Input.inputStage = stage;

            _player02Input.inputStage = InputStage.None;
        }
        else if (player == ChessColor.Black)
        {
            _player01Input.inputStage = InputStage.None;
            _player02Input.inputStage = stage;
        }

    }

    public void Init()
    {
        gameBoardAndChessLayerMask = LayerMask.GetMask("GameBoard") | LayerMask.GetMask("Chess");
        oldConnectingGamePad = Gamepad.all.Count;
        RecodeGamePad();

    }

    private void Update()
    {
        CurrentGamepadWatcher();
    }






}
