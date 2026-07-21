using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// ルークへ Rusher の能力を付与するバフです。
/// レベルに応じて味方駒、敵駒の順に貫通移動を開放し、
/// 最大レベルでは移動距離を制限する代わりに経路上の敵駒を捕獲します。
/// </summary>
public class Rusher : BuffBasic
{
    /// <summary>このバフの対象となる駒種を取得します。</summary>
    public override ChessType buffChess => ChessType.Rook;
    /// <summary>バフの表示名を取得します。</summary>
    public override string buffName => "Rusher";
    /// <summary>プレイヤーが使用するルークバフを Rusher に設定します。</summary>
    public override void Choose() => _player.allTheBuff.rookBuffType = RookBuff.Rusher;
    /// <summary>味方駒を越えて移動できるかどうかを示します。</summary>
    public bool canThroughSameColor = false;
    /// <summary>敵駒を越えて移動できるかどうかを示します。</summary>
    public bool canThroughNonSameColor = false;
    /// <summary>Rusher 使用中に各方向へ探索する最大距離です。</summary>
    public int findRange = 8;
    /// <summary>貫通した敵駒を経路上で捕獲できるかどうかを示します。</summary>
    public bool canEatThroughNonSameColorChess = false;
    /// <summary>Rusher 固有の貫通能力をすべて無効化します。</summary>
    public override void ResetBuff()
    {
        canThroughSameColor = false;
        canThroughNonSameColor = false;
        canEatThroughNonSameColorChess = false;
    }
    /// <summary>味方駒を越えて移動する能力を開放します。</summary>
    public override void FirstLevel()
    {
        canThroughSameColor = true;
    }
    /// <summary>敵駒を越えて移動する能力を開放します。</summary>
    public override void SecondLevel()
    {
        canThroughNonSameColor = true;
    }
    /// <summary>探索距離を4マスへ制限し、経路上の敵駒を捕獲する能力を開放します。</summary>
    public override void ThirdLevel()
    {
        findRange = 4;
        canEatThroughNonSameColorChess = true;
    }
}
/// <summary>
/// ルークへ Guardian の能力を付与するバフです。
/// レベルに応じて左右、上下、斜めの順に保護範囲を広げ、
/// 隣接する味方駒へ捕獲を一度防ぐ追加ライフを付与します。
/// </summary>
public class Guardian : BuffBasic
{
    /// <summary>このバフの対象となる駒種を取得します。</summary>
    public override ChessType buffChess => ChessType.Rook;
    /// <summary>バフの表示名を取得します。</summary>
    public override string buffName => "Guardian";
    /// <summary>プレイヤーが使用するルークバフを Guardian に設定します。</summary>
    public override void Choose() => _player.allTheBuff.rookBuffType = RookBuff.Guardian;
    /// <summary>ルークを基準として味方駒を保護する相対方向です。</summary>
    public HashSet<Vector2Int> protectArea = new HashSet<Vector2Int>();
    /// <summary>レベル1で追加される左右の保護方向です。</summary>
    private readonly HashSet<Vector2Int> firstProtectArea = new HashSet<Vector2Int>() 
    {
        Vector2Int.left, Vector2Int.right
    };
    /// <summary>レベル2で追加される上下の保護方向です。</summary>
    private readonly HashSet<Vector2Int> secondProtectArea = new HashSet<Vector2Int>()
    {
        Vector2Int.up, Vector2Int.down
    };
    /// <summary>レベル3で追加される4つの斜め保護方向です。</summary>
    private readonly HashSet<Vector2Int> thirdProtectArea = new HashSet<Vector2Int>() 
    {
        new Vector2Int(-1, 1),
        new Vector2Int(1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1, -1)

    };
    /// <summary>現在の保護方向をすべて削除します。</summary>
    public override void ResetBuff()
    {
        protectArea = new HashSet<Vector2Int>();
    }
    /// <summary>左右方向の味方駒を保護できるようにします。</summary>
    public override void FirstLevel()
    {
        protectArea.AddRange(firstProtectArea);
    }
    /// <summary>上下方向の味方駒を保護できるようにします。</summary>
    public override void SecondLevel()
    {
        protectArea.AddRange(secondProtectArea);
    }
    /// <summary>斜め方向の味方駒を保護できるようにします。</summary>
    public override void ThirdLevel()
    {
        protectArea.AddRange(thirdProtectArea);
    }


}

/// <summary>
/// 縦横方向へ移動するルークの駒を管理します。
/// Rusher 選択時は駒を貫通する移動と経路上の連続捕獲を処理し、
/// Guardian 選択時は周囲の味方駒へ追加ライフを付与して保護します。
/// </summary>
public class Rook : ChessBasic
{
    /// <summary>この駒の種類を取得します。</summary>
    public override ChessType type => ChessType.Rook;
    /// <summary>駒の表示名を取得します。</summary>
    /// <returns>ルークを示す文字列を返します。</returns>
    public override string ChessName() { return "Rook"; }
    /// <summary>通常状態で各方向へ探索する最大距離を取得します。</summary>
    public override int findRange { get; } = 8;
    /// <summary>ルークが通常移動できる上下左右の4方向を取得します。</summary>
    public override HashSet<Vector2Int> directions => new HashSet<Vector2Int>
    { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    /// <summary>指定した色の駒を Rusher が越えて移動できるか判定します。</summary>
    /// <param name="color">経路上に存在する駒の色です。</param>
    /// <returns>その駒を越えて探索を続けられる場合は <see langword="true" /> です。</returns>
    private bool CanThrough(ChessColor color)
    {
        bool canThroughSameColor = _player.rusher.canThroughSameColor;
        bool canThroughNonSameColor = _player.rusher.canThroughNonSameColor;
        if (!canThroughSameColor && !canThroughNonSameColor) return false;

        if(color == this.color&& canThroughSameColor) return true;
        else if (color != this.color && canThroughNonSameColor) return true;

        return false;


    }
    /// <summary>Rusher の貫通能力と探索距離に基づいて移動・捕獲候補を計算します。</summary>
    private void Rusher_FindCanMove()
    {
        foreach (var dir in directions)
        {
            for (int i = 1; i <=_player.rusher.findRange; i++)
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
                    // 経路上の駒を貫通できない場合は、この方向の探索を終了します。
                    if (!CanThrough(chess.color)) break;
                }
                else
                {
                    possibleMoveList.Add(targetPos);
                }
            }
        }

    }
    /// <summary>Guardian の保護範囲内にいる味方駒へ追加ライフを付与します。</summary>
    public void GuardianBuff()
    {
        if (_player.rookBuffType != RookBuff.Guardian || _player.guardian.protectArea.Count == 0) return;
        foreach (Vector2Int protectedDir in _player.guardian.protectArea)
        {
            Vector2Int spot = position + protectedDir;
            if (_chessBoard.IsOutOfBoard(spot)) continue;

            // 保護方向に存在する味方駒だけを Guardian の保護対象へ追加します。
            if (!_chessBoard.board.TryGetValue(spot, out ChessBasic chess)
                || chess.color != this.color) continue;
            _player.AddGuardianProtectedChess(chess);
        }
    }
    /// <summary>選択中のルークバフに対応する方法で移動・捕獲候補を計算します。</summary>
    /// <param name="isThrough">他の駒を越えて探索を続ける場合は <see langword="true" /> です。</param>
    public override void FindCanMove(bool isThrough)
    {
        if (_player.rookBuffType != RookBuff.Rusher) base.FindCanMove(isThrough);
        else Rusher_FindCanMove();
    }
    /// <summary>Rusher 固有の経路捕獲を含む移動を実行します。</summary>
    /// <param name="moveTo">移動先の盤面座標です。</param>
    private void Rusher_MoveTo(Vector2Int moveTo)
    {
        if (!_player.rusher.canEatThroughNonSameColorChess)
        {
            base.Move(moveTo);
            return;
        }
        bool canMoveTo = CanMoveTo(moveTo, out ChessBasic chessCanMoveTo);
        Vector2Int nowPosition = position;
        ReturnPick();

        if (!canMoveTo)
        {
            _player.Player_TurnEnd();
            return;
        }

        Queue<ChessBasic> eatqueue = new Queue<ChessBasic>();
        Vector2Int dir = new Vector2Int(Math.Sign(moveTo.x - nowPosition.x), Math.Sign(moveTo.y - nowPosition.y));

        // 開始地点から移動先までを走査し、捕獲可能な敵駒を順番に収集します。
        while (nowPosition!= moveTo)
        {
            nowPosition += dir;
            bool posHaveChess = _chessBoard.board.TryGetValue(nowPosition, out ChessBasic chess);
            if (posHaveChess && chess.color != color && !chess.haveExtraLife) eatqueue.Enqueue(chess);
        }
        MoveOnly(moveTo);

        // 経路上で収集した敵駒を移動順に盤面から取り除きます。
        while (eatqueue.Count > 0)
        {
            ChessBasic chess = eatqueue.Dequeue();
            _chessBoard.DeadEffect(chess);
            chess.GotEaten();
        }

        _player.Player_TurnEnd();
    }
    /// <summary>指定座標へ移動し、選択中のルークバフの追加処理を実行します。</summary>
    /// <param name="moveTo">移動先の盤面座標です。</param>
    public override void Move(Vector2Int moveTo)
    {
        if (_player.rookBuffType == RookBuff.None)
        {
            base.Move(moveTo);
            return;
        }
        else if(_player.rookBuffType == RookBuff.Rusher)
        {
            Rusher_MoveTo(moveTo);
        }
        else if (_player.rookBuffType == RookBuff.Guardian)
        {
            // 移動後のルーク配置に合わせ、全 Guardian の保護範囲を更新します。
            MoveOnly(moveTo);
            _player.UpdateGuardianProtectArea();
            _player.Player_TurnEnd();
            return;
        }

    }

}
