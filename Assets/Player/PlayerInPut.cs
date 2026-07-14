using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public enum CanUseDevice { Mouse,Gamepad };
public enum InputStage { None, ChooseSkill,Waiting, Picking, OneMoreMove }

public class PlayerInPut : MonoBehaviour
{

    private GameManager _gameManager => GameManager.Instance;

    private ChessBoard _chessBoard => _gameManager.chessBoard;

    private InPutManager _inPutManager => _gameManager.inPutManager;

    private int gameBoardLayerMask;

    private Camera _camera;

    private Player _player;

    public InputStage inputStage/* { get; private set; } */= InputStage.None;



    public void Init(Player player)
    {
        _player = player;
        inputStage = InputStage.None;
        _camera = player.playerCanvas.playerCamera;
        gameBoardLayerMask = LayerMask.GetMask("GameBoard");

    }


    #region Using Device
    public CanUseDevice nowUsingDevice/* { get; private set; } */= CanUseDevice.Mouse;

    public Gamepad nowUsingGamepad;
    public Mouse nowUsingMouse => Mouse.current;

    public void ChangeToGamepad(Gamepad gamepad = null)
    {
        if (gamepad == null)
        {
            ChangeToMouse();
            return;
        }

        nowUsingDevice = CanUseDevice.Gamepad;

        if (nowUsingGamepad == gamepad)
        {
            ChangeInput(CanUseDevice.Gamepad);
            return;
        }

        nowUsingGamepad = gamepad;
        ChangeInput(CanUseDevice.Gamepad);
    }


    public void ChangeToMouse()
    {
        nowUsingDevice = CanUseDevice.Mouse;
        ChangeInput(CanUseDevice.Mouse);
    }

    private void ChangeInput(CanUseDevice usingDevice)
    {
        switch (usingDevice)
        {
            case CanUseDevice.Mouse:StartMouseInput();break;
            case CanUseDevice.Gamepad:StartGamepadInput();break;
        }
    }

    #endregion

    public void StartGame() => inputStage = InputStage.Waiting;
    public void StartChoose() => inputStage = InputStage.ChooseSkill;

    [SerializeField] private ChessBasic pickIngChess;
    private readonly Vector2Int invalidBoardPos = new(-1, -1);

    private void PickChess(Vector2Int boardPos)
    {
        bool haveChess = _chessBoard.board.TryGetValue(boardPos, out ChessBasic chess);
        if (!haveChess) return;
        if (chess.color != _gameManager.nowTurn) return;
        //Debug.Log(chess.name);

        _player.TurnCamera(PlayerCameraStage.Pick);

        pickIngChess = chess;
        pickIngChess.FindPossibleMove();
        pickIngChess.GotPick();
        inputStage = InputStage.Picking;
    }
    private bool PutChess(Vector2Int boardPos)
    {
        if (pickIngChess == null) return false;
        
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

    private int CanHitLayerMask()
    {
        if (inputStage == InputStage.Waiting) return _inPutManager.chessLayerMask;
        else if (inputStage == InputStage.Picking) return _inPutManager.gameBoardLayerMask;
        else return -1;
    }

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

    private Vector2Int ChessBoardPosition(GameObject hitObject) 
    {
        if(hitObject.TryGetComponent<ChessBasic>(out ChessBasic chess))
            return chess.position;
        if (hitObject.TryGetComponent<ChessBlock>(out ChessBlock chessBlock)) 
            return chessBlock.position;
        return invalidBoardPos;
    }

    private void Press_Chess(Vector2Int boardPos)
    {
        if (boardPos == invalidBoardPos) return;
        _chessBoard.UpdatePlayerChose(boardPos);
        PickChess(boardPos);
    }
    private void Press_ChessBoard(Vector2Int boardPos)
    {
        if (!PutChess(boardPos)) return;

        if (inputStage != InputStage.OneMoreMove)
        {
            _player.TurnCamera(PlayerCameraStage.Normal);
            inputStage = InputStage.None;
        }
    }

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
    private void PressAction()
    {
        bool isPressed = IsPressed(out GameObject hitObject);
        if (!isPressed) return;
        int hitLayer = hitObject.layer;
        //Debug.Log(hitObject.name);

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

    private async UniTask MouseInPut(CancellationToken token)
    {
        while (true)
        {
            await UniTask.Yield(token);
            if (_gameManager.nowGameStage != GameStage.InGame) continue;
            PressAction();
        }

    }

    private void StartMouseInput()
    {
        if (inputUpdate != null) RejectInput();
        inputUpdate = new CancellationTokenSource();
        MouseInPut(inputUpdate.Token).Forget();

    }


    #endregion

    #region Gamepad
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
    private ChooseSkillPanel _chooseSkillPanel => _gameManager.chooseSkillPanel;
    private bool IsNowPickTurn()
    {
        return inputStage == InputStage.ChooseSkill && _gameManager.chooseSkillPanel.chooseSkillPlayerColor == _player.usingChess;
    }
    private void PickNextCard() => _chooseSkillPanel.PickNextCard();
    private void PickBackCard() => _chooseSkillPanel.PickBackCard();

    private void Return()
    {
        if (_chooseSkillPanel.isPicking) _chooseSkillPanel.Button_Return(); 
        else _chooseSkillPanel.Button_DrawAgain();
    }
    private void ConFirm()
    {
        if (_chooseSkillPanel.isPicking)
        {
            _chooseSkillPanel.Button_ConFirm();
            RejectInput();

        }
        else _chooseSkillPanel.PickThatCard();
    }

    private async UniTask WaitGamePadInput_GameSkillChoose(CancellationToken token)
    {
        while (IsNowPickTurn())
        {
            ButtonControl button = await _inPutManager.WaitForGamePadButtonInput(nowUsingGamepad);
            await UniTask.Yield(token);

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
    private SkillDescriptionPanel skillDescriptionPanel => _player.playerCanvas.skillDescriptionPanel;
    private bool isCardDescriptionOpen => skillDescriptionPanel.gameObject.activeSelf;
    private bool isPause => _player.isPause;
    public Vector2Int nowPosPick = Vector2Int.zero;

    private void Pause() => _player.playerCanvas.Button_Pause();

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

    private void ReturnToBoardPosition(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return;
        int offset = _player.usingChess == ChessColor.White ? 1 : -1;
        TryMoveCursor(dir * offset);
    }

    private void Dpad_Left()
    {
        if (isPause)
            _player.playerCanvas.BackCard();
        else
            if (IsPlayerTurn()) ReturnToBoardPosition(Vector2Int.left);
    }
    private void Dpad_Right()
    {
        if (isPause)
            _player.playerCanvas.NextCard();
        else
            if (IsPlayerTurn()) ReturnToBoardPosition(Vector2Int.right);
    }

    private void ButtonEast()
    {
        if (isPause && isCardDescriptionOpen)
            skillDescriptionPanel.Button_Return(); 
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

    private void ButtonSouth()
    {
        if (isPause&& !isCardDescriptionOpen)
            _player.playerCanvas.WatchBuffSkillDescription();
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
    private void ButtonWest()
    {
        if (isPause) _player.playerCanvas.Button_BackToGameTitle();
    }
    private void ButtonNorth()
    {
        if (isPause) _player.playerCanvas.Button_Surrender();
    }




    #region Non Player Turn


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
    private void PlayerTurnInit()
    {
        nowPosPick = _player.allTheChess.Keys.First();
        _chessBoard.UpdatePlayerChose(nowPosPick);
    }
    private bool IsPlayerTurn()=> inputStage != InputStage.None && inputStage != InputStage.ChooseSkill;
    private void Dpad_Up()
    {
        if (!isPause && IsPlayerTurn())
        {
            ReturnToBoardPosition(Vector2Int.up);
        }
            
    }
    private void Dpad_Down()
    {
        if (!isPause && IsPlayerTurn())
            ReturnToBoardPosition(Vector2Int.down);
    }


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
                case "start":     Pause();                break;

                case "up":              Dpad_Up();              break;
                case "down":            Dpad_Down();            break;
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

    private bool IsInGame()
    {
        return _gameManager.nowGameStage == GameStage.InGame
            || _gameManager.nowGameStage == GameStage.TurnChange;
    }

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

    //private void TryMoveCursor(Vector2Int inputDirection)
    //{
    //    Vector2 searchDirection = ((Vector2)inputDirection).normalized;
    //    Vector2Int bestPos = nowPosPick;
    //    float bestScore = float.MinValue;
    //    foreach (Vector2Int targetPos in GetAreas())
    //    {
    //        if (targetPos == nowPosPick) continue;
    //        Vector2 offset = targetPos - nowPosPick;
    //        if (offset == Vector2.zero) continue;
    //        float dot = Vector2.Dot(offset.normalized, searchDirection);

    //        if (dot < 0.5f) continue;

    //        float distance = offset.sqrMagnitude;
    //        float score = dot * 100f - distance;
    //        if (score > bestScore)
    //        {
    //            bestScore = score;
    //            bestPos = targetPos;
    //        }

    //    }
    //    if (bestPos == nowPosPick) return;
    //    nowPosPick = bestPos;
    //    _chessBoard.UpdatePlayerChose(nowPosPick);
    //}
    //private Vector2Int nowPos = Vector2Int.zero;

    //private const float GamepadInputCd = 0.075f;

    //private const float stickInputThreshold = 0.4f;

    //private bool StickInput(float targetIndex) => targetIndex > stickInputThreshold;
    //private bool IsLeftStickInputed(Vector2 input, out Vector2Int dir)
    //{
    //    dir = Vector2Int.zero;
    //    bool result =
    //        Mathf.Abs(input.x) > stickInputThreshold &&
    //        Mathf.Abs(input.y) > stickInputThreshold;

    //    if (result)
    //        dir = new Vector2Int(Mathf.CeilToInt(input.x), Mathf.CeilToInt(input.y));

    //    return result;
    //}

    #endregion

    private CancellationTokenSource inputUpdate;
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


    public void StartInput()
    {
        switch (nowUsingDevice)
        {
            case CanUseDevice.Mouse: StartMouseInput(); break;
            case CanUseDevice.Gamepad:StartGamepadInput();break;
        }
    }



}
