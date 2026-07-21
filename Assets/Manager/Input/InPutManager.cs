using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using static UnityEngine.GraphicsBuffer;

/// <summary>接続されているゲームパッドの種類を表します。</summary>
public enum GamepadType
{
    /// <summary>種類を特定できない、またはゲームパッドが存在しない状態です。</summary>
    None = 0,
    /// <summary>PlayStation系コントローラーです。</summary>
    PlayStation,
    /// <summary>XboxまたはXInput系コントローラーです。</summary>
    Xbox,
    /// <summary>Nintendo Switch系コントローラーです。</summary>
    Switch, 
}

/// <summary>
/// 両プレイヤーの入力状態とゲームパッド接続を管理します。
/// 現在の手番に応じた入力ステージの切り替え、ボタン入力の非同期待機、
/// コントローラー種別の識別、接続台数に応じたプレイヤーへの自動割り当てを行います。
/// また、駒と盤面を検出するためのレイヤーマスクを初期化します。
/// </summary>
public class InPutManager : MonoBehaviour
{
    /// <summary>ゲーム全体を管理する共有インスタンスを取得します。</summary>
    private GameManager _gameManager => GameManager.Instance;
    /// <summary>白プレイヤーの入力管理オブジェクトを取得します。</summary>
    private PlayerInPut _player01Input => _gameManager.player01.playerInPut;
    /// <summary>黒プレイヤーの入力管理オブジェクトを取得します。</summary>
    private PlayerInPut _player02Input => _gameManager.player02.playerInPut;
    /// <summary>駒を検出するためのレイヤーマスクを取得します。</summary>
    public int chessLayerMask { get; private set; } = -1;
    /// <summary>ゲーム盤を検出するためのレイヤーマスクを取得します。</summary>
    public int gameBoardLayerMask { get; private set; } = -1;
    /// <summary>接続中のゲームパッドと識別された種類の対応表を取得します。</summary>
    public Dictionary<Gamepad, GamepadType> recodingGamePads { get; private set; } = new();
    /// <summary>接続解除されたゲームパッドの種類を保持します。</summary>
    private HashSet<GamepadType> removedGamepad = new();
    /// <summary>前回確認時に接続されていたゲームパッド数です。</summary>
    public int oldConnectingGamePad = 0;
    /// <summary>指定プレイヤーだけを対象の入力ステージへ切り替えます。</summary>
    /// <param name="player">入力を有効化するプレイヤーの駒色です。</param>
    /// <param name="stage">対象プレイヤーへ設定する入力ステージです。</param>
    public void PlayerInputStage(ChessColor player, InputStage stage)
    {
        // 一方のプレイヤーを有効化するとき、もう一方の入力を無効化します。
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

    /// <summary>指定ゲームパッドでいずれかの主要ボタンがこのフレームに押されたか判定します。</summary>
    /// <param name="gamepad">入力状態を確認するゲームパッドです。</param>
    /// <returns>対象ボタンのいずれかが押された場合は <see langword="true" /> です。</returns>
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
    /// <summary>現在のゲームパッドで次に押されるボタンを非同期で待機します。</summary>
    /// <returns>押されたゲームパッドのボタンです。取得できない場合は <see langword="null" /> です。</returns>
    public async UniTask<ButtonControl> WaitForGamePadButtonInput()
    {
        // 前回の押下状態が解除されるまで待ち、同じ入力の連続検出を防ぎます。
        await UniTask.WaitUntil(() => !HasAnyButtonPressed(Gamepad.current));
        if (Gamepad.current == null) return null;
        var control = await InputSystem.onAnyButtonPress.First().ToUniTask();
        if (control is ButtonControl button && button.device is Gamepad)
            return button;
        else return null;

    }
    /// <summary>指定ゲームパッドで次に押されるボタンを非同期で待機します。</summary>
    /// <param name="gamepad">入力を待機するゲームパッドです。</param>
    /// <returns>指定ゲームパッドで押されたボタンです。切断された場合は <see langword="null" /> です。</returns>
    public async UniTask<ButtonControl> WaitForGamePadButtonInput(Gamepad gamepad)
    {
        await UniTask.WaitUntil(() => !HasAnyButtonPressed(gamepad));

        // 他デバイスの入力は無視し、指定ゲームパッドの入力だけを返します。
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

    /// <summary>デバイス情報からゲームパッドの種類を判定します。</summary>
    /// <param name="gamepad">種類を判定するゲームパッドです。</param>
    /// <returns>識別されたゲームパッドの種類です。</returns>
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
    /// <summary>どちらのプレイヤーにも割り当てられていないゲームパッドを検索します。</summary>
    /// <returns>未使用のゲームパッドです。存在しない場合は <see langword="null" /> です。</returns>
    private Gamepad FindUnusedGamePad()
    {
        foreach (Gamepad gamePad in Gamepad.all)
        {
            if (!IsGamepadUsing(gamePad))
                return gamePad;
        }

        return null;
    }
    /// <summary>指定ゲームパッドがいずれかのプレイヤーに割り当てられているか判定します。</summary>
    /// <param name="gamePad">使用状態を確認するゲームパッドです。</param>
    /// <returns>割り当て済みの場合は <see langword="true" /> です。</returns>
    private bool IsGamepadUsing(Gamepad gamePad)
    {
        return _player01Input.nowUsingGamepad == gamePad
            || _player02Input.nowUsingGamepad == gamePad;
    }
    /// <summary>接続中の全ゲームパッドを再取得し、コントローラー種類を記録します。</summary>
    private void RecodeGamePad()
    {
        recodingGamePads.Clear();
        foreach (Gamepad gamepad in Gamepad.all)
        {
            GamepadType controllerType = GetControllerType(gamepad);
            recodingGamePads.Add(gamepad, controllerType);
        }
    }
    /// <summary>接続台数と現在の割り当て状態に基づいて両プレイヤーの入力デバイスを調整します。</summary>
    private void GamePadPairProcess()
    {
        oldConnectingGamePad = Gamepad.all.Count;

        bool isPlayer01UsingGamePad = _player01Input.nowUsingDevice == CanUseDevice.Gamepad;
        bool isPlayer01HaveGamePad = _player01Input.nowUsingGamepad != null 
                                    && Gamepad.all.Contains(_player01Input.nowUsingGamepad);

        bool isPlayer02UsingGamePad = _player02Input.nowUsingDevice == CanUseDevice.Gamepad;
        bool isPlayer02HaveGamePad = _player02Input.nowUsingGamepad != null
                                     && Gamepad.all.Contains(_player02Input.nowUsingGamepad);

        // ゲームパッドが0台の場合は、使用中のプレイヤーをマウス操作へ戻します。
        if (oldConnectingGamePad == 0)
        {
            if (!isPlayer01UsingGamePad && !isPlayer02UsingGamePad) return;
            _player01Input.ChangeToMouse();
            _player02Input.ChangeToMouse();
        }
        // 1台の場合は重複割り当てを避け、一方だけをゲームパッド操作にします。
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
        // 2台以上の場合は、両プレイヤーへ未使用のゲームパッドを1台ずつ割り当てます。
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

    /// <summary>
    /// ゲームパッドの接続台数を継続監視し、変化時に記録と割り当てを更新します。
    /// </summary>
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
