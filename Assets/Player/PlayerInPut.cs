using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>プレイヤーが使用できる入力デバイスを表します。</summary>
public enum CanUseDevice 
{
    /// <summary>マウスによる画面上のクリック操作を使用します。</summary>
    Mouse,
    /// <summary>割り当てられたゲームパッドによるカーソル操作を使用します。</summary>
    Gamepad
};
/// <summary>プレイヤー入力が現在受け付ける操作段階を表します。</summary>
public enum InputStage 
{
    /// <summary>盤面上の駒操作を受け付けない非手番状態です。</summary>
    None,
    /// <summary>候補カードからバフスキルを選択している状態です。</summary>
    ChooseSkill,
    /// <summary>自分の駒を選択する入力を待っている状態です。</summary>
    Waiting,
    /// <summary>選択済みの駒を移動させる盤面座標を選んでいる状態です。</summary>
    Picking,
    /// <summary>バフ効果によって同じ駒をもう一度移動させる状態です。</summary>
    OneMoreMove
}
/// <summary>
/// 1人分のマウス・ゲームパッド入力を管理します。
/// 駒の選択と配置、追加行動、スキル選択、ポーズ画面、盤面カーソル移動を処理し、
/// 入力デバイスの切り替え時には実行中の非同期入力ループを安全に再開始します。
/// </summary>
public class PlayerInPut : MonoBehaviour
{
    /// <summary>
    /// ゲーム全体の進行状態と各管理オブジェクトへの共有アクセスポイントを取得します。
    /// 現在のゲームステージ、手番、プレイヤー、スキル選択画面の参照に使用します。
    /// </summary>
    private GameManager _gameManager => GameManager.Instance;
    /// <summary>
    /// 盤面上の駒配置とマス表示を管理するオブジェクトを取得します。
    /// 駒検索、移動候補表示、選択マーク更新、候補表示の解除に使用します。
    /// </summary>
    private ChessBoard _chessBoard => _gameManager.chessBoard;
    /// <summary>
    /// 両プレイヤーの入力ステージとゲームパッド接続を管理するオブジェクトを取得します。
    /// Raycast用レイヤーマスクと、ゲームパッドのボタン入力待機に使用します。
    /// </summary>
    private InPutManager _inPutManager => _gameManager.inPutManager;
    /// <summary>
    /// BGMと効果音を管理するオブジェクトを取得します。
    /// 駒を選択したときと盤面へ配置したときの効果音再生に使用します。
    /// </summary>
    private AudioManager _audioManager => _gameManager.audioManager;
    /// <summary>
    /// ゲームで共有する駒・盤面・音声リソースを取得します。
    /// 入力操作に対応する駒選択音と駒配置音のデータ取得に使用します。
    /// </summary>
    private ResourcesData _resourcesData => _gameManager.resourcesData;
    /// <summary>
    /// マウスのRaycastで盤面マスだけを検出するためのレイヤーマスクです。
    /// <see cref="Init" /> で "GameBoard" レイヤーから取得します。
    /// </summary>
    private int gameBoardLayerMask;
    /// <summary>
    /// マウス座標を盤面上のRayへ変換する、このプレイヤー専用のカメラです。
    /// プレイヤーCanvasに設定されたカメラを初期化時に参照します。
    /// </summary>
    private Camera _camera;
    /// <summary>
    /// この入力コンポーネントを所有するプレイヤーです。
    /// 駒色、所有駒、ポーズ状態、カメラ切り替え、UI操作の参照に使用します。
    /// </summary>
    private Player _player;
    /// <summary>
    /// 現在受け付ける入力操作の段階です。
    /// 非手番、スキル選択、駒選択待ち、移動先選択、追加行動を切り替えます。
    /// </summary>
    public InputStage inputStage/* { get; private set; } */= InputStage.None;
    /// <summary>
    /// 入力コンポーネントの所有者、使用カメラ、盤面レイヤーを設定します。
    /// 初期化直後は入力段階を <see cref="InputStage.None" /> にして、
    /// ゲーム進行側が明示的に入力を開始するまで駒操作を受け付けません。
    /// </summary>
    /// <param name="player">この入力コンポーネントを使用するプレイヤーです。</param>
    public void Init(Player player)
    {
        // 所有プレイヤーを保存し、入力を受け付けない初期状態へ戻します。
        _player = player;
        inputStage = InputStage.None;
        // プレイヤー側の画面に対応したカメラをマウスRaycastへ使用します。
        _camera = player.playerCanvas.playerCamera;
        // 盤面マスだけを検出するレイヤーマスクを取得します。
        gameBoardLayerMask = LayerMask.GetMask("GameBoard");
    }

    #region Using Device

    /// <summary>
    /// このプレイヤーが現在使用している入力デバイスの種類です。
    /// 初期状態ではマウスを使用し、ゲームパッドの接続・割り当てに応じて変更されます。
    /// </summary>
    public CanUseDevice nowUsingDevice/* { get; private set; } */= CanUseDevice.Mouse;
    /// <summary>
    /// このプレイヤーへ現在割り当てられているゲームパッドです。
    /// マウス操作中、またはゲームパッドが切断された場合は <see langword="null" /> です。
    /// </summary>
    public Gamepad nowUsingGamepad;
    /// <summary>Input Systemで現在有効なマウスを取得します。</summary>
    public Mouse nowUsingMouse => Mouse.current;
    /// <summary>
    /// 指定ゲームパッドをこのプレイヤーへ割り当て、ゲームパッド入力を開始します。
    /// 引数が <see langword="null" /> の場合は、安全のためマウス操作へ切り替えます。
    /// </summary>
    /// <param name="gamepad">このプレイヤーへ割り当てるゲームパッドです。</param>
    public void ChangeToGamepad(Gamepad gamepad = null)
    {
        if (gamepad == null)
        {
            ChangeToMouse();
            return;
        }

        nowUsingDevice = CanUseDevice.Gamepad;
        nowUsingGamepad = gamepad;
        //Debug.Log(nowUsingGamepad.name);
        ChangeInput(CanUseDevice.Gamepad);
        _player.playerCanvas.controllerMarkPrinter.ChangeMark(_inPutManager.GetControllerType(nowUsingGamepad));


    }
    /// <summary>
    /// ゲームパッドの割り当てを解除し、現在のマウスによる入力へ切り替えます。
    /// 切り替え後は実行中の入力ループを停止してマウス入力ループを開始します。
    /// </summary>
    public void ChangeToMouse()
    {
        nowUsingGamepad = null;
        _player.playerCanvas.controllerMarkPrinter.ChangeMark(_inPutManager.GetControllerType(null));

        nowUsingDevice = CanUseDevice.Mouse;
        ChangeInput(CanUseDevice.Mouse);
    }
    /// <summary>指定されたデバイス種類に対応する入力ループを開始します。</summary>
    /// <param name="usingDevice">新しく使用する入力デバイスの種類です。</param>
    private void ChangeInput(CanUseDevice usingDevice)
    {

        switch (usingDevice)
        {
            case CanUseDevice.Mouse:StartMouseInput();break;
            case CanUseDevice.Gamepad:StartGamepadInput();break;
        }
    }

    #endregion

    /// <summary>対局中の自分の駒を選択できる待機状態へ切り替えます。</summary>
    public void StartGame() => inputStage = InputStage.Waiting;
    /// <summary>バフカードを選択する入力状態へ切り替えます。</summary>
    public void StartChoose() => inputStage = InputStage.ChooseSkill;

    /// <summary>
    /// 現在選択中で、移動先の入力を待っている駒です。
    /// Inspectorでデバッグ確認でき、選択解除または移動開始時にnullへ戻します。
    /// </summary>
    private ChessBasic pickIngChess;
    /// <summary>有効な盤面座標を取得できなかったことを示す値です。</summary>
    private readonly Vector2Int invalidBoardPos = new(-1, -1);
    /// <summary>
    /// 指定座標に存在する現在手番の駒を選択します。
    /// 選択音とカメラ切り替えを実行し、移動・捕獲候補を計算して
    /// 駒を選択表示へ移動した後、入力段階を移動先選択へ進めます。
    /// </summary>
    /// <param name="boardPos">選択する駒の盤面座標です。</param>
    private void PickChess(Vector2Int boardPos)
    {
        bool haveChess = _chessBoard.board.TryGetValue(boardPos, out ChessBasic chess);
        if (!haveChess) return;
        if (chess.color != _gameManager.nowTurn) return;
        //Debug.Log(chess.name);
        _audioManager.PlaySfx(_resourcesData.sfx_PickChess);
        _player.TurnCamera(PlayerCameraStage.Pick);

        pickIngChess = chess;
        pickIngChess.FindPossibleMove();
        pickIngChess.GotPick();
        inputStage = InputStage.Picking;
    }
    /// <summary>
    /// 選択中の駒が指定座標へ移動できるか判定し、可能な場合は移動を実行します。
    /// 候補表示、カメラ、駒の選択表示を先に解除し、移動できない場合は
    /// 駒選択待ちへ戻します。移動できる場合は駒固有の移動処理へ引き渡します。
    /// </summary>
    /// <param name="boardPos">配置先として選択された盤面座標です。</param>
    /// <returns>指定座標へ移動できた場合は <see langword="true" /> です。</returns>
    private bool PutChess(Vector2Int boardPos)
    {
        if (pickIngChess == null) return false;
        _audioManager.PlaySfx(_resourcesData.sfx_PutChess);

        bool canMove = pickIngChess.possibleMoveList.Contains(boardPos);
        _chessBoard.ReSetActive();
        _player.TurnCamera(PlayerCameraStage.Normal);
        pickIngChess.ReturnPick();

        if (!canMove)
        {
            pickIngChess = null;
            inputStage = InputStage.Waiting;
        }
        else
        {
            ChessBasic moveChess = pickIngChess;
            pickIngChess = null;
            moveChess.Move(boardPos);
        }


        return canMove;
    }
    /// <summary>
    /// バフ効果による指定駒の追加行動を開始します。
    /// カメラを選択視点へ切り替え、移動候補を再計算し、現在位置へ
    /// ゲームパッドカーソルを合わせて追加の移動先入力を待機します。
    /// </summary>
    /// <param name="oneMoreMoveChess">もう一度移動させる対象の駒です。</param>
    public void StartOneMoreMove(ChessBasic oneMoreMoveChess)
    {
        inputStage = InputStage.OneMoreMove;
        _player.TurnCamera(PlayerCameraStage.Pick);
        pickIngChess.FindPossibleMove();
        pickIngChess.GotPick();
        nowPosPick = pickIngChess.position;
        _chessBoard.UpdatePlayerChose(nowPosPick);
        Debug.Log(pickIngChess.name);
    }

    #region Mouse
    /// <summary>
    /// 駒選択待ちでは駒レイヤー、移動先選択中では盤面レイヤーを取得します。
    /// </summary>
    private int CanHitLayerMask()
    {
        if (inputStage == InputStage.Waiting) return _inPutManager.chessLayerMask;
        else if (inputStage == InputStage.Picking) return _inPutManager.gameBoardLayerMask;
        else return -1;
    }
    /// <summary>
    /// ポーズ中でない左クリックを検出し、プレイヤーカメラからRaycastを実行します。
    /// 現在の入力段階に対応するレイヤー上で最初に命中したオブジェクトを返します。
    /// </summary>
    private bool IsPressed(out GameObject hitObject)
    {
        hitObject = null;
        if (!nowUsingMouse.leftButton.wasPressedThisFrame || _player.isPause)
        {
            return false;
        }
        Vector2 mousePos = nowUsingMouse.position.ReadValue();
        Ray rayResult = _camera.ScreenPointToRay(mousePos);


        bool isHit = Physics.Raycast(rayResult, out RaycastHit hit, 1000f, CanHitLayerMask(), QueryTriggerInteraction.Collide);
        if (!isHit)
        {
            hitObject = null;
            return false;
        }

        hitObject = hit.collider.gameObject;
        return true;
    }
    /// <summary>
    /// Raycast対象の駒または盤面マスから座標を取得し、対象外なら無効座標を返します。
    /// </summary>
    private Vector2Int ChessBoardPosition(GameObject hitObject) 
    {
        if(hitObject.TryGetComponent<ChessBasic>(out ChessBasic chess))
            return chess.position;
        if (hitObject.TryGetComponent<ChessBlock>(out ChessBlock chessBlock)) 
            return chessBlock.position;
        return invalidBoardPos;
    }
    /// <summary>
    /// クリックした座標へ選択マークを移動し、その位置の自分の駒を選択します。
    /// </summary>
    private void Press_Chess(Vector2Int boardPos)
    {
        if (boardPos == invalidBoardPos) return;
        _chessBoard.UpdatePlayerChose(boardPos);
        PickChess(boardPos);
    }
    /// <summary>
    /// クリックした盤面座標へ選択中の駒を配置し、通常視点へ戻します。
    /// </summary>
    private void Press_ChessBoard(Vector2Int boardPos)
    {
        if (!PutChess(boardPos)) return;

        if (inputStage != InputStage.OneMoreMove)
        {
            _player.TurnCamera(PlayerCameraStage.Normal);
            inputStage = InputStage.None;
        }
    }
    /// <summary>
    /// バフによる追加行動で、クリックした有効座標へ選択中の駒を移動します。
    /// </summary>
    private void Mouse_OneMoreMove(Vector2Int boardPos)
    {
        bool canMove = pickIngChess.possibleMoveList.Contains(boardPos);
        if (!canMove) return;
        _chessBoard.ReSetActive();
        _chessBoard.ReSetActive();
        _player.TurnCamera(PlayerCameraStage.Normal);
        pickIngChess.ReturnPick();
        ChessBasic moveChess = pickIngChess;
        pickIngChess = null;
        moveChess.Move(boardPos);
        inputStage = InputStage.None;
    }
    /// <summary>
    /// クリック対象を盤面座標へ変換し、現在の入力段階に対応する操作へ振り分けます。
    /// </summary>
    private void PressAction()
    {
        bool isPressed = IsPressed(out GameObject hitObject);
        if (!isPressed) return;
        int hitLayer = hitObject.layer;

        if (inputStage == InputStage.Waiting)
        {
            Press_Chess(ChessBoardPosition(hitObject));
            return;
        }
        else if (inputStage == InputStage.Picking)
        {
            Press_ChessBoard(ChessBoardPosition(hitObject));
            return;

        }
        else if (inputStage == InputStage.OneMoreMove)
        {
            Mouse_OneMoreMove(ChessBoardPosition(hitObject));
 
        }


    }
    /// <summary>
    /// キャンセルされるまで毎フレーム待機し、対局中だけクリック操作を処理します。
    /// </summary>
    private async UniTask MouseInPut(CancellationToken token)
    {
        while (true)
        {
            await UniTask.Yield(token);
            if (_gameManager.nowGameStage != GameStage.InGame) continue;
            PressAction();
        }

    }
    /// <summary>
    /// 既存の入力ループを停止し、新しいキャンセルトークンでマウス監視を開始します。
    /// </summary>
    private void StartMouseInput()
    {
        if (inputUpdate != null) RejectInput();
        inputUpdate = new CancellationTokenSource();
        MouseInPut(inputUpdate.Token).Forget();

    }


    #endregion

    #region Gamepad

    /// <summary>
    /// 現在のゲームステージに対応するゲームパッド入力ループを開始します。
    /// </summary>
    public void StartGamepadInput()
    {
        if (inputUpdate != null) RejectInput();
        inputUpdate = new CancellationTokenSource();

        switch (_gameManager.nowGameStage)
        {
            case GameStage.SkillChoose:     WaitGamePadInput_GameSkillChoose(inputUpdate.Token).Forget();       break;
            case GameStage.InGame:          InGameProcess(inputUpdate.Token).Forget();                          break;
        }

    }

    #region Choose Skill

    /// <summary>スキルカード選択画面を管理するパネルを取得します。</summary>
    private ChooseSkillPanel _chooseSkillPanel => _gameManager.chooseSkillPanel;
    /// <summary>このプレイヤーが現在スキルを選択する順番か判定します。</summary>
    private bool IsNowPickTurn()
    {
        return inputStage == InputStage.ChooseSkill && _gameManager.chooseSkillPanel.chooseSkillPlayerColor == _player.usingChess;
    }
    /// <summary>選択対象を次のスキルカードへ移動します。</summary>
    private void PickNextCard() => _chooseSkillPanel.PickNextCard();
    /// <summary>選択対象を前のスキルカードへ移動します。</summary>
    private void PickBackCard() => _chooseSkillPanel.PickBackCard();
    /// <summary>カード選択中は詳細画面を閉じ、それ以外ではカードを引き直します。</summary>
    private void Return()
    {
        if (_chooseSkillPanel.isPicking) _chooseSkillPanel.Button_Return(); 
        else _chooseSkillPanel.Button_DrawAgain();
    }
    /// <summary>選択中のカードを確定し、選択完了時は入力監視を終了します。</summary>
    private void ConFirm()
    {
        if (_chooseSkillPanel.isPicking)
        {
            _chooseSkillPanel.Button_ConFirm();
            RejectInput();

        }
        else _chooseSkillPanel.PickThatCard();
    }
    /// <summary>スキル選択中のゲームパッド入力を待機し、候補移動・決定・復帰を処理します。</summary>
    private async UniTask WaitGamePadInput_GameSkillChoose(CancellationToken token)
    {

        while (IsNowPickTurn())
        {
            await UniTask.Yield(token);
            ButtonControl button = await _inPutManager.WaitForGamePadButtonInput(nowUsingGamepad);
            if (button == null) continue;

            switch (button.name)
            {
                case "rightShoulder":   PickNextCard();         break;
                case "right":           PickNextCard();         break;

                case "leftShoulder":    PickBackCard();         break;
                case "left":            PickBackCard();         break;

                case "buttonEast":      Return();               break;
                case "buttonSouth":     ConFirm();              break;

                default:                await UniTask.Yield();  break;
            }
        }
    }

    #endregion

    #region InGame

    /// <summary>プレイヤー画面に属するスキル説明パネルを取得します。</summary>
    private SkillDescriptionPanel skillDescriptionPanel => _player.playerCanvas.skillDescriptionPanel;
    /// <summary>スキル説明パネルが表示中かどうかを取得します。</summary>
    private bool isCardDescriptionOpen => skillDescriptionPanel.gameObject.activeSelf;
    /// <summary>プレイヤーがポーズ中かどうかを取得します。</summary>
    private bool isPause => _player.isPause;
    /// <summary>ゲームパッドで現在選択している盤面座標です。</summary>
    public Vector2Int nowPosPick = Vector2Int.zero;
    /// <summary>ポーズボタンの処理をプレイヤー画面へ通知します。</summary>
    private void Pause() => _player.playerCanvas.Button_Pause();
    /// <summary>入力段階に応じてゲームパッドカーソルが移動できる座標を取得します。</summary>
    private IEnumerable<Vector2Int> GetAreas()
    {
        if (inputStage == InputStage.Waiting)
        {
            if (_player != null) return _player.allTheChess.Keys;
        }
        else if (inputStage == InputStage.Picking || inputStage == InputStage.OneMoreMove)
        {
            if (pickIngChess != null) return pickIngChess.possibleMoveList;
        }
        Debug.LogError("it should go to here in GetAreas");
        return Enumerable.Empty<Vector2Int>();
    }
    /// <summary>入力方向に存在する候補から、横ずれが小さく最も近い座標へカーソルを移動します。</summary>
    private void TryMoveCursor(Vector2Int inputDirection)
    {
        if (inputDirection == Vector2Int.zero)
            return;

        Vector2Int bestPos = nowPosPick;
        int bestDistance = int.MaxValue;

        foreach (Vector2Int targetPos in GetAreas())
        {
            if (targetPos == nowPosPick)
                continue;

            Vector2Int offset = targetPos - nowPosPick;

            if (inputDirection.x > 0 && offset.x <= 0) continue;
            if (inputDirection.x < 0 && offset.x >= 0) continue;
            if (inputDirection.y > 0 && offset.y <= 0) continue;
            if (inputDirection.y < 0 && offset.y >= 0) continue;

            int side = inputDirection.x != 0
                ? Mathf.Abs(offset.y)
                : Mathf.Abs(offset.x);

            int forward = inputDirection.x != 0
                ? Mathf.Abs(offset.x)
                : Mathf.Abs(offset.y);

            int score = side * 1000 + forward;

            if (score < bestDistance)
            {
                bestDistance = score;
                bestPos = targetPos;
            }
        }

        if (bestPos == nowPosPick)
            return;

        nowPosPick = bestPos;
        _chessBoard.UpdatePlayerChose(nowPosPick);
    }
    /// <summary>プレイヤーの駒色に合わせて方向を補正し、盤面カーソルを移動します。</summary>
    private void ReturnToBoardPosition(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return;
        int offset = _player.usingChess == ChessColor.White ? 1 : -1;
        TryMoveCursor(dir * offset);
    }
    /// <summary>ポーズ中は前のカードを表示し、対局中はカーソルを左へ移動します。</summary>
    private void Dpad_Left()
    {
        if (isPause)
            _player.playerCanvas.BackCard();
        else
            if (IsPlayerTurn()) ReturnToBoardPosition(Vector2Int.left);
    }
    /// <summary>ポーズ中は次のカードを表示し、対局中はカーソルを右へ移動します。</summary>
    private void Dpad_Right()
    {
        if (isPause)
            _player.playerCanvas.NextCard();
        else
            if (IsPlayerTurn()) ReturnToBoardPosition(Vector2Int.right);
    }
    /// <summary>説明画面を閉じるか、選択中の駒をキャンセルします。</summary>
    private void ButtonEast()
    {
        if (isPause)
        {
            if (_player.playerCanvas.isConfirming)
            {
                _player.playerCanvas.Button_Return();
            }
            else if(!isCardDescriptionOpen)_player.playerCanvas.WatchBuffSkillDescription();
        }
        else
        {
            if(inputStage == InputStage.Picking)
            {
                _chessBoard.ReSetActive();
                _player.TurnCamera(PlayerCameraStage.Normal);
                pickIngChess.ReturnPick();
                pickIngChess = null;
                inputStage = InputStage.Waiting;
            }
        }
    }
    /// <summary>スキル説明を開くか、駒の選択・移動を決定します。</summary>
    private void ButtonSouth()
    {
        if (isPause)
        {
            if (_player.playerCanvas.isConfirming)
            {
                _player.playerCanvas.Button_Confirm();
            }
            else if(!isCardDescriptionOpen)_player.playerCanvas.WatchBuffSkillDescription();
        }
        else
        {
            if (inputStage == InputStage.Waiting)
                PickChess(nowPosPick);
            else if (inputStage == InputStage.Picking)
            {
                if (!PutChess(nowPosPick)) return;
                _player.TurnCamera(PlayerCameraStage.Normal);
                inputStage = InputStage.None;
            }
        }
    }
    /// <summary>ポーズ中にゲームタイトルへ戻る確認を表示します。</summary>
    private void ButtonWest()
    {
        if (isPause) _player.playerCanvas.Button_BackToGameTitle();
    }
    /// <summary>ポーズ中に投了確認を表示します。</summary>
    private void ButtonNorth()
    {
        if (isPause) _player.playerCanvas.Button_Surrender();
    }




    #region Non Player Turn

    /// <summary>非手番中のポーズ画面操作をゲームパッドから受け付けます。</summary>
    private async UniTask NonPlayerTurn(CancellationToken token)
    {
        while(inputStage == InputStage.None)
        {
            await UniTask.Yield(token);
            ButtonControl button = await _inPutManager.WaitForGamePadButtonInput(nowUsingGamepad);
            if (button == null) continue;

            switch (button.name)
            {
                case "start":           Pause();                break;

                case "right":           Dpad_Right();           break;
                case "left":            Dpad_Left();            break;

                case "buttonEast":      ButtonEast();           break;
                case "buttonSouth":     ButtonSouth();          break;

                case "buttonWest":      ButtonWest();           break;

                case "buttonNorth":     ButtonNorth();          break;


                default:                await UniTask.Yield();  break;
            }


        }

    }

    #endregion

    #region PlayerTurn
    
    /// <summary>自分のターン開始時に先頭の所有駒へカーソルを合わせます。</summary>
    private void PlayerTurnInit()
    {
        nowPosPick = _player.allTheChess.Keys.First();
        _chessBoard.UpdatePlayerChose(nowPosPick);
    }
    /// <summary>対局中にプレイヤーが操作可能な入力段階か判定します。</summary>
    private bool IsPlayerTurn()=> inputStage != InputStage.None && inputStage != InputStage.ChooseSkill;
    /// <summary>操作可能な場合に盤面カーソルを上へ移動します。</summary>
    private void Dpad_Up()
    {
        if (!isPause && IsPlayerTurn())
        {
            ReturnToBoardPosition(Vector2Int.up);
        }
            
    }
    /// <summary>操作可能な場合に盤面カーソルを下へ移動します。</summary>
    private void Dpad_Down()
    {
        if (!isPause && IsPlayerTurn())
            ReturnToBoardPosition(Vector2Int.down);
    }
    /// <summary>手番中の盤面カーソル移動、駒選択・配置、ポーズ操作を受け付けます。</summary>
    private async UniTask PlayerTurn(CancellationToken token)
    {
        PlayerTurnInit();
        while (IsPlayerTurn())
        {
            await UniTask.Yield(token);
            ButtonControl button = await _inPutManager.WaitForGamePadButtonInput(nowUsingGamepad);
            if (button == null) continue;

            switch (button.name)
            {
                case "start":           Pause();                break;

                case "up":              Dpad_Up();              break;
                case "down":            Dpad_Down();            break;
                case "right":           Dpad_Right();           break;
                case "left":            Dpad_Left();            break;

                case "buttonEast":      ButtonEast();           break;
                case "buttonSouth":     ButtonSouth();          break;

                case "buttonWest":      ButtonWest();           break;

                case "buttonNorth":     ButtonNorth();          break;


                default:                await UniTask.Yield(token);  break;
            }


        }

    }

    #endregion

    /// <summary>ゲーム段階が対局中またはターン切り替え中か判定します。</summary>
    private bool IsInGame()
    {
        return _gameManager.nowGameStage == GameStage.InGame
            || _gameManager.nowGameStage == GameStage.TurnChange;
    }
    /// <summary>対局ステージ中、入力段階に応じて手番・非手番の入力ループを切り替えます。</summary>
    private async UniTask InGameProcess(CancellationToken token)
    {
        while (true)
        {
            await UniTask.Yield(token);
            if (!IsInGame())
            {
                //Debug.Log("End Process");
                return;
            }
            if (inputStage == InputStage.None) 
                await NonPlayerTurn(token);

            else if (IsPlayerTurn())
                await PlayerTurn(token);
        }

    }

    #endregion

    #endregion
    /// <summary>現在実行中の入力監視処理を停止するためのキャンセルトークンを管理します。</summary>
    private CancellationTokenSource inputUpdate;

    /// <summary>
    /// 実行中の入力監視を停止し、キャンセルトークンと駒の選択状態を破棄します。
    /// 盤面のカーソル表示と移動可能範囲も初期状態へ戻します。
    /// </summary>
    public void RejectInput()
    {
        inputUpdate.Cancel();
        inputUpdate.Dispose();
        inputUpdate = null;

        if (pickIngChess != null)
        {
            pickIngChess.ReturnPick();
            pickIngChess = null;
        }
        _chessBoard.UpdatePlayerChose(new Vector2Int(-1, -1));
        _chessBoard.ReSetActive();
    }

    /// <summary>
    /// 現在使用している入力デバイスを判定し、対応する入力監視処理を開始します。
    /// </summary>
    public void StartInput()
    {
        switch (nowUsingDevice)
        {
            case CanUseDevice.Mouse: StartMouseInput(); break;
            case CanUseDevice.Gamepad:StartGamepadInput();break;
        }
    }



}
