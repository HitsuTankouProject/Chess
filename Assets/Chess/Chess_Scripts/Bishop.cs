using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ビショップへ Sorcerer の能力を付与するバフです。
/// レベルに応じて縦方向の追加移動範囲を開放し、最大レベルでは
/// ビショップの移動後に敵のキング以外の駒をランダムに呪います。
/// </summary>
public class Sorcerer : BuffBasic
{
    /// <summary>このバフの対象となる駒種を取得します。</summary>
    public override ChessType buffChess => ChessType.Bishop;
    /// <summary>バフの表示名を取得します。</summary>
    public override string buffName => "Sorcerer";
    /// <summary>プレイヤーが使用するビショップバフを Sorcerer に設定します。</summary>
    public override void Choose() => _player.allTheBuff.bishopBuffType = BishopBuff.Sorcerer;

    /// <summary>通常の斜め移動に追加される移動方向です。</summary>
    public HashSet<Vector2Int> extraCanGoArea = new HashSet<Vector2Int>();
    /// <summary>レベル1で開放される上方向です。</summary>
    private readonly Vector2Int firstExtraDirections = Vector2Int.up;
    /// <summary>レベル2で開放される下方向です。</summary>
    private readonly Vector2Int secondExtraDirections = Vector2Int.down;
    /// <summary>追加方向へ移動できる最大マス数です。</summary>
    public readonly int extraCanGoRange = 2;

    /// <summary>敵の駒を呪う能力が使用可能かどうかを示します。</summary>
    public bool canCurseChess;
    /// <summary>呪いの対象候補を所有する敵プレイヤーです。</summary>
    private Player _enemy;

    /// <summary>追加移動範囲と呪い能力を初期状態へ戻します。</summary>
    public override void ResetBuff()
    {
        extraCanGoArea.Clear();
        canCurseChess = false;
    }

    /// <summary>上方向への追加移動を開放します。</summary>
    public override void FirstLevel()
    {
        extraCanGoArea.Add(firstExtraDirections);
    }
    /// <summary>下方向への追加移動を開放します。</summary>
    public override void SecondLevel()
    {
        extraCanGoArea.Add(secondExtraDirections);

    }
    /// <summary>呪い能力を開放し、敵プレイヤーへの参照を設定します。</summary>
    public override void ThirdLevel()
    {
        canCurseChess = true;
        // 自分とは反対の駒色から敵プレイヤーを特定します。
        ChessColor othersChessColor = _player.usingChess == ChessColor.White ? ChessColor.Black : ChessColor.White;

        _enemy = GameManager.Instance.TargetPlayer(othersChessColor);
    }
    /// <summary>敵のキング以外の駒からランダムに1体を選んで呪います。</summary>
    public void CurseChess()
    {
        if (!canCurseChess) return;

        if (_enemy == null)
        {
            Debug.LogError("[Sorcerer] _enemy == null");
            return;
        }

        // キングを除外し、呪いを付与できる敵駒の候補を収集します。
        List<ChessBasic> canCurseChessList = new List<ChessBasic>();
        foreach (ChessBasic chess in _enemy.allTheChess.Values)
        {
            if(chess.type == ChessType.King) continue;
            canCurseChessList.Add(chess);
        }
        // 候補から1体をランダムに選択して呪いを付与します。
        int randomIndex = Random.Range(0, canCurseChessList.Count);
        canCurseChessList[randomIndex].CurseThisChess();
    }

}

/// <summary>
/// ビショップへ Monk の能力を付与するバフです。
/// レベルに応じて横方向の追加移動範囲を開放し、最大レベルでは
/// ビショップの移動範囲内にいる味方の呪いを浄化します。
/// </summary>
public class Monk : BuffBasic
{
    /// <summary>このバフの対象となる駒種を取得します。</summary>
    public override ChessType buffChess => ChessType.Bishop;
    /// <summary>バフの表示名を取得します。</summary>
    public override string buffName => "Monk";
    /// <summary>プレイヤーが使用するビショップバフを Monk に設定します。</summary>
    public override void Choose() => _player.allTheBuff.bishopBuffType = BishopBuff.Monk;
    /// <summary>通常の斜め移動に追加される移動方向です。</summary>
    public HashSet<Vector2Int> extraCanGoArea = new HashSet<Vector2Int>();
    /// <summary>レベル1で開放される左方向です。</summary>
    private readonly Vector2Int firstExtraDirections = Vector2Int.left;
    /// <summary>レベル2で開放される右方向です。</summary>
    private readonly Vector2Int secondExtraDirections = Vector2Int.right;
    /// <summary>追加方向へ移動できる最大マス数です。</summary>
    public readonly int extraCanGoRange = 2;
    /// <summary>味方の呪いを浄化する能力が使用可能かどうかを取得します。</summary>
    public bool canPurificChess { get; private set; } = false;
    /// <summary>追加移動範囲を初期状態へ戻します。</summary>
    public override void ResetBuff()
    {
        extraCanGoArea.Clear();
    }
    /// <summary>左方向への追加移動を開放します。</summary>
    public override void FirstLevel()
    {
        extraCanGoArea.Add(firstExtraDirections);
    }
    /// <summary>右方向への追加移動を開放します。</summary>
    public override void SecondLevel()
    {
        extraCanGoArea.Add(secondExtraDirections);

    }
    /// <summary>味方の呪いを浄化する能力を開放します。</summary>
    public override void ThirdLevel()
    {
        canPurificChess = true;
    }
    /// <summary>指定された味方駒のうち、呪われている駒を浄化します。</summary>
    /// <param name="purificChesses">浄化候補となる味方駒の集合です。</param>
    public void PurificChess(HashSet<ChessBasic> purificChesses)
    {
        if (!canPurificChess || purificChesses.Count == 0) return;
        foreach(ChessBasic chess in purificChesses)
        {
            if (chess.gotCurse) chess.PurifyThisChess();
        }
    }


}
/// <summary>
/// 斜め方向へ移動するビショップの駒を管理します。
/// 選択された Sorcerer または Monk バフに応じて追加移動範囲を計算し、
/// 移動後に敵への呪い、または味方への浄化効果を発動します。
/// </summary>
public class Bishop : ChessBasic
{
    /// <summary>この駒の種類を取得します。</summary>
    public override ChessType type => ChessType.Bishop;
    /// <summary>駒の表示名を取得します。</summary>
    /// <returns>ビショップを示す文字列を返します。</returns>
    public override string ChessName() { return "Bishop"; }
    /// <summary>移動可能範囲を探索する最大距離を取得します。</summary>
    public override int findRange { get;} = 8;
    /// <summary>ビショップが通常移動できる4つの斜め方向を取得します。</summary>
    public override HashSet<Vector2Int> directions => new HashSet<Vector2Int>()
    {
         new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
    };
    /// <summary>選択中のビショップバフによる追加移動候補を検索します。</summary>
    /// <param name="isThrougt">他の駒を越えて探索を続ける場合は <see langword="true" /> です。</param>
    public override void ExtraFindPossibleMove(bool isThrougt)
    {
        if (_player.bishopBuffType == BishopBuff.None) return;
        HashSet<Vector2Int> extraCanGoArea = new HashSet<Vector2Int>();
        int extraCanGoRange;

        // 選択中のバフから追加方向と最大距離を取得します。
        if (_player.bishopBuffType == BishopBuff.Sorcerer)
        {
            extraCanGoArea = _player.sorcerer.extraCanGoArea;
            extraCanGoRange = _player.sorcerer.extraCanGoRange;
        }
        else
        {
            extraCanGoArea = _player.monk.extraCanGoArea;
            extraCanGoRange = _player.monk.extraCanGoRange;
        }

        // 各追加方向について、盤外または他の駒に到達するまで探索します。
        foreach (Vector2Int dir in extraCanGoArea)
        {
            for (int i = 1; i <= extraCanGoRange; i++)
            {
                Vector2Int targetPos = position + dir * i;

                if (_chessBoard.IsOutOfBoard(targetPos)) break;

                if (_chessBoard.board.TryGetValue(targetPos, out ChessBasic chess))
                {
                    if (chess.color != this.color)
                    {
                        possibleMoveList.Add(targetPos);
                        possibleMoveList.Add(targetPos);
                    }
                    if(!isThrougt) break;
                }
                else
                {
                    possibleMoveList.Add(targetPos);
                }
            }
        }
    }
    /// <summary>
    /// この駒に呪いを適用します。Sorcerer 選択中は、代わりに現在の呪いを浄化します。
    /// </summary>
    public override void CurseThisChess()
    {
        if (_player.bishopBuffType == BishopBuff.Sorcerer) PurifyThisChess();
        else base.CurseThisChess();
    }

    /// <summary>
    /// 移動範囲内にいる、呪われた味方駒を収集して Monk の浄化を適用します。
    /// </summary>
    public void PurificChess()
    {
        HashSet<ChessBasic> purificChesses = new HashSet<ChessBasic>();
        // 駒を越えて探索し、浄化できる範囲を取得します。
        HashSet<Vector2Int> findPurific = PossibleMove(true);

        foreach (Vector2Int targetPos in findPurific)
        {
            if (_chessBoard.board.TryGetValue(targetPos, out ChessBasic chess))
            {
                Debug.Log(chess.chessInfo.first.ToString());
                Debug.Log(chess.chessInfo.second.ToString());
                // 自分と同じ色で、呪われている駒だけを浄化対象にします。
                if (chess.color == color && chess.gotCurse)
                {
                    Debug.Log(targetPos);
                    purificChesses.Add(chess);
                }
            }
        }

        _player.monk.PurificChess(purificChesses);
    }
    /// <summary>
    /// 指定位置へ移動し、選択中のビショップバフの追加効果を発動します。
    /// </summary>
    /// <param name="moveTo">移動先の盤面座標です。</param>
    public override void Move(Vector2Int moveTo)
    {
        base.Move(moveTo);
        // 移動完了後、選択中のバフに対応する固有効果を実行します。
        switch (_player.bishopBuffType)
        {
            case BishopBuff.None: return;
            case BishopBuff.Sorcerer:_player.sorcerer.CurseChess();return;
            case BishopBuff.Monk:PurificChess();return;
        }
    }


}
