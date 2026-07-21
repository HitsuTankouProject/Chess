using System.Collections.Generic;
using System;
using UnityEngine;
using static Player;
using Unity.VisualScripting;

/// <summary>
/// キングへ SageKing の能力を付与するバフです。
/// レベルに応じて再出現の禁止、確率によるバリア付与、
/// 敵キングの開始地点へ到達した際の特殊勝利条件を開放します。
/// </summary>
public class SageKing : BuffBasic
{
    /// <summary>このバフの対象となる駒種を取得します。</summary>
    public override ChessType buffChess => ChessType.King;
    /// <summary>バフの表示名を取得します。</summary>
    public override string buffName => "SageKing";
    /// <summary>プレイヤーが使用するキングバフを SageKing に設定します。</summary>
    public override void Choose() => _player.allTheBuff.kingBuffType = KingBuff.SageKing;
    /// <summary>キングの再出現を禁止する効果が有効かどうかを示します。</summary>
    public bool cantReSpawn = false;
    /// <summary>移動時に確率でバリアを付与する効果が有効かどうかを示します。</summary>
    public bool canAddBarrierInPercent = false;
    /// <summary>バリアが付与される確率を百分率で保持します。</summary>
    private float addBarrierPercent;
    /// <summary>敵陣を横断した際の特殊勝利条件が有効かどうかを示します。</summary>
    public bool winWentKingCrossTheBoard = false;
    /// <summary>SageKing 固有の能力をすべて無効化します。</summary>
    public override void ResetBuff()
    {
        cantReSpawn = false;
        canAddBarrierInPercent = false;
        winWentKingCrossTheBoard = false;
    }
    /// <summary>キングの復活を禁止します。</summary>
    public override void FirstLevel()
    {
        cantReSpawn = true;
    }
    /// <summary>移動時のバリア付与を開放し、対戦相手に応じて発動確率を設定します。</summary>
    public override void SecondLevel()
    {
        canAddBarrierInPercent = true;

        ChessColor othersChessColor = _player.usingChess == ChessColor.White ? ChessColor.Black : ChessColor.White;
        // 対戦相手が MadKing を使用している場合はバリア確率を上昇させます。
        addBarrierPercent = 
            GameManager.Instance.TargetPlayer(othersChessColor).kingBuffType == KingBuff.MadKing 
            ? 60.0f : 30.0f;
    }
    /// <summary>設定済みの確率に基づいてバリアを付与できるか抽選します。</summary>
    /// <returns>バリアの付与条件を満たした場合は <see langword="true" /> です。</returns>
    public bool TryAddBarrier()
    {
        if (!canAddBarrierInPercent) return false;
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        return randomValue < addBarrierPercent;
    }
    /// <summary>敵キングの開始地点へ到達した際の特殊勝利条件を開放します。</summary>
    public override void ThirdLevel()
    {
        winWentKingCrossTheBoard = true;
    }

}
/// <summary>
/// キングへ MadKing の能力を付与するバフです。
/// レベルに応じて再出現の禁止、移動距離の拡張と経路上の駒の捕獲、
/// 1手の追加行動を開放します。
/// </summary>
public class MadKing : BuffBasic
{
    /// <summary>このバフの対象となる駒種を取得します。</summary>
    public override ChessType buffChess => ChessType.King;
    /// <summary>バフの表示名を取得します。</summary>
    public override string buffName => "MadKing";
    /// <summary>プレイヤーが使用するキングバフを MadKing に設定します。</summary>
    public override void Choose() => _player.allTheBuff.kingBuffType = KingBuff.MadKing;
    /// <summary>MadKing が各方向へ探索できる最大距離です。</summary>
    public int extraFindRange = 1;
    /// <summary>キングの再出現を禁止する効果が有効かどうかを示します。</summary>
    public bool cantReSpawn = false;
    /// <summary>経路上の駒を通過して捕獲できるかどうかを取得します。</summary>
    public bool canThroughAndEatAllChess { get; private set; } = false;
    /// <summary>移動後にもう一度行動できるかどうかを取得します。</summary>
    public bool canMoveItAgain { get; private set; } = false;
    /// <summary>MadKing 固有の能力をすべて無効化します。</summary>
    public override void ResetBuff()
    {
        cantReSpawn = false;
        canThroughAndEatAllChess = false;
        canMoveItAgain = false;
    }
    /// <summary>キングの復活を禁止します。</summary>
    public override void FirstLevel()
    {
        cantReSpawn = true;
    }
    /// <summary>探索距離を2マスへ拡張し、経路上の駒を捕獲する能力を開放します。</summary>
    public override void SecondLevel()
    {
        extraFindRange = 2;
        canThroughAndEatAllChess = true;
        
    }
    /// <summary>移動後の追加行動を開放します。</summary>
    public override void ThirdLevel()
    {
        canMoveItAgain = true;
    }

}

/// <summary>
/// 縦、横、斜めの隣接マスへ移動するキングを管理します。
/// SageKing 選択時はバリア付与と敵陣到達による勝利を処理し、
/// MadKing 選択時は拡張移動、経路上の連続捕獲、追加行動を処理します。
/// 捕獲された場合は Substitute の身代わり判定後、必要に応じて再出現します。
/// </summary>
public class King : ChessBasic
{
    /// <summary>この駒の種類を取得します。</summary>
    public override ChessType type => ChessType.King;
    /// <summary>通常状態で各方向へ探索する距離を取得します。</summary>
    public override int findRange { get;} = 1;
    /// <summary>MadKing の追加行動をすでに使用したかどうかを示します。</summary>
    private bool isMoveAgain = false;
    /// <summary>捕獲されたキングが再出現できるかどうかを示します。</summary>
    private bool canReSpawn = true;
    /// <summary>駒の表示名を取得します。</summary>
    /// <returns>キングを示す文字列を返します。</returns>
    public override string ChessName() { return "King"; }
    /// <summary>対戦相手のプレイヤーです。</summary>
    private Player _enemy;
    /// <summary>所有者と敵プレイヤーを設定し、再出現の初期条件を決定します。</summary>
    /// <param name="player">このキングを所有するプレイヤーです。</param>
    public override void ChessInit(Player player)
    {
        base.ChessInit(player);

        ChessColor othersChessColor = _player.usingChess == ChessColor.White ? ChessColor.Black : ChessColor.White;
        _enemy = GameManager.Instance.TargetPlayer(othersChessColor);
        canReSpawn = player.kingBuffType == KingBuff.None;
    }
    /// <summary>キングが通常移動できる縦、横、斜めの8方向を取得します。</summary>
    public override HashSet<Vector2Int> directions => new HashSet<Vector2Int>
    {Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right,
        new Vector2Int(1, 1),new Vector2Int(1, -1),new Vector2Int(-1, 1),new Vector2Int(-1, -1) };
    /// <summary>SageKing の確率抽選に成功した場合、このキングへバリアを付与します。</summary>
    private void SageKing_Level2_AddBarrier()
    {
        if (!_player.sageKing.TryAddBarrier()) return;
        else GotExtraLife(true);
    }
    /// <summary>敵キングの開始地点へ到達する特殊勝利条件を満たしたか判定します。</summary>
    /// <param name="moveTo">今回の移動先座標です。</param>
    /// <returns>SageKing が最大レベルで勝利地点へ到達した場合は <see langword="true" /> です。</returns>
    private bool SageKing_Level3_WonByGoToEnemyKingStart(Vector2Int moveTo)
    {
        if (_player.sageKing.nowBuffLevel != 3) return false;

        return _chessBoard.GetKingStartPoint(_enemy.usingChess) == moveTo;
    }
    /// <summary>MadKing の移動経路上にいる、追加ライフを持たない駒を収集します。</summary>
    /// <param name="nowPosition">移動開始時の盤面座標です。</param>
    /// <param name="moveTo">移動先の盤面座標です。</param>
    /// <param name="eatqueue">経路上で捕獲する駒を順番に格納したキューです。</param>
    /// <returns>経路上に捕獲対象が存在する場合は <see langword="true" /> です。</returns>
    private bool MadKing_Level2_ThroughAndEatAllChess(Vector2Int nowPosition, Vector2Int moveTo, out Queue<ChessBasic> eatqueue)
    {
        eatqueue = null;
        if(!_player.madKing.canThroughAndEatAllChess) return false;
        eatqueue = new();
        // 開始地点から移動先へ進む単位方向を求めます。
        Vector2Int dir = new Vector2Int(Math.Sign(moveTo.x - nowPosition.x), Math.Sign(moveTo.y - nowPosition.y));
        while (true)
        {
            nowPosition += dir;
            if(nowPosition == moveTo) break;
            bool posHaveChess = _chessBoard.board.TryGetValue(nowPosition, out ChessBasic chess);
            if (posHaveChess && !chess.haveExtraLife) eatqueue.Enqueue(chess);
        }
        return eatqueue.Count != 0;
    }
    /// <summary>MadKing のレベル3効果に応じて追加行動または手番終了を処理します。</summary>
    private void MadKing_Level3_MoveAgain()
    {
        if (!_player.madKing.canMoveItAgain) 
        {
            _player.Player_TurnEnd();
            return;
        }
        // 初回の移動後だけ追加操作を開始し、2回目の移動後に手番を終了します。
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
    /// <summary>MadKing 固有の経路捕獲と追加行動を含む移動を実行します。</summary>
    /// <param name="moveTo">移動先の盤面座標です。</param>
    private void MadKing_MoveTo(Vector2Int moveTo)
    {
        ReturnPick();
        if (!CanMoveTo(moveTo, out ChessBasic chess))
        {
            _player.Player_TurnEnd();
            return;
        }
        bool pathThroughHaveChess = MadKing_Level2_ThroughAndEatAllChess(position, moveTo, out Queue<ChessBasic> eatqueue);
        // 移動先と経路上の駒の有無に応じて移動・捕獲処理を分岐します。
        if (chess == null && !pathThroughHaveChess)
        {
            _chessBoard.MoveTo(this, moveTo);
            _player.Player_TurnEnd();
            return;
        }
        else if(chess != null && !pathThroughHaveChess)
        {
            EatChess(chess);
        }
        else if(chess != null && pathThroughHaveChess)
        {
            while (eatqueue.Count > 0) eatqueue.Dequeue().GotEaten();
            EatChess(chess);
        }
        else if(chess == null && pathThroughHaveChess)
        {
            while (eatqueue.Count > 0) eatqueue.Dequeue().GotEaten();
        }

        MadKing_Level3_MoveAgain();

    }
    /// <summary>MadKing のレベルに応じて拡張された移動・捕獲候補を計算します。</summary>
    /// <param name="isThrougt">基底クラスとの互換性のために受け取る貫通探索フラグです。</param>
    public override void ExtraFindPossibleMove(bool isThrougt)
    {
        if(_player.madKing.nowBuffLevel <= 1) return;
        foreach (var dir in directions)
        { 
            for (int i = 1; i <= _player.madKing.extraFindRange; i++)
            {
                Vector2Int targetPos = position + dir * i;

                if (IsOutOfBoard(targetPos)) break;

                if (_chessBoard.board.ContainsKey(targetPos))
                {
                    possibleEatList.Add(targetPos);

                }
                possibleMoveList.Add(targetPos);
            }
        }
    }
    /// <summary>捕獲されたキングを復活させられるか判定して処理します。</summary>
    private void Respawn()
    {
        if (!canReSpawn)
        {
            _player.haveKing = false;
            return;
        }
        Vector2Int targetSpawn = color == ChessColor.White ? 
            _chessBoard.white_KingChessSpawn : _chessBoard.black_KingChessSpawn;

        if (targetSpawn == new Vector2Int(-1, -1))
        {
            Debug.LogError("No Spawn Location");
            return;
        }
        // 開始地点が空いている場合は、その位置へ新しいキングを生成します。
        if (!_chessBoard.board.TryGetValue(targetSpawn, out ChessBasic chess))
        {
            _chessBoard.StartGenChessProcess(targetSpawn, new Pair<ChessColor, ChessType>(color, ChessType.King), _player);

            return;
        }
        // 味方駒が開始地点を占有している場合は、その駒と入れ替えて生成します。
        if (chess.color == color)
        {
            chess.GotEaten();
            _chessBoard.StartGenChessProcess(targetSpawn,new Pair<ChessColor, ChessType>(color, ChessType.King), _player);
            return;
        }
        // 敵駒が開始地点を占有している場合は再出現できません。
        _player.haveKing = false;
    }
    /// <summary>捕獲時に Substitute の身代わりを優先し、失敗した場合は再出現を試みます。</summary>
    public override void GotEaten()
    {
        if (_player.IsProtectbySubstitute(out ChessBasic pawn) && pawn != null)
        {
            SwapPosition(pawn);
            pawn.GotEaten();
        }
        else
        {
            Respawn();
            base.GotEaten();
        }

    }
    /// <summary>選択中のキングバフに対応する固有効果を含めて移動します。</summary>
    /// <param name="moveTo">移動先の盤面座標です。</param>
    public override void Move(Vector2Int moveTo)
    {
        // MadKing がレベル2以上の場合は専用の移動処理を使用します。
        if (_player.madKing.nowBuffLevel > 1)
        {
            MadKing_MoveTo(moveTo);
            return;
        }
        // 通常移動前に SageKing のバリア抽選を行います。
        SageKing_Level2_AddBarrier();
        base.Move(moveTo);
        // 敵キングの開始地点へ到達した場合は、この対局の勝者として確定します。
        if (SageKing_Level3_WonByGoToEnemyKingStart(moveTo))
            GameManager.Instance.EndInGame(_player.usingChess);

    }


}
