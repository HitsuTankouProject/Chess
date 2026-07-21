using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Collections;
using static UnityEngine.Analytics.IAnalytic;

/// <summary>
/// ナイトへ Charger の能力を付与するバフです。
/// レベルに応じて縦方向の追加移動、追加移動距離の拡張、
/// 敵駒を捕獲した後の追加行動を開放します。
/// </summary>
public class Charger : BuffBasic
{
    /// <summary>このバフの対象となる駒種を取得します。</summary>
    public override ChessType buffChess => ChessType.Knight;
    /// <summary>バフの表示名を取得します。</summary>
    public override string buffName => "Charger";
    /// <summary>プレイヤーが使用するナイトバフを Charger に設定します。</summary>
    public override void Choose() => _player.allTheBuff.knightBuffType = KnightBuff.Charger;
    /// <summary>通常のナイト移動に追加される移動方向です。</summary>
    public HashSet<Vector2Int> extraCanGoArea = new HashSet<Vector2Int>();
    /// <summary>レベル1で開放される上下方向です。</summary>
    private readonly HashSet<Vector2Int> firstExtraArea = new HashSet<Vector2Int>()
    {
        new Vector2Int(0,1),new Vector2Int(0,-1),
    };
    /// <summary>追加方向へ移動できる最大マス数を取得します。</summary>
    public int extraCanGoRange { get; private set; } = 1;
    /// <summary>捕獲後にもう一度行動できるかどうかを取得します。</summary>
    public bool canMoveItAgain {  get; private set; } = false;
    /// <summary>追加移動距離と方向を初期状態へ戻します。</summary>
    public override void ResetBuff()
    {
        extraCanGoRange = 1;
        extraCanGoArea.Clear();
    }
    /// <summary>上下方向への追加移動を開放します。</summary>
    public override void FirstLevel()
    {
        extraCanGoArea.AddRange(firstExtraArea);
    }
    /// <summary>追加方向へ移動できる距離を3マスへ拡張します。</summary>
    public override void SecondLevel()
    {
        extraCanGoRange = 3;
    }
    /// <summary>敵駒を捕獲した後の追加行動を開放します。</summary>
    public override void ThirdLevel()
    {
        canMoveItAgain = true;
    }

}
/// <summary>
/// ナイトへ Skirmisher の能力を付与するバフです。
/// レベルに応じて横方向の追加移動、追加移動距離の拡張、
/// 移動後に左右の敵駒を捕獲する能力を開放します。
/// </summary>
public class Skirmisher : BuffBasic
{
    /// <summary>このバフの対象となる駒種を取得します。</summary>
    public override ChessType buffChess => ChessType.Knight;
    /// <summary>バフの表示名を取得します。</summary>
    public override string buffName => "Skirmisher";
    /// <summary>プレイヤーが使用するナイトバフを Skirmisher に設定します。</summary>
    public override void Choose() => _player.allTheBuff.knightBuffType = KnightBuff.Skirmisher;
    /// <summary>通常のナイト移動に追加される移動方向です。</summary>
    public HashSet<Vector2Int> extraCanGoArea = new HashSet<Vector2Int>();
    /// <summary>レベル1で開放される左右方向です。</summary>
    private readonly HashSet<Vector2Int> firstExtraArea = new HashSet<Vector2Int>()
    {
        new Vector2Int(1,0),new Vector2Int(-1,0),
    };
    /// <summary>追加方向へ移動できる最大マス数を取得します。</summary>
    public int extraCanGoRange { get; private set; } = 1;
    /// <summary>移動後に隣接する敵駒を捕獲できるかどうかを取得します。</summary>
    public bool canEatNextChess { get; private set; } = false;
    /// <summary>追加移動距離と方向を初期状態へ戻します。</summary>
    public override void ResetBuff()
    {
        extraCanGoRange = 1;
        extraCanGoArea.Clear();
    }
    /// <summary>左右方向への追加移動を開放します。</summary>
    public override void FirstLevel()
    {
        extraCanGoArea.AddRange(firstExtraArea);
    }
    /// <summary>追加方向へ移動できる距離を3マスへ拡張します。</summary>
    public override void SecondLevel()
    {
        extraCanGoRange = 3;
    }
    /// <summary>移動後に左右へ隣接する敵駒を捕獲する能力を開放します。</summary>
    public override void ThirdLevel()
    {
        canEatNextChess = true;
    }

}

/// <summary>
/// L字方向へ移動するナイトの駒を管理します。
/// 選択された Charger または Skirmisher バフに応じて追加移動範囲を計算し、
/// 移動後に追加行動、または隣接する敵駒の捕獲を処理します。
/// </summary>
public class Knight : ChessBasic
{
    /// <summary>この駒の種類を取得します。</summary>
    public override ChessType type => ChessType.Knight;
    /// <summary>駒の表示名を取得します。</summary>
    /// <returns>ナイトを示す文字列を返します。</returns>
    public override string ChessName() { return "Knight"; }
    /// <summary>Charger の追加行動をすでに使用したかどうかを示します。</summary>
    private bool isMoveAgain = false;
    /// <summary>各L字方向について探索する距離を取得します。</summary>
    public override int findRange { get; } = 1;
    /// <summary>ナイトが通常移動できる8つのL字方向を取得します。</summary>
    public override HashSet<Vector2Int> directions => new HashSet<Vector2Int>()
    {
        new Vector2Int(2, 1),
        new Vector2Int(2, -1),
        new Vector2Int(-2, 1),
        new Vector2Int(-2, -1),
        new Vector2Int(1, 2),
        new Vector2Int(1, -2),
        new Vector2Int(-1, 2),
        new Vector2Int(-1, -2)
    };
    /// <summary>通常のL字移動候補を計算します。</summary>
    /// <param name="isThrougt">他の駒を越えて探索する場合は <see langword="true" /> です。</param>
    public override void FindCanMove(bool isThrougt)
    {
        base.FindCanMove(isThrougt);
    }
    /// <summary>選択中のナイトバフによる追加移動・捕獲候補を計算します。</summary>
    /// <param name="isthrough">他の駒を越えて探索する場合は <see langword="true" /> です。</param>
    public override void ExtraFindPossibleMove(bool isthrough)
    {
        if (_player.knightBuffType == KnightBuff.None) return;

        // 選択中のバフから追加方向と最大距離を取得します。
        HashSet<Vector2Int> extraCanGoArea = _player.knightBuffType == KnightBuff.Charger ?
            _player.charger.extraCanGoArea : _player.skirmisher.extraCanGoArea;

        int extraCanGoRange = _player.knightBuffType == KnightBuff.Charger ?
            _player.charger.extraCanGoRange : _player.skirmisher.extraCanGoRange;

        foreach (var dir in extraCanGoArea)
        {
            for (int i = 1; i <= extraCanGoRange; i++)
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
                    if (!isthrough) break;
                }
                else
                {
                    possibleMoveList.Add(targetPos);
                }

            }


        }
    }
    /// <summary>指定座標へ移動し、選択中のナイトバフの最終効果を発動します。</summary>
    /// <param name="moveTo">移動先の盤面座標です。</param>
    public override void Move(Vector2Int moveTo)
    {
        KnightBuff knightBuff = _player.knightBuffType;

        // バフが未選択の場合は通常の移動処理を使用します。
        if (knightBuff == KnightBuff.None)
        {
            base.Move(moveTo);
            return;
        }

        bool canMove = CanMoveTo(moveTo, out ChessBasic posHaveChess);
        bool isEatTheChess = posHaveChess != null;
        if (!canMove)
        {
            base.Move(moveTo);
            return;
        }

        MoveOnly(moveTo);

        if (knightBuff == KnightBuff.Skirmisher) SkirmisherFinalBuff();
        else if (knightBuff == KnightBuff.Charger) ChargerFinalBuff(isEatTheChess);
       

    }
    /// <summary>Charger の捕獲後追加行動、または手番終了を処理します。</summary>
    /// <param name="moveAgain">今回の移動で敵駒を捕獲した場合は <see langword="true" /> です。</param>
    private void ChargerFinalBuff(bool moveAgain)
    {
        if (!moveAgain || !_player.charger.canMoveItAgain)
        {
            _player.Player_TurnEnd();
            return;
        }
        // 初回の捕獲後だけ追加操作を開始し、2回目の移動後に手番を終了します。
        if (!isMoveAgain)
        {
            isMoveAgain = true;
            _player.playerInPut.StartOneMoreMove(this);
        }
        else
        {
            isMoveAgain = false;
            _player.Player_TurnEnd();

        }


    }
    /// <summary>Skirmisher の左右に隣接する敵駒を収集して捕獲します。</summary>
    private void SkirmisherFinalBuff()
    {
        if (_player.skirmisher.nowBuffLevel != 3)
        {
            _player.Player_TurnEnd();
            return;
        }

        Queue<ChessBasic> eatQueue = new Queue<ChessBasic>();

        // Skirmisher の追加方向に隣接する敵駒を捕獲対象へ追加します。
        foreach (Vector2Int dir in _player.skirmisher.extraCanGoArea)
        {
            Vector2Int targetPos = position + dir;
            if (_chessBoard.board.TryGetValue(targetPos, out ChessBasic chess))
            {
                if (chess.color != this.color) eatQueue.Enqueue(chess);
            }

        }

        // 収集した敵駒へ撃破演出を適用して盤面から取り除きます。
        while (eatQueue.Count>0)
        {
            ChessBasic chess = eatQueue.Dequeue();
            _chessBoard.DeadEffect(chess);
            chess.GotEaten();
        }

        _player.Player_TurnEnd();
    }

}
