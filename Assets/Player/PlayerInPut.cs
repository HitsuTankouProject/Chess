using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using static UnityEngine.Rendering.DebugUI;
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
    public void SetUseGamepadType(Gamepad gamepad)
    {
        if (gamepad != null) nowUsingDevice = CanUseDevice.Gamepad;
        else nowUsingDevice = CanUseDevice.Mouse;
    }
    public Gamepad nowUsingGamepad;
    public Mouse nowUsingMouse => Mouse.current;  

    public void ChangeToGamepad()
    {
        if (nowUsingGamepad == null)
        {
            ChangeToMouse();
            return;
        }
        nowUsingDevice = CanUseDevice.Gamepad;
        if(inputStage == InputStage.OneMoreMove)StartCoroutine(OneMoreMove(pickIngChess));
        else ChangeInput(CanUseDevice.Gamepad);

    }
    public void ChangeToMouse()
    {
        nowUsingDevice = CanUseDevice.Mouse;
        nowUsingGamepad = null;

        if (inputStage == InputStage.OneMoreMove) StartCoroutine(OneMoreMove(pickIngChess));
        else ChangeInput(CanUseDevice.Mouse);
    }

    private void ChangeInput(CanUseDevice usingDevice)
    {
        if (_gameManager.nowGameStage != GameStage.InGame) return;
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

        StartCoroutine(OneMoreMove(oneMoreMoveChess));
    }

    private IEnumerator OneMoreMove(ChessBasic oneMoreMoveChess)
    {
        _player.TurnCamera(PlayerCameraStage.Pick);

        pickIngChess = oneMoreMoveChess;
        pickIngChess.FindPossibleMove();
        pickIngChess.GotPick();

        nowPos = pickIngChess.position;
        _chessBoard.UpdatePlayerChose(nowPos);


        Debug.Log(pickIngChess.name);

        if (nowUsingDevice == CanUseDevice.Mouse) yield return Mouse_OneMoreMove();
        else if (nowUsingDevice == CanUseDevice.Gamepad) yield return GamePad_OneMoreMove();

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

    private bool IsSameLayer(int checkLayer, int sampleLayer) => (sampleLayer & (1 << checkLayer)) != 0;

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
        //Debug.Log(hitObject.name);

        if (inputStage == InputStage.Waiting) Press_Chess(ChessBoardPosition(hitObject));
        else if (inputStage == InputStage.Picking) Press_ChessBoard(ChessBoardPosition(hitObject));

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

    private IEnumerator MouseInPut()
    {
        while (true)
        {
            yield return null;
            if (inputStage! == InputStage.None || _gameManager.nowGameStage != GameStage.InGame) continue;
            PressAction();
        }

    }

    private IEnumerator Mouse_OneMoreMove()
    {
        while (true)
        {
            yield return null;
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
        inputUpdate = StartCoroutine(MouseInPut());
    }


    #endregion

    #region Gamepad

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
            case GameStage.ControllerChoose:
                //_inPutManager.controllerChoosePanel.player01Choose.Button_ChoosePsController();
                break;
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


    private void StartGamepadInput()
    {
        if (inputUpdate != null) RejectInput();
        inputUpdate = StartCoroutine(GamePadInPut());
    }

    #endregion


    private Coroutine inputUpdate;
    private void RejectInput()
    {
        StopAllCoroutines();
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

    public void InputWatcher()
    {
        if (nowUsingGamepad == null) return;

        if (nowUsingDevice == CanUseDevice.Mouse && nowUsingGamepad != null)
            StartGamepadInput();
        else if (nowUsingDevice == CanUseDevice.Gamepad && nowUsingGamepad == null)
            StartMouseInput();

    }

    private void Update()
    {
        //InputWatcher();
    }

}
