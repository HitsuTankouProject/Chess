using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private PoolObject poolObject => this.gameObject.GetComponent< PoolObject>();
    /// <summary>　駒の色 </summary>
    public ChessColor color;
    public Material m_Black;
    public Material m_White;


    /// <summary>　駒タイプ　派生クラス側で定義　</summary>
    public abstract ChessType type { get;}
    /// <summary>　駒の色と種類情報　</summary>
    public Pair<ChessColor, ChessType> chessInfo => new Pair<ChessColor, ChessType>(color, type);
    /// <summary>　現在の盤面座標　</summary>
    public Vector2Int position { get; private set; } = new Vector2Int(-1,-1);
    /// <summary>　駒座標更新　</summary>
    public void SetPosition(Vector2Int pos) => position = pos;
    /// <summary>　 駒名取得　</summary>
    public virtual string ChessName() { return "ChessBasic"; }

    public Player _player;
    public virtual void ChessInit(Player player)
    {
        _player = player;
    }

    public bool haveBuffed = false;
    public bool haveExtraLife = false;

    public bool gotCurse{get;private set;} = false;
    public void CurseThisChess()=>gotCurse = true;
    public void PurifyThisChess()
    {
        gotCurse = false;
    }

    /// <summary>　 移動可能マス一覧　</summary>
    public HashSet<Vector2Int> possibleMoveList = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> canEatChessPosition = new HashSet<Vector2Int>();
    public abstract int findRange { get; protected set; }

    /// <summary>　移動可能位置探索 派生クラスでオーバーライドして使用 </summary>
    public virtual void FindPossibleMove() { }

    /// <summary>
    /// 駒移動処理
    /// </summary>
    public virtual void Move(Vector2Int moveTo) 
    {
        if(gotCurse)PurifyThisChess();
        // ワールド座標へ移動
        this.transform.position = _chessBoard.ReturnChessBlockPosition(moveTo);
        // 盤面情報更新
        _chessBoard.BoardUpdate(this, moveTo, ChessAction.Move);
    }

    /// <summary>
    /// 駒撃破処理
    /// </summary>
    public virtual void GotEaten()
    {
        // 盤面から削除
        _chessBoard.BoardUpdate(this, this.position, ChessAction.GotEat);
        if (poolObject != null) poolObject.pool.Return(this.gameObject);
        else Debug.LogError("Not In Pool");
    }





}
