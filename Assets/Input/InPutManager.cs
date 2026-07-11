using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;


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
            recodingGamePads[gamepad] = controllerType;
        }
    }



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
    private void GamepadDistribute()
    {
        List<Gamepad> gamepads = Gamepad.all.ToList();

        if (_player01Input.nowUsingGamepad != null &&
            !gamepads.Contains(_player01Input.nowUsingGamepad))
        {
            _player01Input.ChangeToMouse();
        }

        if (_player02Input.nowUsingGamepad != null &&
            !gamepads.Contains(_player02Input.nowUsingGamepad))
        {
            _player02Input.ChangeToMouse();
        }

        if (gamepads.Count == 0)
        {
            _player01Input.ChangeToMouse();
            _player02Input.ChangeToMouse();
            return;
        }

        HashSet<Gamepad> usingGamepads = new();

        if (_player01Input.nowUsingGamepad != null)
            usingGamepads.Add(_player01Input.nowUsingGamepad);

        if (_player02Input.nowUsingGamepad != null)
            usingGamepads.Add(_player02Input.nowUsingGamepad);

        foreach (Gamepad gamepad in gamepads)
        {
            if (usingGamepads.Contains(gamepad))
                continue;

            if (_player01Input.nowUsingGamepad == null)
            {
                _player01Input.SetUseGamepadType(gamepad);
                usingGamepads.Add(gamepad);
                continue;
            }

            if (_player02Input.nowUsingGamepad == null)
            {
                _player02Input.SetUseGamepadType(gamepad);
                usingGamepads.Add(gamepad);
            }
        }

        if (_player01Input.nowUsingGamepad == null)
            _player01Input.ChangeToMouse();

        if (_player02Input.nowUsingGamepad == null)
            _player02Input.ChangeToMouse();
    }

    private void CurrentGamepadWatcher()
    {
        if (oldConnectingGamePad == Gamepad.all.Count) return;

        Debug.Log(Gamepad.all.Count);
        oldConnectingGamePad = Gamepad.all.Count;
        RecodeGamePad();
        GamepadDistribute();

    }

    private bool HasAnyButtonPressed(Gamepad gamepad)
    {

        foreach (var control in gamepad.allControls)
        {
            if (control is ButtonControl button && button.isPressed)
                return true;
        }
        return false;
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









    public void Init()
    {
        gameBoardLayerMask = LayerMask.GetMask("GameBoard");
        chessLayerMask = LayerMask.GetMask("Chess");


        oldConnectingGamePad = Gamepad.all.Count;
        RecodeGamePad();

        int gamepadCount = Gamepad.all.Count;
        if (gamepadCount == 1)
        {
            _player01Input.SetUseGamepadType(Gamepad.all[0]);
        }
        else if (gamepadCount >= 2)
        {
            _player01Input.SetUseGamepadType(Gamepad.all[0]);
            _player02Input.SetUseGamepadType(Gamepad.all[1]);
        }
    }


    private void Update()
    {
        CurrentGamepadWatcher();
    }






}
