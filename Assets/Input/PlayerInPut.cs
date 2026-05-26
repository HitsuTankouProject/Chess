using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.GraphicsBuffer;

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
    public CanUseDevice nowUsingDevice {  get; private set; } = CanUseDevice.Mouse;
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

    public Vector2Int FindPos(Vector3 targetClick)
    {
        Vector2Int result;

        Ray ray = new Ray(targetClick + Vector3.up * 10f, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("GameBoard"), QueryTriggerInteraction.Collide))
        {
            Debug.Log(hit.collider.gameObject.name);
            string[] split = hit.collider.gameObject.name.Split('_');
            result = new Vector2Int(int.Parse(split[1]), int.Parse(split[2]));
            return result;
        }
        else return new Vector2Int(-1, -1);
    }

    private bool PickChess(Vector2Int result)
    {
        bool posHaveChess = _chessBoard.board.TryGetValue(result, out ChessBasic targetChess);
        if (!posHaveChess) return false;

        bool sameColor = targetChess.color == _inGame.nowTurn;
        if (!sameColor) return false;

        pickIngChess = _chessBoard.board[result];
        pickIngChess.FindPossibleMove();
        pickIngChess.GotPick();
        return true;
    }

    private bool PutChess(Vector2Int result)
    {
        bool chessCanGo = pickIngChess.possibleMoveList.Contains(result);

        _chessBoard.ReSetActive();
        if (!chessCanGo)
        {
            pickIngChess.ReturnPick();
            pickIngChess = null;
            return false;
        }

        pickIngChess.Move(result);

        
        return true;
    }


    private bool IsFoundPos(Vector3 targetClick, out Vector2Int result)
    {
        Ray ray = new Ray(targetClick + Vector3.up * 10f, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("GameBoard"), QueryTriggerInteraction.Collide))
        {
            Debug.Log(hit.collider.gameObject.name);
            string[] split = hit.collider.gameObject.name.Split('_');
            result = new Vector2Int(int.Parse(split[1]), int.Parse(split[2]));
            return true;
        }
        else
        {
            result = new Vector2Int(-1, -1);
            return false;
        } 

    }
    private bool IsPressedInBoard(out Vector3 mouseWorldPos)
    {
        Vector2 mouse = nowUsingMouse.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mouse);
        Plane boardPlane = new Plane(Vector3.up, Vector3.zero);

        if (boardPlane.Raycast(ray, out float enter))
        {
            mouseWorldPos = ray.GetPoint(enter);
            return true;
        }
        else
        {
            mouseWorldPos = new Vector3(-1, -1, -1);
            return false;
        }
    }
    private bool IsPressed()
    {
        return nowUsingMouse.rightButton.wasPressedThisFrame && InGame.Instance.inGameStage == InGameStage.TurnStart;
    }

    private bool IsPressProcessAccessible(out Vector2Int result)
    {
        if (!IsPressed())
        {
            result = new Vector2Int(-1, -1);
            return false;
        }
        Debug.Log("Pressed");
        bool isPressedInBoard = IsPressedInBoard(out Vector3 mouseWorldPos);
        if (!isPressedInBoard)
        {
            result = new Vector2Int(-1, -1);
            return false;
        }

        return IsFoundPos(mouseWorldPos, out result);
    }

    private void PickingChess()
    {
        bool isPressProcessAccessible = IsPressProcessAccessible(out Vector2Int target);
        if (!isPressProcessAccessible) return;
        bool isPutSuccess = PutChess(target);
        if (!isPutSuccess)
        {
            inputStage = InputStage.Waiting;
            return;
        }

        inputStage = InputStage.None;
    }

    private void Waiting()
    {
        bool isPressProcessAccessible = IsPressProcessAccessible(out Vector2Int target);
        if (!isPressProcessAccessible) return;
        bool isPickSuccess = PickChess(target);
        if (!isPickSuccess) return;
        inputStage = InputStage.Picking;
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
            bool isPressProcessAccessible = IsPressProcessAccessible(out Vector2Int target);
            if (!isPressProcessAccessible) continue;

            bool chessCanGo = pickIngChess.possibleMoveList.Contains(target);
            if (!chessCanGo) continue;
            pickIngChess.Move(target);
            _chessBoard.ReSetActive();

            break;
        }
        pickIngChess = null;
        _player.nowPlayerStage = PlayerStage.ReadytoEnd;
    }


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
