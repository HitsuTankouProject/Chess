
using System.Collections.Generic;
using UnityEngine;


/// <summary>チェス駒の所属色を表します。</summary>
public enum ChessColor
{
    /// <summary>白プレイヤーに所属する駒です。</summary>
    White,
    /// <summary>黒プレイヤーに所属する駒です。</summary>
    Black,

    /// <summary>どちらのプレイヤーにも所属しない状態です。</summary>
    None = -1
}

/// <summary>　チェス駒の種類定義　</summary>
public enum ChessType
{
    /// <summary>キングです。</summary>
    King,
    /// <summary>クイーンです。</summary>
    Queen,
    /// <summary>ルークです。</summary>
    Rook,
    /// <summary>ビショップです。</summary>
    Bishop,
    /// <summary>ナイトです。</summary>
    Knight,
    /// <summary>ポーンです。</summary>
    Pawn
}

/// <summary>
/// すべてのチェス駒に共通する基底クラスです。
/// 駒の色、種類、盤面座標、移動候補、捕獲候補、状態エフェクトを管理し、
/// 移動範囲の探索、駒の選択表示、移動、捕獲、プールへの返却を提供します。
/// 派生クラスは駒ごとの移動方向、探索距離、追加移動処理を定義します。
/// </summary>
public abstract class ChessBasic : MonoBehaviour
{
    /// <summary>盤面を管理する共有インスタンスを取得します。</summary>
    public ChessBoard _chessBoard => ChessBoard.Instance;
    /// <summary>この駒に設定されているプール管理コンポーネントを取得します。</summary>
    public PoolObject poolObject => this.gameObject.GetComponent<PoolObject>();
    /// <summary>ゲーム内で共有する素材やマテリアルを取得します。</summary>
    private ResourcesData _resourcesData => GameManager.Instance.resourcesData;
    /// <summary>この駒が所属するプレイヤーの色です。</summary>
    public ChessColor color;
    /// <summary>駒本体を描画するレンダラーです。</summary>
    public MeshRenderer _meshRenderer;
        /// <summary>追加ライフや呪いを表示するエフェクト用レンダラーです。</summary>
    public MeshRenderer effect;
    /// <summary>駒の所属色と表示マテリアルを変更します。</summary>
    /// <param name="chessColor">新しく設定する駒色です。</param>
    public void ChangeChessColor(ChessColor chessColor)
    {
        color = chessColor;
        _meshRenderer.material = _resourcesData.TargetColor(color);

    }


    /// <summary>派生クラスが表す駒の種類を取得します。</summary>
    public abstract ChessType type { get; }
    /// <summary>この駒の色と種類を組み合わせた情報を取得します。</summary>
    public Pair<ChessColor, ChessType> chessInfo => new Pair<ChessColor, ChessType>(color, type);
    /// <summary>現在の盤面座標を取得します。未配置時は (-1, -1) です。</summary>
    public Vector2Int position { get; private set; } = new Vector2Int(-1, -1);
    /// <summary>指定座標が盤面の外側か判定します。</summary>
    /// <param name="position">判定する盤面座標です。</param>
    /// <returns>盤面外の場合は <see langword="true" /> です。</returns
    public bool IsOutOfBoard(Vector2Int position) => _chessBoard.IsOutOfBoard(position);

    /// <summary>この駒の盤面座標を更新します。</summary>
    /// <param name="pos">新しい盤面座標です。</param>
    public void SetPosition(Vector2Int pos) => position = pos;
    /// <summary>駒の表示名を取得します。</summary>
    /// <returns>派生クラスで上書きされない場合は "ChessBasic" を返します。</returns>
    public virtual string ChessName() { return "ChessBasic"; }
    /// <summary>この駒が通常移動できる方向の集合を取得します。</summary>
    public abstract HashSet<Vector2Int> directions { get; }
    /// <summary>この駒を所有し、手番を管理するプレイヤーです。</summary>
    public Player _player;
    /// <summary>所有者を設定し、所有者の駒色をこの駒へ反映します。</summary>
    /// <param name="player">この駒を所有するプレイヤーです。</param>
    public virtual void ChessInit(Player player)
    {
        _player = player;
        ChangeChessColor(_player.usingChess);
    }
    /// <summary>捕獲を一度だけ無効化する追加ライフを持っているか取得します。</summary>
    public bool haveExtraLife { get; private set; } = false;
    /// <summary>追加ライフの状態と表示エフェクトを更新します。</summary>
    /// <param name="isHaveExtraLife">追加ライフを付与する場合は <see langword="true" /> です。</param>
    public void GotExtraLife(bool isHaveExtraLife)
    {
        // Guardian 自身であるルークには追加ライフを付与しません。
        if (type == ChessType.Rook)
        {
            haveExtraLife = false;
            return;
        }
        haveExtraLife = isHaveExtraLife;
        _chessBoard.IsGotExtraLife(this, isHaveExtraLife);
        effect.material = _resourcesData.allMaterial.m_ChessHaveExtraLife;
        effect.enabled = haveExtraLife;
    }
    /// <summary>この駒が呪われているか取得します。</summary>
    public bool gotCurse { get; private set; } = false;
    /// <summary>この駒へ呪いを付与し、呪いのエフェクトを表示します。</summary>
    public virtual void CurseThisChess()
    {
        gotCurse = true;
        effect.material = _resourcesData.allMaterial.m_GotCurse;
        effect.enabled = true;

    }
    /// <summary>この駒の呪いを解除し、盤面へ浄化エフェクトを通知します。</summary>
    public void PurifyThisChess()
    {
        if (!gotCurse) return;
        gotCurse = false;
        effect.enabled = false;
        _chessBoard.PurificEffect(this);
    }


    /// <summary>現在計算されている移動可能座標の集合です。</summary>
    public HashSet<Vector2Int> possibleMoveList = new HashSet<Vector2Int>();
    /// <summary>現在計算されている捕獲可能座標の集合です。</summary>
    public HashSet<Vector2Int> possibleEatList = new HashSet<Vector2Int>();
    /// <summary>各方向について探索する最大距離を取得します。</summary>
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
                    // 貫通探索では駒の有無にかかわらず、その先の座標も確認します。
                    if (isThrougt)
                    {
                        possibleMoveList.Add(targetPos);
                        if (chess.color != this.color)
                            possibleEatList.Add(targetPos);
                        continue;
                    }
                    // 通常探索では敵駒の座標を候補へ加え、そこで探索を終了します。
                    if (chess.color != this.color)
                    {
                        possibleMoveList.Add(targetPos);
                        possibleEatList.Add(targetPos);
                    }
                    break;
                }
                else
                {
                    possibleMoveList.Add(targetPos);
                }
            }
        }

    }
    /// <summary>バフなどによる駒固有の追加移動候補を計算します。</summary>
    /// <param name="isThrougt">他の駒を越えて探索を続ける場合は <see langword="true" /> です。</param>
    public virtual void ExtraFindPossibleMove(bool isThrougt) { }
    /// <summary>移動・捕獲候補を再計算し、対応する盤面表示を有効にします。</summary>
    public virtual void FindPossibleMove()
    {
        possibleMoveList.Clear();
        possibleEatList.Clear();

        FindCanMove(false);
        ExtraFindPossibleMove(false);

        _chessBoard.ShowActive(ChessBlockStage.CanGo, type, possibleMoveList);
        _chessBoard.ShowActive(ChessBlockStage.CanEat, type, possibleEatList);

    }
    /// <summary>移動可能座標を再計算して取得します。</summary>
    /// <param name="isThrougt">他の駒を越えて探索を続ける場合は <see langword="true" /> です。</param>
    /// <returns>計算された移動可能座標の集合です。</returns>
    public HashSet<Vector2Int> PossibleMove(bool isThrougt)
    {
        possibleMoveList.Clear();

        FindCanMove(isThrougt);
        ExtraFindPossibleMove(isThrougt);

        return possibleMoveList;
    }
    /// <summary>同じ色の指定駒と盤面上の位置を交換します。</summary>
    /// <param name="swapChess">位置を交換する味方駒です。</param>
    public void SwapPosition(ChessBasic swapChess)
    {
        if (swapChess.color != this.color) return;
        _chessBoard.Swap(this, swapChess);
    }
    /// <summary>駒を選択した際に適用する回転角度を取得します。</summary>
    private Vector3 pickAngle
    {
        get
        {
            float angleX = color==ChessColor.White?-15.0f:15.0f;
            return new Vector3 (0,0,0);
        }
    }
    /// <summary>駒を選択した際に表示するワールド座標を取得します。</summary>
    private Vector3 pickPosition => new Vector3(transform.position.x, 5, transform.position.z);
    /// <summary>選択中であることを示す位置と角度へ駒を移動します。</summary>
    public virtual void GotPick()
    {
        transform.position = pickPosition;
        transform.rotation = Quaternion.Euler(pickAngle);
    }
    /// <summary>選択表示を解除し、駒を現在の盤面座標へ戻します。</summary>
    public virtual void ReturnPick()
    {
        transform.position = _chessBoard.ReturnChessBlockPosition(position);
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    /// <summary>この駒を捕獲できるか判定します。</summary>
    /// <returns>捕獲できる場合は <see langword="true" /> です。</returns>
    public virtual bool CanBeEat()
    {
        // 追加ライフを消費した場合は、今回の捕獲を無効にします。
        if (haveExtraLife)
        {
            GotExtraLife(false);
            return false;
        }
        return true;
    }
    /// <summary>指定した敵駒を捕獲できるか判定します。</summary>
    /// <param name="chess">捕獲対象の駒です。</param>
    /// <returns>対象を捕獲できる場合は <see langword="true" /> です。</returns>
    public virtual bool CanEatChess(ChessBasic chess)
    {
        return chess.CanBeEat();
    }
    /// <summary>指定座標へ移動または捕獲できるか判定します。</summary>
    /// <param name="moveTo">移動先の盤面座標です。</param>
    /// <param name="chess">移動先に存在する駒です。空きマスの場合は <see langword="null" /> です。</param>
    /// <returns>移動を実行できる場合は <see langword="true" /> です。</returns>
    public bool CanMoveTo(Vector2Int moveTo, out ChessBasic chess)
    {
        bool posHaveChess = _chessBoard.board.TryGetValue(moveTo, out chess);
        if(!posHaveChess) return true;
        return CanEatChess(chess);
    }
    /// <summary>指定駒を捕獲し、この駒と対象の盤面位置を更新します。</summary>
    /// <param name="chess">捕獲する敵駒です。</param>
    public virtual void EatChess(ChessBasic chess)
    {
        if (chess == null) return;
        _chessBoard.DeadEffect(chess);
        _chessBoard.Swap(this, chess);
        chess.GotEaten();
    }
    /// <summary>手番終了を行わず、指定座標への移動または捕獲だけを実行します。</summary>
    /// <param name="moveTo">移動先の盤面座標です。</param>
    public void MoveOnly(Vector2Int moveTo)
    {
        ReturnPick();
        if (!CanMoveTo(moveTo, out ChessBasic chess))
        {
            _player.Player_TurnEnd();
            return;
        }

        if (chess == null) _chessBoard.MoveTo(this, moveTo);
        else EatChess(chess);

        // 移動前の呪いを解除し、呪われたマスへ移動した場合は改めて呪いを付与します。
        if (gotCurse) PurifyThisChess();
        if (_chessBoard.ChessBlock(moveTo).blockStage == BlockStage.GotCurse)
            CurseThisChess();

    }

    /// <summary>指定座標へ駒を移動し、プレイヤーの手番を終了します。</summary>
    /// <param name="moveTo">移動先の盤面座標です。</param>
    public virtual void Move(Vector2Int moveTo)
    {
        MoveOnly(moveTo);
        _player.Player_TurnEnd();
    }

    /// <summary>捕獲された駒を盤面から除去し、利用可能であればオブジェクトプールへ返します。</summary>
    public virtual void GotEaten()
    {
        // 盤面から削除
        _chessBoard.GotEat(this);
        if (poolObject != null) poolObject.pool.Return(this.gameObject);
        else Debug.LogError("Not In Pool");
    }

}
