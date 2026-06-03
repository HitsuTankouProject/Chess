using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
public enum CanUseDevice { Mouse,Gamepad };
public enum InputStage { None, Waiting, Picking, OneMoreMove }

[System.Serializable]
public class PlayerInPut
{
    private ChessBoard _chessBoard => ChessBoard.Instance;
    private InGame _inGame => InGame.Instance;
    private Player _player;
    public InputStage inputStage/* { get; private set; } */= InputStage.None;
    public PlayerInPut(Player player)
    {
        _player = player;
        inputStage = InputStage.None;
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
            pickIngChess.ReturnPick();
            pickIngChess = null;
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

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            100f,
            LayerMask.GetMask("GameBoard"),
            QueryTriggerInteraction.Collide))
        {
            Debug.Log(hit.collider.gameObject.name);

            string[] split = hit.collider.gameObject.name.Split('_');

            boardPos = new Vector2Int(
                int.Parse(split[1]),
                int.Parse(split[2]));

            return true;
        }

        boardPos = new Vector2Int(-1, -1);
        return false;
    }
    private bool TryGetMouseWorldPos(out Vector3 mouseWorldPos)
    {
        Vector2 mousePos = nowUsingMouse.position.ReadValue();

        Ray ray = Camera.main.ScreenPointToRay(mousePos);

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
        boardPos = new Vector2Int(-1, -1);

        if (!IsPressed())
        {
            return false;
        }

        Debug.Log("Pressed");

        if (!TryGetMouseWorldPos(out Vector3 mouseWorldPos))
        {
            return false;
        }

        return TryGetBoardPos(mouseWorldPos, out boardPos);
    }



    private void Waiting()
    {
        if (!TryGetPressedBoardPos(out Vector2Int boardPos))
        {
            return;
        }

        bool pickSuccess = PickChess(boardPos);

        if (!pickSuccess)
        {
            return;
        }

        inputStage = InputStage.Picking;
    }

    private void PickingChess()
    {
        if (!TryGetPressedBoardPos(out Vector2Int boardPos))
        {
            return;
        }

        bool putSuccess = PutChess(boardPos);

        if (!putSuccess)
        {
            inputStage = InputStage.Waiting;
            return;
        }

        inputStage = InputStage.None;
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
        if (nowUsingGamepad.dpad.up.wasPressedThisFrame) result = Vector2Int.up;
        else if (nowUsingGamepad.dpad.down.wasPressedThisFrame) result = Vector2Int.down;
        else if (nowUsingGamepad.dpad.left.wasPressedThisFrame) result = Vector2Int.left;
        else if (nowUsingGamepad.dpad.right.wasPressedThisFrame) result = Vector2Int.right;

        return result * offset;
    }

    private Vector2Int nowPos = Vector2Int.zero;

    private Vector2Int rightStickInput => Vector2Int.RoundToInt(nowUsingGamepad.rightStick.ReadValue());

    private bool haveRightStickInput => rightStickInput.magnitude > 0.5f;

    private const float GamepadInputCd = 0.2f;

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
            if(!isGamepadInputAccessible)
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



#endregion

public void InPutSystem_Update()
{
        switch (inputStage)
        {
            case InputStage.None: pickIngChess = null; return;
            case InputStage.Waiting: Waiting(); return;
            case InputStage.Picking: PickingChess(); return;
        }
}


}
