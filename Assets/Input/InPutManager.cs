using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private Coroutine watchControllerConnecting;

    public int nowUsingGamePadIndex = 0;

    private Dictionary <PlayerInPut, GamepadData> 
        theLastConnectGamepadData = new Dictionary<PlayerInPut, GamepadData>();

    public int gameBoardLayerMask { get; private set; } = -1;
    public int chessLayerMask { get; private set; } = -1;
    public int buttonLayerMask { get; private set; } = -1;
    //public int cardLayerMask { get; private set; } = -1;

    public Dictionary<InGameStage, int> hitLayerMasks;



    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private bool IsPlayerReady() => Player01Input != null && Player02Input != null;

    #region Gamepad

    private bool IsPlayerInputUsingGamepad(PlayerInPut playerInPut)
    {
        return playerInPut.nowUsingDevice == CanUseDevice.Gamepad &&
        playerInPut.nowUsingGamepad != null &&
        playerInPut.nowUsingGamepad.added;
    }

    private void AssignUnusedGamepads(Dictionary<GamepadData, Gamepad> gamepadDatas)
    {
        if (!IsPlayerReady()) return;

        foreach (PlayerInPut playerInput in theLastConnectGamepadData.Keys)
        {
            foreach (GamepadData gamepadData in gamepadDatas.Keys)
            {
                if (theLastConnectGamepadData[playerInput] != gamepadData)
                    continue;

                Gamepad targetGamepad = gamepadDatas[gamepadData];

                if (targetGamepad == null || !targetGamepad.added) continue;
                playerInput.ChangeToGamepad(targetGamepad);
            }
        }

        bool player01UsingGamepad = IsPlayerInputUsingGamepad(Player01Input);
        bool player02UsingGamepad = IsPlayerInputUsingGamepad(Player02Input);
        if (player01UsingGamepad && player02UsingGamepad) return;

        foreach (Gamepad gamepad in Gamepad.all)
        {
            if (gamepad == null || !gamepad.added) continue;
            bool alreadyUse = 
                Player01Input.nowUsingGamepad == gamepad || Player02Input.nowUsingGamepad == gamepad;
            if (alreadyUse) continue;

            if (!player01UsingGamepad)
            {
                Player01Input.ChangeToGamepad(gamepad);
                player01UsingGamepad = true;
                theLastConnectGamepadData[Player01Input] = new GamepadData(gamepad);
            }
            else if (!player02UsingGamepad)
            {
                Player02Input.ChangeToGamepad(gamepad);
                player02UsingGamepad = true;
                theLastConnectGamepadData[Player02Input] = new GamepadData(gamepad);

            }

            if (player01UsingGamepad && player02UsingGamepad) break;
        }

    }

    private void ValidatePlayerGamepad()
    {
        if (!IsPlayerReady()) return;

        List<PlayerInPut> players =
        new List<PlayerInPut>(theLastConnectGamepadData.Keys);

        foreach (PlayerInPut playerInput in players)
        {
            if (playerInput.nowUsingDevice == CanUseDevice.Mouse)
                continue;

            if (playerInput.nowUsingGamepad == null)
                continue;

            if (!playerInput.nowUsingGamepad.added)
                continue;

            theLastConnectGamepadData[playerInput] = new GamepadData(playerInput.nowUsingGamepad);
        }
    }

    private void UpdateTheGamePadAndPlayerInPut()
    {
        if (!IsPlayerReady()) return;

        if (Player01Input.nowUsingGamepad != null &&
            Player01Input.nowUsingGamepad.added &&
            Player01Input.nowUsingGamepad ==
            Player02Input.nowUsingGamepad)
        {
            Debug.LogError("Same Gamepad assigned to different players");
        }

        Player01Input.ChangeToMouse();
        Player02Input.ChangeToMouse();

        if (Gamepad.all.Count == 0) return;

        Dictionary<GamepadData, Gamepad> nowConnectGamepadData =
        new Dictionary<GamepadData, Gamepad>();

        foreach (Gamepad gamepad in Gamepad.all)
        {
            if (gamepad == null || !gamepad.added) continue;
            nowConnectGamepadData[new GamepadData(gamepad)] = gamepad;
        }

        foreach (GamepadData gamepadData in nowConnectGamepadData.Keys)
        {
            Debug.Log($"{gamepadData.name} + {nowConnectGamepadData[gamepadData].name}");
        }


        AssignUnusedGamepads(nowConnectGamepadData);
        ValidatePlayerGamepad();

        foreach (var pair in theLastConnectGamepadData)
        {
            Debug.Log(
                $"{pair.Key.name} -> {pair.Value?.displayName}");
        }

    }

    private IEnumerator WatchControllerConnecting()
    {
        while (true)
        {
            if (nowUsingGamePadIndex != Gamepad.all.Count)
            {
                UpdateTheGamePadAndPlayerInPut();
                nowUsingGamePadIndex = Gamepad.all.Count;
            }

            yield return null;
        }
    }

    #endregion

    public void LayerMask_Init()
    {
        gameBoardLayerMask = LayerMask.GetMask("GameBoard");
        chessLayerMask = LayerMask.GetMask("Chess");
        buttonLayerMask = LayerMask.GetMask("Button");
        //cardLayerMask = LayerMask.GetMask("Card");

        hitLayerMasks = new Dictionary<InGameStage, int>()
        {
            {   InGameStage.Init, -1 },
            {   InGameStage.ChooseSkill, buttonLayerMask },
            {   InGameStage.TurnStart, gameBoardLayerMask | chessLayerMask | buttonLayerMask },
            {   InGameStage.TurnChanging, -1 },
            {   InGameStage.GameSet,buttonLayerMask},
        };

    }

    public int CanHitLayerMask() => hitLayerMasks.ContainsKey(InGame.Instance.inGameStage) ? hitLayerMasks[InGame.Instance.inGameStage] : -1;

    public void InPutManager_Init()
    {
        LayerMask_Init();

        if (watchControllerConnecting != null)
        {
            StopCoroutine(watchControllerConnecting);
        }
        watchControllerConnecting =
            StartCoroutine(WatchControllerConnecting());

        nowUsingGamePadIndex = Gamepad.all.Count;

        UpdateTheGamePadAndPlayerInPut();
    }




}
