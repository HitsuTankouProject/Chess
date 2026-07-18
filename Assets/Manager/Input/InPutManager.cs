using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using static UnityEngine.GraphicsBuffer;


public enum GamepadType { None = 0, PlayStation, Xbox, Switch, }

public class InPutManager : MonoBehaviour
{
    /*NEW ONE*/
    private GameManager _gameManager => GameManager.Instance;
    private PlayerInPut _player01Input => _gameManager.player01.playerInPut;
    private PlayerInPut _player02Input => _gameManager.player02.playerInPut;

    public int chessLayerMask { get; private set; } = -1;
    public int gameBoardLayerMask { get; private set; } = -1;

    public Dictionary<Gamepad, GamepadType> recodingGamePads { get; private set; } = new();

    private HashSet<GamepadType> removedGamepad = new();
    public int oldConnectingGamePad = 0;

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

    #region GamePad Input
    private bool HasAnyButtonPressed(Gamepad gamepad)
    {
        if (gamepad == null) return false;
        return
               gamepad.buttonSouth.wasPressedThisFrame ||
               gamepad.buttonNorth.wasPressedThisFrame ||
               gamepad.buttonEast.wasPressedThisFrame ||
               gamepad.buttonWest.wasPressedThisFrame ||
               gamepad.leftShoulder.wasPressedThisFrame ||
               gamepad.rightShoulder.wasPressedThisFrame ||
               gamepad.leftTrigger.wasPressedThisFrame ||
               gamepad.rightTrigger.wasPressedThisFrame ||
               gamepad.startButton.wasPressedThisFrame ||
               gamepad.selectButton.wasPressedThisFrame ||
               gamepad.leftStickButton.wasPressedThisFrame ||
               gamepad.rightStickButton.wasPressedThisFrame;
    }
    public async UniTask<ButtonControl> WaitForGamePadButtonInput()
    {
        await UniTask.WaitUntil(() => !HasAnyButtonPressed(Gamepad.current));
        if (Gamepad.current == null) return null;
        var control = await InputSystem.onAnyButtonPress.First().ToUniTask();
        if (control is ButtonControl button && button.device is Gamepad)
            return button;
        else return null;

    }
    public async UniTask<ButtonControl> WaitForGamePadButtonInput(Gamepad gamepad)
    {
        await UniTask.WaitUntil(() => !HasAnyButtonPressed(gamepad));

        while (gamepad != null)
        {
            var control = await InputSystem.onAnyButtonPress.First().ToUniTask();

            if (control is ButtonControl button && button.device == gamepad)
            {
            return button;
            }
        }

        return null;
    }
    #endregion

    #region Gamepad Connect Watcher
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
    private Gamepad FindUnusedGamePad()
    {
        foreach (Gamepad gamePad in Gamepad.all)
        {
            if (!IsGamepadUsing(gamePad))
                return gamePad;
        }

        return null;
    }

    private bool IsGamepadUsing(Gamepad gamePad)
    {
        return _player01Input.nowUsingGamepad == gamePad
            || _player02Input.nowUsingGamepad == gamePad;
    }

    private void RecodeGamePad()
    {
        recodingGamePads.Clear();
        foreach (Gamepad gamepad in Gamepad.all)
        {
            GamepadType controllerType = GetControllerType(gamepad);
            recodingGamePads.Add(gamepad, controllerType);
        }
    }
    private void GamePadPairProcess()
    {
        oldConnectingGamePad = Gamepad.all.Count;

        bool isPlayer01UsingGamePad = _player01Input.nowUsingDevice == CanUseDevice.Gamepad;
        bool isPlayer01HaveGamePad = _player01Input.nowUsingGamepad != null 
                                    && Gamepad.all.Contains(_player01Input.nowUsingGamepad);

        bool isPlayer02UsingGamePad = _player02Input.nowUsingDevice == CanUseDevice.Gamepad;
        bool isPlayer02HaveGamePad = _player02Input.nowUsingGamepad != null
                                     && Gamepad.all.Contains(_player02Input.nowUsingGamepad);

        if (oldConnectingGamePad == 0)
        {
            if (!isPlayer01UsingGamePad && !isPlayer02UsingGamePad) return;
            _player01Input.ChangeToMouse();
            _player02Input.ChangeToMouse();
        }
        else if (oldConnectingGamePad == 1) 
        { 
            Gamepad connectedGamePad = Gamepad.all[0]; 
            bool onlyOnePlayerUsingGamepad = isPlayer01UsingGamePad ^ isPlayer02UsingGamePad; 
            if (onlyOnePlayerUsingGamepad) 
            { 
                if (IsGamepadUsing(connectedGamePad)) return; 
                if (isPlayer01UsingGamePad) _player01Input.ChangeToGamepad(connectedGamePad); 
                else _player02Input.ChangeToGamepad(connectedGamePad); return; 
            } 
            else
            { 
                if (!isPlayer01UsingGamePad && !isPlayer02UsingGamePad) 
                { 
                    _player01Input.ChangeToGamepad(connectedGamePad); 
                    _player02Input.ChangeToMouse(); 
                    return; 
                } 
                else 
                { 
                    if (!IsGamepadUsing(connectedGamePad)) 
                    { 
                        _player01Input.ChangeToGamepad(connectedGamePad); 
                        _player02Input.ChangeToMouse(); 
                        return; 
                    } 
                    if (isPlayer01HaveGamePad) _player02Input.ChangeToMouse(); 
                    else if (isPlayer02HaveGamePad) _player01Input.ChangeToMouse(); 
                    return; 
                } 
            } 
        }
        else if (oldConnectingGamePad >= 2)
        {
            if ((isPlayer01UsingGamePad && isPlayer01HaveGamePad) && (isPlayer02UsingGamePad && isPlayer02HaveGamePad)) return;
            else if (!isPlayer01UsingGamePad && !isPlayer02UsingGamePad)
            {
                _player01Input.ChangeToGamepad(Gamepad.all[0]);
                _player02Input.ChangeToGamepad(Gamepad.all[1]);
                return;
            }

            bool isPlayer01NeedToReConnect = !(isPlayer01UsingGamePad && isPlayer01HaveGamePad);
            bool isPlayer02NeedToReConnect = !(isPlayer02UsingGamePad && isPlayer02HaveGamePad);

            if (isPlayer01NeedToReConnect)
            {
                Gamepad connectedGamePad = FindUnusedGamePad();
                _player01Input.ChangeToGamepad(connectedGamePad);
            }
            if (isPlayer02NeedToReConnect)
            {
                Gamepad connectedGamePad = FindUnusedGamePad();
                _player02Input.ChangeToGamepad(connectedGamePad);
            }
        }

    }
    private async UniTask GamePadWatcher()
    {
        while (true)
        {
            await UniTask.WaitUntil(() => oldConnectingGamePad != Gamepad.all.Count);
            Debug.Log("oldConnectingGamePad != Gamepad.all.Count");
            RecodeGamePad();
            GamePadPairProcess();
            Debug.Log($"P1 Using:{_player01Input.nowUsingDevice.ToString()} Have:{_player01Input.nowUsingGamepad != null}");
            Debug.Log($"P2 Using:{_player02Input.nowUsingDevice.ToString()} Have:{_player02Input.nowUsingGamepad != null}");
        }
    }

    #endregion

    public void Init()
    {
        gameBoardLayerMask = LayerMask.GetMask("GameBoard");
        chessLayerMask = LayerMask.GetMask("Chess");

        GamePadWatcher().Forget();

    }


}
