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
        nowUsingGamepad = gamepad;
        nowUsingDevice = CanUseDevice.Gamepad;



        //if (inputStage == InputStage.OneMoreMove) StartCoroutine(OneMoreMove(pickIngChess));
        //else ChangeInput(CanUseDevice.Gamepad);

    }
    public void ChangeToMouse()
    {
        nowUsingDevice = CanUseDevice.Mouse;

        if (inputStage == InputStage.OneMoreMove) OneMoreMove(pickIngChess).Forget();
        else ChangeInput(CanUseDevice.Mouse);
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
        Debug.Log(chess.name);
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
        _player.nowPlayerStage = PlayerStage.MovingChess;

        OneMoreMove(oneMoreMoveChess).Forget();
    }

    private async UniTask OneMoreMove(ChessBasic oneMoreMoveChess)
    {
        _player.TurnCamera(PlayerCameraStage.Pick);

        pickIngChess = oneMoreMoveChess;
        pickIngChess.FindPossibleMove();
        pickIngChess.GotPick();

        nowPos = pickIngChess.position;
        _chessBoard.UpdatePlayerChose(nowPos);


        Debug.Log(pickIngChess.name);

        if (nowUsingDevice == CanUseDevice.Mouse) await Mouse_OneMoreMove();
        //else if (nowUsingDevice == CanUseDevice.Gamepad) yield return GamePad_OneMoreMove();

        _player.nowPlayerStage = PlayerStage.ReadytoEnd;
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
        if (!nowUsingMouse.leftButton.wasPressedThisFrame || _player.isPause)
        {
            hitObject = null;
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

    private void PressAction()
    {
        if (inputStage == InputStage.OneMoreMove) return;
        bool isPressed = IsPressed(out GameObject hitObject);
        if (!isPressed) return;
        int hitLayer = hitObject.layer;
        Debug.Log(hitObject.name);

  
        if (inputStage == InputStage.Picking)
        {
            Press_ChessBoard(ChessBoardPosition(hitObject));
            return;
        }
        else if (inputStage == InputStage.Waiting)
        {
            Press_Chess(ChessBoardPosition(hitObject));
            return;
        }
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

        if(inputStage!= InputStage.OneMoreMove)
        {
            _player.TurnCamera(PlayerCameraStage.Normal);
            inputStage = InputStage.None;
        }


    }

    private async UniTask MouseInPut(CancellationToken token)
    {
        while (true)
        {
            await UniTask.Yield();
            if (inputStage == InputStage.None || _gameManager.nowGameStage != GameStage.InGame) return;
            PressAction();
        }

    }
    private async UniTask Mouse_OneMoreMove()
    {
        while (true)
        {
            await UniTask.Yield();
            bool isPressed = IsPressed(out GameObject hitObject);
            if (!isPressed) continue;
            Vector2Int moveTo = ChessBoardPosition(hitObject);
            bool canMove = pickIngChess.possibleMoveList.Contains(moveTo);
            if(!canMove) continue;
            _chessBoard.ReSetActive();
            _player.TurnCamera(PlayerCameraStage.Normal);
            pickIngChess.ReturnPick();

            ChessBasic moveChess = pickIngChess;
            pickIngChess = null;
            moveChess.Move(moveTo);

            //if (!PutChess(ChessBoardPosition(hitObject))) continue;

            inputStage = InputStage.None;
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
            case GameStage.SkillChoose:     WaitGamePadInput_GameSkillChoose(inputUpdate.Token).Forget();     break;
            case GameStage.InGame: break;
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
            await UniTask.Yield();

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
    private bool isPause => _player.isPause;
    private Vector2Int nowPosPick = Vector2Int.zero;

    private void Pause()
    {
        pickCardIndex = 0;
        _player.playerCanvas.Button_Pause();
    }


    #region Non Player Turn
    private SkillDescriptionPanel skillDescriptionPanel => _player.playerCanvas.skillDescriptionPanel;
    private bool isCardDescriptionOpen => skillDescriptionPanel.gameObject.activeSelf;
    private int maxCanPick => _player.choseBuffs.Count - 1;
    private int pickCardIndex = 0;

    private void Pause_NextCard()
    {
        if(!isPause) { return; }
        pickCardIndex = Mathf.Min(pickCardIndex + 1, maxCanPick);
    }
    private void Pause_BackCard()
    {
        if (!isPause) { return; }
        pickCardIndex = Mathf.Max(pickCardIndex - 1, 0);
    }
    private void Pause_Confirm()
    {
        if (!isPause || isCardDescriptionOpen) { return; }
        _player.playerCanvas.cardActions[pickCardIndex]();

    }
    private void Pause_Return()
    {
        if (!isPause || !isCardDescriptionOpen) { return; }
        skillDescriptionPanel.Button_Return();
    }

    private void Pause_Surrender() => _player.playerCanvas.Button_Surrender();
    private void Pause_BackToGameTitle() => _player.playerCanvas.Button_BackToGameTitle();


    private async UniTask NonPlayerTurn(CancellationToken token)
    {
        while(inputStage == InputStage.None)
        {
            await UniTask.Yield();
            ButtonControl button = await _inPutManager.WaitForGamePadButtonInput(nowUsingGamepad);
            if (button == null) continue;

            switch (button.name)
            {
                case "startButton": Pause(); break;

                case "right": Pause_NextCard(); break;
                case "left": Pause_BackCard(); break;

                case "buttonEast": Pause_Confirm(); break;
                case "buttonSouth": Pause_Return(); break;

                case "buttonWest": Pause_BackToGameTitle(); break;

                case "buttonNorth": Pause_Surrender(); break;


                default: await UniTask.Yield(); break;
            }


        }

    }

    #endregion

    #region Player Turn
    private void NowPosInit()
    {
        nowPosPick = _player.allTheChess.Keys.First();
    }



    #endregion



    private async UniTask InGameProcess(CancellationToken token)
    {
        while (true)
        {
            if (_gameManager.nowGameStage != GameStage.InGame) return;
            await UniTask.Yield();

            if (inputStage == InputStage.None)
            {
                await NonPlayerTurn(token);
            }
            else if (inputStage == InputStage.Waiting || inputStage == InputStage.Picking)
            {
                NowPosInit();

            }
            else if (inputStage == InputStage.OneMoreMove)
            {

            }

        }

    }






    #endregion







    private Vector2Int nowPos = Vector2Int.zero;

    private const float GamepadInputCd = 0.075f;

    private const float stickInputThreshold = 0.4f;

    private bool StickInput(float targetIndex) => targetIndex > stickInputThreshold;
    private bool IsLeftStickInputed(Vector2 input, out Vector2Int dir)
    {
        dir = Vector2Int.zero;
        bool result =
            Mathf.Abs(input.x) > stickInputThreshold &&
            Mathf.Abs(input.y) > stickInputThreshold;

        if (result)
            dir = new Vector2Int(Mathf.CeilToInt(input.x), Mathf.CeilToInt(input.y));

        return result;
    }

    private void Confirm()
    {
        switch (inputStage)
        {
            case InputStage.Waiting: PickChess(nowPos); break;         
            case InputStage.Picking:

                if (PutChess(nowPos)) inputStage = InputStage.None;
                //else 
                //{
                //    _player.playerCanvas.TurnCamera(CameraStage.Normal);
                //    pickIngChess.ReturnPick();
                //    pickIngChess = null;
                //    inputStage = InputStage.Waiting;
                //}

            break;
        }
    }

    private void TryMoveCursor(Vector2Int inputDirection)
    {
        Vector2 searchDirection = ((Vector2)inputDirection).normalized; 
        Vector2Int bestPos = nowPos;
        float bestScore = float.MinValue;
        foreach (Vector2Int targetPos in GetAreas())
        {
            if (targetPos == nowPos) continue;
            Vector2 offset = targetPos - nowPos;
            if (offset == Vector2.zero) continue;
            float dot = Vector2.Dot(offset.normalized, searchDirection);

            if (dot < 0.5f) continue;

            float distance = offset.sqrMagnitude;
            float score = dot * 100f - distance;
            if (score > bestScore)
            {
                bestScore = score;
                bestPos = targetPos;
            }

        }
        if (bestPos == nowPos) return;
        nowPos = bestPos;
        _chessBoard.UpdatePlayerChose(nowPos);
    }
    private IEnumerable<Vector2Int> GetAreas()
    {
        if (inputStage == InputStage.Waiting) 
        {
            if (_player != null) return _player.allTheChess.Keys;
        }
        else if (inputStage == InputStage.Picking||inputStage == InputStage.OneMoreMove)
        {
            if (pickIngChess != null) return pickIngChess.possibleMoveList;
        }
        Debug.LogError("it should go to here in GetAreas");
        return Enumerable.Empty<Vector2Int>();
    }

    private void ReturnToBoardPosition(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return;
        int offset = _player.usingChess == ChessColor.White ? 1 : -1;
        TryMoveCursor(dir * offset);
    }

    public void Press_Dpad_Left()
    {
       switch (_gameManager.nowGameStage)
       {
            case GameStage.InGame:
                if (inputStage == InputStage.None) return;
                ReturnToBoardPosition(Vector2Int.left);
                break;

            default : break;
        }
    }

    private void GamePad_None()
    {
        if (_gameManager.nowGameStage != GameStage.InGame) return;

        if (nowUsingGamepad.startButton.wasPressedThisFrame) 
            _player.playerCanvas.Button_Pause();
    }

    private int nowChooseSkillIndex = 0;
    private const int maxChooseSkillIndex = 2;

    private void GamePad_ChooseSkill()
    {
        if (!_gameManager.isPicking)
        {
            IsLeftStickInputed(nowUsingGamepad.leftStick.ReadValue(), out Vector2Int dir);
            if (nowUsingGamepad.dpad.left.wasPressedThisFrame || dir.x < 0)
            {
                if (nowChooseSkillIndex - 1 >= 0) nowChooseSkillIndex--;
            }
            else if (nowUsingGamepad.dpad.right.wasPressedThisFrame || dir.x > 0) 
            {
                if (nowChooseSkillIndex + 1 <= maxChooseSkillIndex) nowChooseSkillIndex++;

            }
            else if (nowUsingGamepad.buttonSouth.wasPressedThisFrame)
            {
                _gameManager.chooseSkillPanel.Button_OpenSkillDescriptionPanel(_gameManager.chooseSkillPanel.pickedThreeCard[nowChooseSkillIndex]);
            }
            else if (nowUsingGamepad.buttonEast.wasPressedThisFrame)
            {
                _gameManager.chooseSkillPanel.Button_DrawAgain(); 
            }
        }
        else
        {
            if (nowUsingGamepad.buttonSouth.wasPressedThisFrame)
            {
                _gameManager.chooseSkillPanel.Button_ConFirm();
            }
            else if (nowUsingGamepad.buttonEast.wasPressedThisFrame)
            {
                _gameManager.chooseSkillPanel.Button_Return();
            }

        }
    }

    private void GamePad_Gaming()
    {
        if (_gameManager.nowGameStage != GameStage.InGame) return;

        if (nowUsingGamepad.startButton.wasPressedThisFrame)
            _player.playerCanvas.Button_Pause();
        else if (IsLeftStickInputed(nowUsingGamepad.leftStick.ReadValue(), out Vector2Int dir))
            ReturnToBoardPosition(dir);
        else if (nowUsingGamepad.buttonSouth.wasPressedThisFrame)
            Confirm();
        else if (nowUsingGamepad.buttonEast.wasPressedThisFrame)
        {
            _player.TurnCamera(PlayerCameraStage.Normal);
            pickIngChess.ReturnPick();
            pickIngChess = null;
            inputStage = InputStage.Waiting;
        }

    }

    private IEnumerator GamePadInPut()
    {
        while (true)
        {
            yield return null;
            switch (inputStage)
            {
                case InputStage.None: GamePad_None(); break;
                case InputStage.ChooseSkill: GamePad_ChooseSkill(); break;
                case InputStage.Waiting: GamePad_Gaming(); break;
                case InputStage.Picking: GamePad_Gaming(); break;
                default: yield break;
            }


        }

    }

    private IEnumerator GamePad_OneMoreMove()
    {

        while (true)
        {
            yield return null;

            if (IsLeftStickInputed(nowUsingGamepad.leftStick.ReadValue(), out Vector2Int dir))
            {
                ReturnToBoardPosition(dir);
                continue;
            }
            else if (nowUsingGamepad.buttonSouth.wasPressedThisFrame)
            {
                Confirm();

            }
        

        }
    }


    private void StartGaamepadInput()
    {
        //if (inputUpdate != null) RejectInput();
        //inputUpdate = StartCoroutine(GamePadInPut());
    }

    #endregion

    private CancellationTokenSource inputUpdate;
    private void RejectInput()
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
