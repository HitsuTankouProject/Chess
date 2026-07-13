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
        if (gamepad == null) return null;
        var control = await InputSystem.onAnyButtonPress.First().ToUniTask();
        if (control is ButtonControl button && button.device is Gamepad)
            return button;
        else return null;

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
    private Gamepad FindUnusedGamePad(Gamepad usedGamePad)
    {
        foreach (Gamepad gamePad in Gamepad.all)
            if (gamePad != usedGamePad) return gamePad;
        return null;
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
    private void GamePadPairProcess()
    {
        oldConnectingGamePad = Gamepad.all.Count;

        bool isPlayer01UsingGamePad = _player01Input.nowUsingDevice == CanUseDevice.Gamepad;
        bool isPlayer01HaveGamePad = _player01Input.nowUsingGamepad != null 
                                    &&Gamepad.all.Contains(_player01Input.nowUsingGamepad);

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

            // Player 1 ›ßãS³íŽg—p”‡ŒÂŽè”cC•sŽù—v™|—
            if (isPlayer01UsingGamePad && isPlayer01HaveGamePad)
            {
                if (isPlayer02UsingGamePad) _player02Input.ChangeToMouse();
                return;
            }

            // Player 2 ›ßãS³íŽg—p”‡ŒÂŽè”cC•sŽù—v™|—
            else if (isPlayer02UsingGamePad && isPlayer02HaveGamePad)
            {
                if (isPlayer01UsingGamePad)
                    _player01Input.ChangeToMouse();
                return;
            }

            // —LlÝ’èˆ× GamepadC’AŒ´–{“IŽè”c›ßÐü
            if (isPlayer01UsingGamePad && !isPlayer01HaveGamePad)
            {
                _player01Input.ChangeToGamepad(connectedGamePad);
                _player02Input.ChangeToMouse();
                return;
            }
            else if (isPlayer02UsingGamePad && !isPlayer02HaveGamePad)
            {
                _player02Input.ChangeToGamepad(connectedGamePad);
                _player01Input.ChangeToMouse();

                return;
            }

            // ™_l“sŸ“—LŽg—pŽè”cC—aÝ•ª”z‹‹ Player 1
            _player01Input.ChangeToGamepad(connectedGamePad);
            _player02Input.ChangeToMouse();
            return;
        }
        else if(oldConnectingGamePad >= 2)
        {
            // Player 1 “IŽè”c–³ÁCQˆêŒÂŸ“—L”í Player 2 Žg—p“IŽè”c
            if (!isPlayer01HaveGamePad)
            {
                Gamepad player01GamePad = FindUnusedGamePad(
                    isPlayer02UsingGamePad? _player02Input.nowUsingGamepad: null );

                if (player01GamePad != null) _player01Input.ChangeToGamepad(player01GamePad);
            }

            // dVŽæ“¾ Player 1 “IŽè”cC”ð–Æ™_l•ª”z“ž“¯ˆêŒÂ
            Gamepad player01CurrentGamePad = _player01Input.nowUsingGamepad;

            // Player 2 “IŽè”c–³Áˆ½äo Player 1 ‘Š“¯
            if (!isPlayer02HaveGamePad ||_player02Input.nowUsingGamepad == player01CurrentGamePad)
            {
                Gamepad player02GamePad = FindUnusedGamePad(player01CurrentGamePad);

                if (player02GamePad != null) _player02Input.ChangeToGamepad(player02GamePad);
            }
        }

    }
    private async UniTask GamePadWatcher()
    {
        while (true)
        {
            await UniTask.WaitUntil(() => oldConnectingGamePad != Gamepad.all.Count);
            RecodeGamePad();
            GamePadPairProcess();
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
