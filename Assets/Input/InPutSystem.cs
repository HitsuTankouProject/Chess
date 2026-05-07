using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public enum MouseStage
{
    None,
    FoundChess
}
public class InPutSystem : MonoBehaviour
{
    private ChessBoard _chessBoard => ChessBoard.Instance;
    private InGame  _inGame => InGame.Instance;

    private InPutDevice inPutDevice = new InPutDevice();
    private MouseStage mouseStage = MouseStage.None;
    [SerializeField] private ChessBasic pickIngChess;

    public Vector2Int FindPos(Vector3 targetClick) 
    {
        Vector2Int result;
        Debug.Log(targetClick);

        Ray ray = new Ray( targetClick + Vector3.up * 10f, Vector3.down );
        RaycastHit hit;

        if (Physics.Raycast( ray, out hit, 100f, LayerMask.GetMask("GameBoard"), QueryTriggerInteraction.Collide))
        {
            Debug.Log(hit.collider.gameObject.name);

            string[] split = hit.collider.gameObject.name.Split('_');
            result = new Vector2Int(int.Parse(split[1]), int.Parse(split[2]));

            Debug.Log(result);
            return result;

        }
        else return new Vector2Int(-1, -1);
    }

    public void PickChess(Vector2Int result)
    {
        bool posHaveChess = _chessBoard.board.ContainsKey(result);
        if (!posHaveChess) return;

        bool sameColor = _chessBoard.board[result].color == _inGame.nowTurn;
        if (!sameColor) return;

        mouseStage = MouseStage.FoundChess;
        _chessBoard.board[result].FindPossibleMove();
        pickIngChess = _chessBoard.board[result];
    }

    public void PutChess(Vector2Int result)
    {
        bool chessCanGo = pickIngChess.possibleMoveList.Contains(result);
        bool posHaveChess = _chessBoard.board.ContainsKey(result);

        _chessBoard.ReSetCanGo();

        if (chessCanGo && !posHaveChess)
        {
            pickIngChess.Move(result);

            _inGame.TurnChange();
        }
        else if (chessCanGo && posHaveChess)
        {
            _chessBoard.board[result].GotEaten();
            pickIngChess.Move(result);
            _inGame.TurnChange();
        }

        pickIngChess = null;
        mouseStage = MouseStage.None;

    }
    public void InPutSystem_Update()
    {
        if (!inPutDevice.mouse.rightButton.wasPressedThisFrame)
            return;
        Vector2 mouse = inPutDevice.mouse.position.ReadValue();
        Vector3 mouseWorldPos;
        // Camera 射线
        Ray ray = Camera.main.ScreenPointToRay(mouse);
        // 棋盘平面 (y = 0)
        Plane boardPlane = new Plane(Vector3.up, Vector3.zero);
        // Ray 与 Plane 相交
        if (boardPlane.Raycast(ray, out float enter)) mouseWorldPos = ray.GetPoint(enter);
        else return;
        
        Vector2Int target = FindPos(mouseWorldPos);

        switch (mouseStage)
        {
            case MouseStage.None:
                PickChess(target);
                break;  
            case MouseStage.FoundChess:
                Debug.Log("ddd");
                PutChess(target);
                break;
        }
    }

    private void Update()
    {
        InPutSystem_Update();
    }
}
