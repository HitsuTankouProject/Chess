using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ポーンへ Scout の能力を付与するバフです。
/// レベルに応じて昇格の禁止、捕獲した駒の移動方向の継承、
/// 継承した方向へ移動できる距離の拡張を行います。
/// </summary>
public class Scout : BuffBasic
{
    /// <summary>このバフの対象となる駒種を取得します。</summary>
    public override ChessType buffChess => ChessType.Pawn;
    /// <summary>バフの表示名を取得します。</summary>
    public override string buffName => "Scout";
    /// <summary>プレイヤーが使用するポーンバフを Scout に設定します。</summary>
    public override void Choose() => _player.allTheBuff.pawnBuffType = PawnBuff.Scout;
    /// <summary>ポーンの昇格を禁止する効果が有効かどうかを示します。</summary>
    public bool cantPromotion = false;
    /// <summary>捕獲した駒の移動方向を継承できるかどうかを示します。</summary>
    public bool canReceiveMoveAreaFromYouAteChess = false;
    /// <summary>捕獲した駒から継承した追加移動方向です。</summary>
    public HashSet<Vector2Int> extraMoveArea = new HashSet<Vector2Int>();
    /// <summary>捕獲した駒から継承した追加移動方向です。</summary>
    public int extraMoveRange { get; private set; } = 1;
    /// <summary>Scout 固有の能力と継承済み移動方向を初期状態へ戻します。</summary>
    public override void ResetBuff()
    {
        cantPromotion = false;
        canReceiveMoveAreaFromYouAteChess = false;
        extraMoveArea.Clear();
        extraMoveRange = 1;
    }
    /// <summary>ポーンの昇格を禁止します。</summary>
    public override void FirstLevel()
    {
        cantPromotion = true;
    }
    /// <summary>捕獲した駒の移動方向を継承する能力を開放します。</summary>
    public override void SecondLevel()
    {
        canReceiveMoveAreaFromYouAteChess = true;
    }
    /// <summary>継承した方向へ移動できる距離を3マスへ拡張します。</summary>
    public override void ThirdLevel()
    {
        extraMoveRange = 3;
    }
    /// <summary>捕獲した駒の移動方向を Scout の追加移動方向へ登録します。</summary>
    /// <param name="capturedChess">移動方向を継承する捕獲済みの駒です。</param>
    public void AddExtraMoveArea(ChessBasic capturedChess)
    {
        foreach (Vector2Int moveDirection in capturedChess.directions)
        {
            // 既に継承済みの方向は重複して追加しません。
            if (!extraMoveArea.Contains(moveDirection))
            {
                extraMoveArea.Add(moveDirection);
            }
        }
    }

}
/// <summary>
/// ポーンへ Substitute の能力を付与するバフです。
/// レベルに応じて昇格の禁止、敵駒を捕獲しない全方向移動、
/// 味方キングが捕獲される際の身代わり効果を開放します。
/// </summary>
public class Substitute : BuffBasic
{
    /// <summary>このバフの対象となる駒種を取得します。</summary>
    public override ChessType buffChess => ChessType.Pawn;
    /// <summary>バフの表示名を取得します。</summary>
    public override string buffName => "Substitute";
    /// <summary>プレイヤーが使用するポーンバフを Substitute に設定します。</summary>
    public override void Choose() => _player.allTheBuff.pawnBuffType = PawnBuff.Substitute;
    /// <summary>Substitute が移動候補として探索する縦、横、斜めの8方向を取得します。</summary>
    public HashSet<Vector2Int> extraMoveArea => new()
    {
        Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right,
        new Vector2Int(1, 1),new Vector2Int(1, -1),new Vector2Int(-1, 1),new Vector2Int(-1, -1)
    };
    /// <summary>ポーンの昇格を禁止する効果が有効かどうかを示します。</summary>
    public bool cantPromotion = false;
    /// <summary>ポーンが敵駒を捕獲できない状態かどうかを示します。</summary>
    public bool cantKill = false;
    /// <summary>ポーンが存在する間、キングの身代わりになれるかどうかを示します。</summary>
    public bool cantKillKingWhenPawnExist = false;
    /// <summary>Substitute 固有の能力をすべて無効化します。</summary>
    public override void ResetBuff()
    {
        cantPromotion = false;
        cantKill = false;
        cantKillKingWhenPawnExist = false;
    }
    /// <summary>ポーンの昇格を禁止します。</summary>
    public override void FirstLevel()
    {
        cantPromotion = true;
    }
    /// <summary>敵駒を捕獲せずに全方向へ移動する能力を開放します。</summary>
    public override void SecondLevel()
    {
        cantKill = true;
    }
    /// <summary>味方キングが捕獲される際の身代わり能力を開放します。</summary>
    public override void ThirdLevel()
    {
        cantKillKingWhenPawnExist = true;
    }
}

/// <summary>
/// 前方移動と斜め捕獲を行うポーンの駒を管理します。
/// 初回移動時の2マス移動、Scout または Substitute による追加移動、
/// Scout の移動方向継承、最終列へ到達した際のランダム昇格を処理します。
/// </summary>
public class Pawn : ChessBasic
{
    /// <summary>この駒の種類を取得します。</summary>
    public override ChessType type => ChessType.Pawn;
    /// <summary>このポーンがまだ一度も移動していないかどうかを示します。</summary>
    private bool isFirstMove = true;
    /// <summary>駒の表示名を取得します。</summary>
    /// <returns>ポーンを示す文字列を返します。</returns>
    public override string ChessName() { return "Pawn"; }
    /// <summary>前方へ探索する最大距離を取得します。初回のみ2マス、それ以降は1マスです。</summary>
    public override int findRange => isFirstMove ? 2 : 1;
    /// <summary>白側を基準とした通常の前進方向を取得します。</summary>
    public override HashSet<Vector2Int> directions => new HashSet<Vector2Int>() { Vector2Int.up };
    /// <summary>ポーンが昇格できる駒種の集合です。</summary>
    public HashSet<ChessType> canPromotionChessType = new HashSet<ChessType>() { ChessType.Queen, ChessType.Rook, ChessType.Bishop, ChessType.Knight };
    /// <summary>白側を基準とした斜め前方の捕獲方向です。</summary>
    private List<Vector2Int> attackDirs = new List<Vector2Int>
    { new Vector2Int(1, 1), new Vector2Int(-1, 1) };
    /// <summary>所有者と駒色を設定し、初回移動状態へ戻します。</summary>
    /// <param name="player">このポーンを所有するプレイヤーです。</param>
    public override void ChessInit(Player player)
    {
        base.ChessInit(player);
        isFirstMove = true;
    }
    /// <summary>Scout が継承した方向に基づいて追加移動・捕獲候補を計算します。</summary>
    private void Scout_ExtraFindPossibleMove()
    {
        foreach (Vector2Int direction in _player.scout.extraMoveArea)
        {
            for (int i = 1; i <= _player.scout.extraMoveRange; i++)
            {
                Vector2Int targetPosition = position + direction * i;

                if (IsOutOfBoard(targetPosition)) break;

                bool haveChess = _chessBoard.board.TryGetValue(targetPosition, out ChessBasic chess);
                if (!haveChess)
                {
                    possibleMoveList.Add(targetPosition);
                    continue;
                }
                // 最初に見つかった敵駒を移動・捕獲候補へ追加します。
                if (chess.color != this.color)
                {
                    possibleMoveList.Add(targetPosition);
                    possibleEatList.Add(targetPosition);
                }

                break;
            }
        }
    }
    /// <summary>Substitute の8方向について、駒に遮られるまで空きマスを移動候補へ追加します。</summary>
    private void Substitute_ExtraFindPossibleMove()
    {
        foreach (Vector2Int direction in _player.substitute.extraMoveArea)
        {
            for (int i = 1; i <= _player.scout.extraMoveRange; i++)
            {
                Vector2Int targetPosition = position + direction * i;

                if (IsOutOfBoard(targetPosition)) break;

                bool haveChess = _chessBoard.board.TryGetValue(targetPosition, out ChessBasic chess);
                if (!haveChess)
                {
                    possibleMoveList.Add(targetPosition);
                    continue;
                }
                break;
            }
        }
    }
    /// <summary>選択中のポーンバフに対応する追加移動候補を計算します。</summary>
    /// <param name="isThrough">基底クラスとの互換性のために受け取る貫通探索フラグです。</param>
    public override void ExtraFindPossibleMove(bool isThrough)
    {
        if (_player.scout.extraMoveArea.Count != 0)
        {
            Scout_ExtraFindPossibleMove();
            return;
        }
        else if (_player.substitute.cantKill) Substitute_ExtraFindPossibleMove();

    }
    /// <summary>駒色に応じた斜め前方から敵駒の捕獲候補を検索します。</summary>
    private void FindCanEat()
    {
        int moveDirectionValue = (color == ChessColor.White) ? 1 : -1;
        foreach (Vector2Int attackDirection in attackDirs)
        {
            Vector2Int targetPosition = position + attackDirection * moveDirectionValue;

            if (IsOutOfBoard(targetPosition)) break;

            bool haveChess = _chessBoard.board.TryGetValue(targetPosition, out ChessBasic targetChess);
            if (!haveChess) continue;

            if (targetChess.color != color)
            {
                possibleMoveList.Add(targetPosition);
                possibleEatList.Add(targetPosition);
            }
        }
    }
    /// <summary>前方の空きマスと斜め前方の捕獲候補を計算します。</summary>
    /// <param name="isThrough">他の駒を越えて探索を続ける場合は <see langword="true" /> です。</param>
    public override void FindCanMove(bool isThrough)
    {
        // Substitute の非攻撃型移動が有効な場合、通常の前進と捕獲を行いません。
        if (_player.substitute.cantKill) return;

        int moveDirectionValue = (color == ChessColor.White) ? 1 : -1;
        foreach (Vector2Int moveDirection in directions)
        {
            Vector2Int moveOffset = moveDirection * moveDirectionValue;

            for (int distance = 1; distance <= findRange; distance++)
            {
                Vector2Int targetPosition = position + moveOffset * distance;

                if (IsOutOfBoard(targetPosition)) break;

                bool haveChess = _chessBoard.board.ContainsKey(targetPosition);

                if (!haveChess) possibleMoveList.Add(targetPosition);
                if (!isThrough && haveChess) break;
            }
        }
        FindCanEat();

    }
    /// <summary>Scout のレベル2効果により、捕獲した駒の移動方向を継承します。</summary>
    /// <param name="chess">今回捕獲する駒です。</param>
    private void ScoutSecondBuff(ChessBasic chess)
    {
        if (_player.pawnBuffType != PawnBuff.Scout || !_player.scout.canReceiveMoveAreaFromYouAteChess) return;
        _player.scout.AddExtraMoveArea(chess);
    }
    /// <summary>敵駒の移動方向を必要に応じて継承してから捕獲します。</summary>
    /// <param name="chess">捕獲する敵駒です。</param>
    public override void EatChess(ChessBasic chess)
    {
        ScoutSecondBuff(chess);
        base.EatChess (chess);
    }
    /// <summary>指定座標へ移動し、昇格判定後にプレイヤーの手番を終了します。</summary>
    /// <param name="moveTo">移動先の盤面座標です。</param>
    public override void Move(Vector2Int moveTo)
    {
        if (isFirstMove) isFirstMove = false;
        MoveOnly(moveTo);
        Promotion();
        _player.Player_TurnEnd();

    }
    /// <summary>ランダム昇格の候補となる駒種です。</summary>
    private readonly List<ChessType> canPromotionChessTypes = new List<ChessType>()
    {
        ChessType.Queen,  ChessType.Bishop,
        ChessType.Rook,   ChessType.Knight,
    };
    /// <summary>バフ未選択のポーンが最終列へ到達した場合、候補からランダムに昇格させます。</summary>
    private void Promotion()
    {
        // ポーンバフを使用中の場合は昇格しません。
        if (_player.pawnBuffType != PawnBuff.None) return;
        int targetY = (color == ChessColor.White) ? 7 : 0;
        if (position.y != targetY) return;

        // 昇格先をランダムに選び、現在位置へ新しい駒を生成します。
        Pair<ChessColor, ChessType> promotionInfo = 
            new Pair<ChessColor, ChessType>(color, canPromotionChessTypes[Random.Range(0, canPromotionChessTypes.Count)]);
        _chessBoard.StartGenChessProcess(position, promotionInfo,_player);
        if (poolObject != null) poolObject.pool.Return(this.gameObject);
        else Debug.LogError("Not In Pool");
    }




}
