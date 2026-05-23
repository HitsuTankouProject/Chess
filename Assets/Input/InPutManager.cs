using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;


public class GamepadData
{
    public string name { get; private set; } = null;
    public string displayName { get; private set; } = null;
    public string descriptionProduct { get; private set; } = null;

    public GamepadData() { }
    public GamepadData(Gamepad newGamepad)
    {
        if (newGamepad == null) return;

        name = newGamepad.name;
        displayName = newGamepad.displayName;
        descriptionProduct = newGamepad.description.product;
    }

    public override bool Equals(object obj)
    {
        if (obj is not GamepadData other)
            return false;

        return name == other.name
            && displayName == other.displayName
            && descriptionProduct == other.descriptionProduct;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(name, displayName, descriptionProduct);
    }
    public static bool operator ==(GamepadData a, GamepadData b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }
    public static bool operator !=(GamepadData a, GamepadData b)
    {
        return !(a == b);
    }
}

public class InPutManager : MonoBehaviour
{
    public static InPutManager Instance { get; private set; }
    private PlayerInPut _player01Input => InGame.Instance.whiteChessPlayer.playerInPut;
    private Gamepad _player01GamePad => _player01Input.nowUsingGamepad;
    private PlayerInPut _player02Input => InGame.Instance.blackChessPlayer.playerInPut;
    private Gamepad _player02GamePad => _player02Input.nowUsingGamepad;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    #region GamePad
    public int nowUsingGamePad = 0;
    private Coroutine watchControllerConnecting;

    private void AssignUnusedGamepads()
    {
        bool isPlayer01UsingMouse;
        bool isPlayer02UsingMouse;
        bool isUsed;

        foreach(Gamepad gamepad in Gamepad.all)
        {
            isUsed = _player01Input.nowUsingGamepad == gamepad || _player02Input.nowUsingGamepad == gamepad;
            if (isUsed) continue;
            isPlayer01UsingMouse = _player01Input.nowUsingDevice == CanUseDevice.Mouse;
            isPlayer02UsingMouse = _player02Input.nowUsingDevice == CanUseDevice.Mouse;

            if (isPlayer01UsingMouse)
            {
                _player01Input.ChangeToGamepad(gamepad);
                continue;
            }
            else if (isPlayer02UsingMouse)
            {
                _player02Input.ChangeToGamepad(gamepad);
                continue;
            }

            if (!isPlayer01UsingMouse && !isPlayer02UsingMouse) return;

        }


    }

    private void ValidatePlayerGamepad(PlayerInPut input, Dictionary<GamepadData, Gamepad> gamepadDatas)
    {
        if (input.nowUsingDevice == CanUseDevice.Gamepad && Gamepad.all.Contains(input.nowUsingGamepad)) return;
        if (input.nowUsingDevice == CanUseDevice.Mouse)
        {
            if (gamepadDatas.ContainsKey(input.lastConnectingGamepadData))
            {
                input.ChangeToGamepad(gamepadDatas[input.lastConnectingGamepadData]);
                return;
            }
        }
    }

    private void UpdateTheGamePadAndPlayerInPut()
    {
        if (_player01GamePad == _player02GamePad && _player01GamePad != null)
        {
            Debug.LogError("Same Gamepad for Different Player");
            return;
        }
        if (Gamepad.all.Count == 0)
        {
            _player01Input.ChangeToMouse();
            _player02Input.ChangeToMouse();
            return;
        }

        Dictionary<GamepadData, Gamepad> gamepadDatas = new Dictionary<GamepadData, Gamepad>();
        foreach (Gamepad gamepad in Gamepad.all) gamepadDatas[new GamepadData(gamepad)] = gamepad;


        ValidatePlayerGamepad(_player01Input, gamepadDatas);
        ValidatePlayerGamepad(_player02Input, gamepadDatas);

        AssignUnusedGamepads();

    }


    private IEnumerator WatchControllerConnecting()
    {
        bool haveChange;

        while (true)
        {
            haveChange = nowUsingGamePad != Gamepad.all.Count;
            if (!haveChange)
            {
                yield return null; continue;
            }

            UpdateTheGamePadAndPlayerInPut();
            nowUsingGamePad = Gamepad.all.Count;
            yield return null;
        }

    }
    #endregion

    public void InPutManager_Init()
    {
        if(watchControllerConnecting!=null)StopCoroutine(watchControllerConnecting);
        watchControllerConnecting = null;

        watchControllerConnecting = StartCoroutine(WatchControllerConnecting());
    }


}
