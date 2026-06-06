using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
public enum CanUseDevice { Mouse,Gamepad };
public enum InputStage { None, Waiting, Picking, OneMoreMove }

public class PlayerInPut : MonoBehaviour
{
    private ChessBoard _chessBoard => ChessBoard.Instance;
    private int gameBoardLayerMask;
    private InGame _inGame => InGame.Instance;
    private Camera _camera;

    private Player _player;

    public InputStage inputStage { get; private set; } = InputStage.None;

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

    public void StartInPutSystem() => inputStage = InputStage.Waiting;

    [SerializeField] private ChessBasic pickIngChess;
    private readonly Vector2Int invalidBoardPos = new(-1, -1);
    private bool PickChess(Vector2Int boardPos)
    {
        bool haveChess = _chessBoard.board.TryGetValue(boardPos, out ChessBasic targetChess);

        if (!haveChess) return false;

        bool sameColor = targetChess.color == _inGame.nowTurn;

        if (!sameColor) return false;

        pickIngChess = targetChess;
        pickIngChess.FindPossibleMove();
        pickIngChess.GotPick();
        return true;
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

    private bool TryGetBoardPos(Vector3 worldPos, out Vector2Int boardPos)
    {
        Ray ray = new Ray(worldPos + Vector3.up * 10f, Vector3.down);

        bool isHitGameBoard = Physics.Raycast(ray, out RaycastHit hit, 100f, gameBoardLayerMask, QueryTriggerInteraction.Collide);
        if (!isHitGameBoard)
        {
            boardPos = invalidBoardPos;
            _chessBoard.UpdatePlayerChose(boardPos);
            return false;
        }
        string[] split = hit.collider.gameObject.name.Split('_');

        if (!int.TryParse(split[1], out int x) || !int.TryParse(split[2], out int y)) 
        {
            boardPos = invalidBoardPos;
            return false;
        }
        boardPos = new Vector2Int(x, y);
        _chessBoard.UpdatePlayerChose(boardPos);
        return true;
     
    }
    private bool TryGetMouseWorldPos(out Vector3 mouseWorldPos)
    {
        Vector2 mousePos = nowUsingMouse.position.ReadValue();

        Ray ray = _camera.ScreenPointToRay(mousePos);

        Plane boardPlane = new Plane(Vector3.up, Vector3.zero);

        if (boardPlane.Raycast(ray, out float enter))
        {
            mouseWorldPos = ray.GetPoint(enter);
            return true;
        }

        mouseWorldPos = Vector3.zero;
        return false;
    }

    private bool IsPressed()
    {
        return nowUsingMouse.rightButton.wasPressedThisFrame &&
               InGame.Instance.inGameStage == InGameStage.TurnStart;
    }

    private bool TryGetPressedBoardPos(out Vector2Int boardPos)
    {
        boardPos = invalidBoardPos;

        if (!IsPressed()) return false;
        if (!TryGetMouseWorldPos(out Vector3 mouseWorldPos)) return false;
        return TryGetBoardPos(mouseWorldPos, out boardPos);
    }

    private IEnumerator MouseInPut()
    {
        while (true)
        {
            if (inputStage == InputStage.None)  yield break;
            if (!TryGetPressedBoardPos(out Vector2Int boardPos))
            {
                yield return null;
                continue;
            }
            if(inputStage == InputStage.Waiting)
            {
                if (!PickChess(boardPos))
                {
                    yield return null;
                    continue;
                }
                inputStage = InputStage.Picking;
                yield return null;
                continue;
            }
            else if (inputStage == InputStage.Picking)
            {
                if (!PutChess(boardPos))
                {
                    pickIngChess.ReturnPick();
                    pickIngChess = null;
                    inputStage = InputStage.Waiting;
                    yield return null;
                    continue;
                }

                inputStage = InputStage.None;
                yield return null;
            }
        }

    }

    private IEnumerator Mouse_OneMoreMove()
    {
        while (true)
        {
            yield return null;
            if (!TryGetPressedBoardPos(out Vector2Int boardPos)) continue;
            bool canMove = pickIngChess.possibleMoveList.Contains(boardPos);
            if (!canMove) continue;
            ChessBasic moveChess = pickIngChess;
            pickIngChess = null;
            moveChess.Move(boardPos);
            _chessBoard.ReSetActive();

            yield break;

        }
    }

    private void StartMouseInput()
    {
        if (inputUpdate != null) RejectInput();
        inputStage = InputStage.Waiting;
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
        switch (inputStage)
        {
            case InputStage.Waiting:

                if (PickChess(boardPos))
                {
                    inputStage = InputStage.Picking;
                }

                break;

            case InputStage.Picking:

                if (PutChess(boardPos))
                {
                    inputStage = InputStage.None;
                }
                else
                {
                    inputStage = InputStage.Waiting;
                }

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
