using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;


public class GamepadData
{
    public string name { get; private set; }
    public string displayName { get; private set; }
    public string descriptionProduct { get; private set; }

    public GamepadData()
    {
        name = string.Empty;
        displayName = string.Empty;
        descriptionProduct = string.Empty;
    }

    public GamepadData(Gamepad newGamepad)
    {
        if (newGamepad == null)
        {
            name = string.Empty;
            displayName = string.Empty;
            descriptionProduct = string.Empty;
            return;
        }

        name = newGamepad.name ?? string.Empty;
        displayName = newGamepad.displayName ?? string.Empty;
        descriptionProduct = newGamepad.description.product ?? string.Empty;
    }

    public override bool Equals(object obj)
    {
        if (obj is not GamepadData other)
        {
            return false;
        }

        return name == other.name
            && displayName == other.displayName
            && descriptionProduct == other.descriptionProduct;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            name,
            displayName,
            descriptionProduct);
    }

    public static bool operator ==(GamepadData a, GamepadData b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

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

    private PlayerInPut Player01Input =>
        InGame.Instance?.whiteChessPlayer?.playerInPut;

    private PlayerInPut Player02Input =>
        InGame.Instance?.blackChessPlayer?.playerInPut;

    private Gamepad Player01Gamepad =>
        Player01Input?.nowUsingGamepad;

    private Gamepad Player02Gamepad =>
        Player02Input?.nowUsingGamepad;

    private Coroutine watchControllerConnecting;

    public int nowUsingGamePad = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private bool IsPlayerReady()
    {
        return Player01Input != null
            && Player02Input != null;
    }

    #region Gamepad

    private void AssignUnusedGamepads()
    {
        if (!IsPlayerReady())
        {
            return;
        }

        foreach (Gamepad gamepad in Gamepad.all)
        {
            bool isUsed =
                Player01Input.nowUsingGamepad == gamepad ||
                Player02Input.nowUsingGamepad == gamepad;

            if (isUsed)
            {
                continue;
            }

            bool player01UsingMouse =
                Player01Input.nowUsingDevice == CanUseDevice.Mouse;

            bool player02UsingMouse =
                Player02Input.nowUsingDevice == CanUseDevice.Mouse;

            if (player01UsingMouse)
            {
                Player01Input.ChangeToGamepad(gamepad);
                continue;
            }

            if (player02UsingMouse)
            {
                Player02Input.ChangeToGamepad(gamepad);
                continue;
            }

            return;
        }
    }

    private void ValidatePlayerGamepad(
        PlayerInPut input,
        Dictionary<GamepadData, Gamepad> gamepadDatas)
    {
        if (input == null)
        {
            return;
        }

        if (input.nowUsingDevice == CanUseDevice.Gamepad)
        {
            if (input.nowUsingGamepad != null &&
                Gamepad.all.Contains(input.nowUsingGamepad))
            {
                return;
            }
        }

        if (input.nowUsingDevice != CanUseDevice.Mouse)
        {
            return;
        }

        if (input.lastConnectingGamepadData == null)
        {
            return;
        }

        if (gamepadDatas.TryGetValue(
            input.lastConnectingGamepadData,
            out Gamepad targetGamepad))
        {
            input.ChangeToGamepad(targetGamepad);
        }
    }

    private void UpdateTheGamePadAndPlayerInPut()
    {
        if (!IsPlayerReady())
        {
            return;
        }

        if (Player01Gamepad != null &&
            Player01Gamepad == Player02Gamepad)
        {
            Debug.LogError(
                "Same Gamepad assigned to different players");

            return;
        }

        if (Gamepad.all.Count == 0)
        {
            Player01Input.ChangeToMouse();
            Player02Input.ChangeToMouse();
            return;
        }

        Dictionary<GamepadData, Gamepad> gamepadDatas =
            new Dictionary<GamepadData, Gamepad>();

        foreach (Gamepad gamepad in Gamepad.all)
        {
            if (gamepad == null)
            {
                continue;
            }

            GamepadData data =
                new GamepadData(gamepad);

            gamepadDatas[data] = gamepad;
        }

        ValidatePlayerGamepad(
            Player01Input,
            gamepadDatas);

        ValidatePlayerGamepad(
            Player02Input,
            gamepadDatas);

        AssignUnusedGamepads();
    }

    private IEnumerator WatchControllerConnecting()
    {
        while (true)
        {
            if (nowUsingGamePad != Gamepad.all.Count)
            {
                UpdateTheGamePadAndPlayerInPut();
                nowUsingGamePad = Gamepad.all.Count;
            }

            yield return null;
        }
    }

    #endregion

    public void InPutManager_Init()
    {
        if (watchControllerConnecting != null)
        {
            StopCoroutine(watchControllerConnecting);
        }

        watchControllerConnecting =
            StartCoroutine(WatchControllerConnecting());

        nowUsingGamePad = Gamepad.all.Count;

        UpdateTheGamePadAndPlayerInPut();
    }
}
