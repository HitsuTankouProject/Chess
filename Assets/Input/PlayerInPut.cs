using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.InputSystem.LowLevel;
using Unity.VisualScripting;
public enum CanUseDevice { Mouse,Gamepad };
public enum InputStage { None, Waiting, Picking, OneMoreMove }

public class PlayerInPut : MonoBehaviour
{
    private ChessBoard _chessBoard => ChessBoard.Instance;
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
        //if (nowUsingDevice == CanUseDevice.Gamepad) StartCoroutine(GamePadInPut());
    }


    #region Using Device
    public CanUseDevice nowUsingDevice/* {  get; private set; } */= CanUseDevice.Mouse;
    public GamepadData lastConnectingGamepadData { get; private set; } = null;
    public Gamepad nowUsingGamepad { get; private set; } = null;
    public Mouse nowUsingMouse => Mouse.current;

    public void ChangeToGamepad(Gamepad targetGamepad)
    {
        nowUsingGamepad = targetGamepad;
        lastConnectingGamepadData = new GamepadData(targetGamepad);
        nowUsingDevice = CanUseDevice.Gamepad;
    }
    public void ChangeToMouse()
    {
        nowUsingDevice = CanUseDevice.Mouse;
        nowUsingGamepad = null;
    }

    #endregion

    public void StartInPutSystem() => inputStage = InputStage.Waiting;

    [SerializeField] private ChessBasic pickIngChess;
    private readonly Vector2Int invalidBoardPos = new(-1, -1);
    private bool PickChess(Vector2Int boardPos)
    {
        bool haveChess =
            _chessBoard.board.TryGetValue(boardPos, out ChessBasic targetChess);

        if (!haveChess)
        {
            return false;
        }

        bool sameColor = targetChess.color == _inGame.nowTurn;

        if (!sameColor)
        {
            return false;
        }

        pickIngChess = targetChess;
        pickIngChess.FindPossibleMove();
        pickIngChess.GotPick();
        return true;
    }
    private bool PutChess(Vector2Int boardPos)
    {
        if (pickIngChess == null)
        {
            return false;
        }

        bool canMove =
            pickIngChess.possibleMoveList.Contains(boardPos);

        _chessBoard.ReSetActive();

        if (!canMove)
        {
            return false;
        }

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

        Debug.Log(pickIngChess.name);

        while (pickIngChess != null)
        {
            yield return null;

            if (!TryGetPressedBoardPos(out Vector2Int boardPos))
            {
                continue;
            }

            bool canMove =
                pickIngChess.possibleMoveList.Contains(boardPos);

            if (!canMove)
            {
                continue;
            }

            ChessBasic moveChess = pickIngChess;

            pickIngChess = null;

            moveChess.Move(boardPos);

            _chessBoard.ReSetActive();

            break;
        }

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

    private void StartMouseInput()
    {
        if (inputUpdate != null) RejectInput();
        inputStage = InputStage.Waiting;
        inputUpdate = StartCoroutine(MouseInPut());
    }

    #endregion

    #region Gamepad

    private enum GamepadInputUse { Button, Stick }
    private GamepadInputUse nowGamepadInputUse = GamepadInputUse.Button;

    private Vector2Int GamepadInputButton()
    {
        Vector2Int result = Vector2Int.zero;
        if (nowUsingGamepad == null) return result;

        int offset = InGame.Instance.nowTurn == ChessColor.White ? 1 : -1;
        if (nowUsingGamepad.dpad.up.isPressed) result = Vector2Int.up;
        else if (nowUsingGamepad.dpad.down.isPressed) result = Vector2Int.down;
        else if (nowUsingGamepad.dpad.left.isPressed) result = Vector2Int.left;
        else if (nowUsingGamepad.dpad.right.isPressed) result = Vector2Int.right;

        return result * offset;
    }

    private Vector2Int nowPos = Vector2Int.zero;

    private Vector2Int rightStickInput => Vector2Int.RoundToInt(nowUsingGamepad.rightStick.ReadValue());

    private bool haveRightStickInput => rightStickInput.magnitude > 0.5f;

    private const float GamepadInputCd = 0.075f;

    private bool IsConformed()
    {
        return nowUsingGamepad.buttonSouth.wasPressedThisFrame;
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
            Vector2Int inputDirection = GamepadInputButton();

            if (inputDirection != Vector2Int.zero)
            {
                Vector2Int targetPos = nowPos + inputDirection;

                while (!_chessBoard.IsOutOfBoard(targetPos))
                {
                    if (_player.allTheChess.ContainsKey(targetPos))
                    {
                        nowPos = targetPos;
                        _chessBoard.UpdatePlayerChose(nowPos);

                        isGamepadInputAccessible = false;
                        break;
                    }

                    targetPos += inputDirection;
                }
            }

            if (IsConformed())
            {
                Conform(nowPos);
                isGamepadInputAccessible = true;
            }


            yield return null;
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
        StartGamepadInput();
    }


    public bool test;
    public void InPutSystem_Update()
    {
        if (test)
        {
            StartInput();
            test = false;
        }


    }


}
