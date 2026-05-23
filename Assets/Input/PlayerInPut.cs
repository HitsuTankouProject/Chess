using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.XR;

public enum CanUseDevice { Mouse,Gamepad };
public enum PickStage{ None, FoundChess }
public class PlayerInPut : MonoBehaviour
{
    private ChessBoard _chessBoard => ChessBoard.Instance;
    private InGame _inGame => InGame.Instance;

    #region Using Device
    public CanUseDevice nowUsingDevice {  get; private set; } = CanUseDevice.Mouse;
    public GamepadData lastConnectingGamepadData { get; private set; } = null;
    public Gamepad nowUsingGamepad { get; private set; } = null;
    public Mouse nowUsingMouse => Mouse.current;

    public void ChangeToGamepad(Gamepad targetGamepad)
    {
        nowUsingGamepad = targetGamepad;
        lastConnectingGamepadData = new GamepadData(targetGamepad);
    }
    public void ChangeToMouse()
    {
        nowUsingDevice = CanUseDevice.Mouse;
        nowUsingGamepad = null;
    }
    #endregion

    [SerializeField] private PickStage pickStage = PickStage.None;
    [SerializeField] private ChessBasic pickIngChess;

    public Vector2Int FindPos(Vector3 targetClick)
    {
        Vector2Int result;
        //Debug.Log(targetClick);

        Ray ray = new Ray(targetClick + Vector3.up * 10f, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("GameBoard"), QueryTriggerInteraction.Collide))
        {
            //Debug.Log(hit.collider.gameObject.name);

            string[] split = hit.collider.gameObject.name.Split('_');
            result = new Vector2Int(int.Parse(split[1]), int.Parse(split[2]));

           // Debug.Log(result);
            return result;

        }
        else return new Vector2Int(-1, -1);
    }

    private void PickChess(Vector2Int result)
    {
        bool posHaveChess = _chessBoard.board.TryGetValue(result, out ChessBasic targetChess);
        if (!posHaveChess) return;

        bool sameColor = targetChess.color == _inGame.nowTurn;
        if (!sameColor) return;

        pickStage = PickStage.FoundChess;
        pickIngChess = _chessBoard.board[result];
        _chessBoard.board[result].FindPossibleMove();
        _chessBoard.board[result].GotPick();
    }

    private void PutChess(Vector2Int result)
    {
        bool chessCanGo = pickIngChess.possibleMoveList.Contains(result);
        bool posHaveChess = _chessBoard.board.ContainsKey(result);

        _chessBoard.ReSetCanGo();
        if (!chessCanGo)
        {
            pickIngChess.ReturnPick();
            pickIngChess = null;
            pickStage = PickStage.None;
            return;
        }

        if (chessCanGo && !posHaveChess)
        {
            pickIngChess.Move(result);
        }
        else if (chessCanGo && posHaveChess)
        {
            _chessBoard.board[result].GotEaten();
            pickIngChess.Move(result);
        }

        pickIngChess = null;
        pickStage = PickStage.None;

    }


    public void InPutSystem_Update()
    {
        if (!nowUsingMouse.rightButton.wasPressedThisFrame || InGame.Instance.inGameStage != InGameStage.TurnStart)
            return;
        Vector2 mouse = nowUsingMouse.position.ReadValue();
        Vector3 mouseWorldPos;
        // Camera 射线
        Ray ray = Camera.main.ScreenPointToRay(mouse);
        // 棋盘平面 (y = 0)
        Plane boardPlane = new Plane(Vector3.up, Vector3.zero);
        // Ray 与 Plane 相交
        if (boardPlane.Raycast(ray, out float enter)) mouseWorldPos = ray.GetPoint(enter);
        else return;

        Vector2Int target = FindPos(mouseWorldPos);

        switch (pickStage)
        {
            case PickStage.None:
                PickChess(target);
                break;
            case PickStage.FoundChess:
                Debug.Log("ddd");
                PutChess(target);
                break;
        }
    }

}
