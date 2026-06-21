using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
public enum CanUseDevice { Mouse,Gamepad };
public enum InputStage { None, ChooseSkill,Waiting, Picking, OneMoreMove }

public class PlayerInPut : MonoBehaviour
{
    private ChessBoard _chessBoard => ChessBoard.Instance;

    private InPutManager _inPutManager => InPutManager.Instance;

    private int gameBoardLayerMask;
    private InGame _inGame => InGame.Instance;
    private Camera _camera;

    private Player _player;

    public InputStage inputStage/* { get; private set; } */= InputStage.None;

    public void Init(Player player)
    {
        _player = player;
        inputStage = InputStage.None;
        _camera = Camera.main;
        gameBoardLayerMask = LayerMask.GetMask("GameBoard");

    }


    #region Using Device
    public CanUseDevice nowUsingDevice/* { get; private set; } */= CanUseDevice.Mouse;
    public Gamepad nowUsingGamepad { get; private set; } = null;
    public Mouse nowUsingMouse => Mouse.current;

    public void ChangeToGamepad(Gamepad targetGamepad)
    {
        if (targetGamepad == null || !targetGamepad.added)
        {
            ChangeToMouse();
            return;
        }

        nowUsingGamepad = targetGamepad;
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
        if (_inGame.nowTurn != _player.usingChess || _inGame.inGameStage != InGameStage.TurnStart) return;

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
        if (chess.color != _inGame.nowTurn) return;

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

        if (!canMove) return false;

        ChessBasic moveChess = pickIngChess;

        pickIngChess = null;

        moveChess.Move(boardPos);

        return true;
    }

    public IEnumerator OneMoreMove(ChessBasic oneMoreMoveChess)
    {
        _player.nowPlayerStage = PlayerStage.MovingChess;

        inputStage = InputStage.OneMoreMove;

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
    private bool IsPressed(out GameObject hitObject)
    {
        if (!nowUsingMouse.rightButton.wasPressedThisFrame)
        {
            hitObject = null;
            return false;
        }
        Vector2 mousePos = nowUsingMouse.position.ReadValue();
        Ray rayResult = _camera.ScreenPointToRay(mousePos);
        bool isHit = Physics.Raycast(rayResult, out RaycastHit hit, 100f, _inPutManager.CanHitLayerMask(), QueryTriggerInteraction.Collide);
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
        bool isPressed = IsPressed(out GameObject hitObject);
        if (!isPressed) return;
        int hitLayer = hitObject.layer;
        Debug.Log(hitObject.name);

        if (IsSameLayer(hitLayer, _inPutManager.buttonLayerMask)) Press_Button(hitObject);
        //else if (IsSameLayer(hitLayer, _inPutManager.cardLayerMask)) Press_Card(hitObject);
        else if (inputStage == InputStage.Waiting) Press_Chess(ChessBoardPosition(hitObject));
        else if (inputStage == InputStage.Picking) Press_ChessBoard(ChessBoardPosition(hitObject));

    }


    private void Press_Card(GameObject cardObject)
    {
        if(!cardObject.TryGetComponent<Card>(out Card card))
        {
            Debug.LogError("hit card but no Card");
            return;
        }
    }
    private void Press_Button(GameObject buttonObject)
    {
        if (!buttonObject.TryGetComponent<MyButton>(out MyButton button))
        {
            Debug.LogError("hit button but no MyButton");
            return;
        }

        button.OnClick();
    }

    private void Press_Chess(Vector2Int boardPos)
    {
        if (boardPos == invalidBoardPos) return;

        _chessBoard.UpdatePlayerChose(boardPos);
        PickChess(boardPos);
    }

    private void Press_ChessBoard(Vector2Int boardPos)
    {
        if (!PutChess(boardPos))
        {
            pickIngChess.ReturnPick();
            pickIngChess = null;
            inputStage = InputStage.Waiting;
            return;
        }

        inputStage = InputStage.None;

    }

    private IEnumerator MouseInPut()
    {
        while (true)
        {
            yield return null;
            if (inputStage! == InputStage.None) continue;
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

            if (!PutChess(ChessBoardPosition(hitObject))) continue;
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
    private bool IsConformedKey()
    {
        if (nowUsingGamepad == null) return false;
        return nowUsingGamepad.buttonSouth.wasPressedThisFrame;
    }
    private bool IsCancelKey()
    {
        if (nowUsingGamepad == null) return false;
        return nowUsingGamepad.buttonEast.wasPressedThisFrame;
    }
    private void Conform(Vector2Int boardPos)
    {
        //switch (inputStage)
        //{
        //    case InputStage.Waiting:

        //        if (PickChess(boardPos))
        //        {
        //            inputStage = InputStage.Picking;
        //        }

        //        break;

        //    case InputStage.Picking:

        //        if (PutChess(boardPos))
        //        {
        //            inputStage = InputStage.None;
        //        }
        //        else
        //        {
        //            inputStage = InputStage.Waiting;
        //        }

        //        break;
        //}

        switch (inputStage)
        {
            case InputStage.Waiting: PickChess(boardPos); break;         
            case InputStage.Picking:

                if (PutChess(boardPos)) inputStage = InputStage.None;
                else inputStage = InputStage.Waiting;

            break;
        }
    }
   
    private Vector2Int GamepadLeftStick()
    {
        if (nowUsingGamepad == null) return Vector2Int.zero;

        if (!nowUsingGamepad.added)
        {
            ChangeToMouse();
            return Vector2Int.zero;
        }

        Vector2Int result = Vector2Int.zero;
        Vector2 leftStickValue = nowUsingGamepad.leftStick.ReadValue();
        float x = Mathf.Abs(leftStickValue.x);
        float y = Mathf.Abs(leftStickValue.y);

        int resultX = StickInput(x)? (leftStickValue.x > 0 ? 1 : -1) : 0;
        int resultY = StickInput(y) ? (leftStickValue.y > 0 ? 1 : -1) : 0;
        result = new Vector2Int(resultX, resultY);
        int offset = InGame.Instance.nowTurn == ChessColor.White ? 1 : -1;
        return result * offset;
    }
    private bool TryMoveCursor(Vector2Int inputDirection)
    {
        if (inputDirection == Vector2Int.zero) return false;
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
        if (bestPos == nowPos) return false;
        nowPos = bestPos;
        _chessBoard.UpdatePlayerChose(nowPos);
        return true;

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

    private IEnumerator GamePadInPut()
    {
        if (_player.allTheChess.Count == 0) yield break;

        bool isGamepadInputAccessible = true;
        nowPos = _player.allTheChess.Keys.First();
        _chessBoard.UpdatePlayerChose(nowPos);

        while (true)
        {
            if (inputStage == InputStage.None) yield break;

            if (!isGamepadInputAccessible)
            {
                yield return new WaitForSeconds(GamepadInputCd);
                isGamepadInputAccessible = true;
                continue;
            }
            Vector2Int inputDirection = GamepadLeftStick();

            if (inputDirection != Vector2Int.zero)
            {
                if (TryMoveCursor(inputDirection)) isGamepadInputAccessible = false;
            }

            if(IsConformedKey()) Conform(nowPos);
            else if (IsCancelKey())
            {
                if (inputStage == InputStage.Picking)
                {
                    pickIngChess.ReturnPick();
                    pickIngChess = null;
                    _chessBoard.ReSetActive();
                    inputStage = InputStage.Waiting;

                }
            }

            yield return null;
        }

    }

    private IEnumerator GamePad_OneMoreMove()
    {
        bool isGamepadInputAccessible = true;

        while (true)
        {
            if (!isGamepadInputAccessible)
            {
                yield return new WaitForSeconds(GamepadInputCd);
                isGamepadInputAccessible = true;
                continue;
            }
            yield return null;

            Vector2Int inputDirection = GamepadLeftStick();
            if (inputDirection != Vector2Int.zero)
            {
                if (TryMoveCursor(inputDirection)) isGamepadInputAccessible = false;
            }
            if (!IsConformedKey()) continue;
            bool canMove = pickIngChess.possibleMoveList.Contains(nowPos);

            if (!canMove) continue;

            ChessBasic moveChess = pickIngChess;
            pickIngChess = null;
            moveChess.Move(nowPos);
            _chessBoard.ReSetActive();

            yield break;

        }
    }


    private void StartGamepadInput()
    {
        if (inputUpdate != null) RejectInput();
        inputStage = InputStage.Waiting;
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


}
