using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;

/// <summary>　駒の色定義　</summary>
public enum ChessColor
{
    White,
    Black
}

/// <summary>　チェス駒の種類定義　</summary>
public enum ChessType
{
    King,
    Queen,
    Rook,
    Bishop,
    Knight,
    Pawn
}

/// <summary>
/// 全チェス駒共通の基底クラス
/// 駒情報・座標・移動処理・撃破処理を管理
/// </summary>
public abstract class ChessBasic : MonoBehaviour
{
    /// <summary>　ChessBoard シングルトン参照　</summary>
    public ChessBoard _chessBoard => ChessBoard.Instance;
    public PoolObject poolObject => this.gameObject.GetComponent<PoolObject>();
    private ResourcesData _resourcesData => GameManager.Instance.resourcesData;


    /// <summary>　駒の色 </summary>
    public ChessColor color;
    public MeshRenderer _meshRenderer;
    public void ChangeChessColor(ChessColor chessColor)
    {
        color = chessColor;
        _meshRenderer.material= _resourcesData.TargetColor(color);

    }


    /// <summary>　駒タイプ　派生クラス側で定義　</summary>
    public abstract ChessType type { get;}
    /// <summary>　駒の色と種類情報　</summary>
    public Pair<ChessColor, ChessType> chessInfo => new Pair<ChessColor, ChessType>(color, type);
    /// <summary>　現在の盤面座標　</summary>
    public Vector2Int position { get; private set; } = new Vector2Int(-1,-1);
    public bool IsOutOfBoard(Vector2Int position) =>_chessBoard.IsOutOfBoard(position);

    /// <summary>　駒座標更新　</summary>
    public void SetPosition(Vector2Int pos) => position = pos;
    /// <summary>　 駒名取得　</summary>
    public virtual string ChessName() { return "ChessBasic"; }

    public abstract HashSet<Vector2Int> directions { get;}


    public Player _player;
    public virtual void ChessInit(Player player)
    {
        _player = player;
        ChangeChessColor(_player.usingChess);
    }

    public bool haveBuffed = false;



    public bool haveExtraLife {  get; private set; } = false;
    public void GotExtraLife(bool isHaveExtraLife)
    {
        if(type == ChessType.Rook)
        {
            haveExtraLife = false;
            return;
        }
        haveExtraLife = isHaveExtraLife;

    }

    public bool gotCurse { get; private set; } = false;
    public void CurseThisChess() =>gotCurse = true;
    public void PurifyThisChess()=> gotCurse = false;


    /// <summary>　 移動可能マス一覧　</summary>
    public HashSet<Vector2Int> possibleMoveList = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> possibleEatList = new HashSet<Vector2Int>();

    public abstract int findRange { get;}

    /// <summary>　移動可能位置探索 派生クラスでオーバーライドして使用 </summary>
    public virtual void FindCanMove(bool isThrougt)
    {
        foreach (var dir in directions)
        {
            for (int i = 1; i <= findRange; i++)
            {
                Vector2Int targetPos = position + dir * i;

                if (IsOutOfBoard(targetPos)) break;

                if (_chessBoard.board.TryGetValue(targetPos, out ChessBasic chess))
                {
                    if (chess.color != this.color)
                    {
                        possibleMoveList.Add(targetPos);
                        possibleEatList.Add(targetPos);
                    }
                    if (!isThrougt) break;
                }
                else
                {
                    possibleMoveList.Add(targetPos);
                }
            }
        }

    }
    public virtual void ExtraFindPossibleMove(bool isThrougt) { }
    public virtual void FindPossibleMove()
    {
        possibleMoveList.Clear();
        possibleEatList.Clear();

        FindCanMove(false);
        ExtraFindPossibleMove(false);

        _chessBoard.ShowActive(ChessBlockStage.CanGo, type, possibleMoveList);
        _chessBoard.ShowActive(ChessBlockStage.CanEat, type, possibleEatList);

    }
    public HashSet<Vector2Int> PossibleMove(bool isThrougt)
    {
        possibleMoveList.Clear();

        FindCanMove(isThrougt);
        ExtraFindPossibleMove(isThrougt);

        return possibleMoveList;
    }

    public void SwapPosition(ChessBasic swapChess)
    {
        if (swapChess.color != this.color) return;
        _chessBoard.Swap(this, swapChess);
    }

    private Vector3 pickAngle
    {
        get
        {
            float angleX = color==ChessColor.White?-15.0f:15.0f;
            return new Vector3 (0,0,0);
        }
    }
    private Vector3 pickPosition => new Vector3(transform.position.x, 5, transform.position.z);

    public virtual void GotPick()
    {
        transform.position = pickPosition;
        transform.rotation = Quaternion.Euler(pickAngle);
    }
    public virtual void ReturnPick()
    {
        transform.position = _chessBoard.ReturnChessBlockPosition(position);
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    public virtual bool CanEatChess(ChessBasic chess)
    {
        if (chess.haveExtraLife)
        {
            chess.GotExtraLife(false);
            return false;
        }
       
        return true;
    }
    public bool CanMoveTo(Vector2Int moveTo, out ChessBasic chess)
    {
        bool posHaveChess = _chessBoard.board.TryGetValue(moveTo, out chess);
        if(!posHaveChess) return true;
        return CanEatChess(chess);
    }
    public virtual void EatChess(ChessBasic chess)
    {
        if (chess == null) return;
        _player.nowPlayerStage = PlayerStage.EatingChess;
        _chessBoard.DeadEffect(chess);
        _chessBoard.Swap(this, chess);
        chess.GotEaten();
    }

    public void MoveOnly(Vector2Int moveTo)
    {
        _player.nowPlayerStage = PlayerStage.MovingChess;
        ReturnPick();
        if (!CanMoveTo(moveTo, out ChessBasic chess))
        {
            _player.nowPlayerStage = PlayerStage.ReadytoEnd;
            return;
        }
        if (chess == null) _chessBoard.MoveTo(this, moveTo);
        else EatChess(chess);

        if (_player.IsProTectedByRook_Guardian(position))
        {
            haveExtraLife = true;
        }
        else haveExtraLife = false;
    }

    /// <summary>
    /// 駒移動処理
    /// </summary>
    public virtual void Move(Vector2Int moveTo)
    {
        MoveOnly(moveTo);
        _player.nowPlayerStage = PlayerStage.ReadytoEnd;
    }

    /// <summary>
    /// 駒撃破処理
    /// </summary>
    public virtual void GotEaten()
    {
        // 盤面から削除
        _chessBoard.GotEat(this);
        if (poolObject != null) poolObject.pool.Return(this.gameObject);
        else Debug.LogError("Not In Pool");
    }

}
