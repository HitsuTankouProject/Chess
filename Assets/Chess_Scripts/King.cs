using System.Collections.Generic;
using System;
using UnityEngine;
using static Player;
using Unity.VisualScripting;

public class SageKing : BuffBasic
{
    public override ChessType buffChess => ChessType.King;
    public override string buffName => "SageKing";
    public override void Choose() => _player.allTheBuff.kingBuffType = KingBuff.SageKing;


    public bool cantReSpawn = false;
    public bool canAddBarrierInPercent = false;
    private float addBarrierPercent;

    public bool winWentKingCrossTheBoard = false;

   
    public override void ResetBuff()
    {
        cantReSpawn = false;
        canAddBarrierInPercent = false;
        winWentKingCrossTheBoard = false;
    }
    public override void FirstLevel()
    {
        cantReSpawn = true;
    }
    public override void SecondLevel()
    {
        canAddBarrierInPercent = true;

        ChessColor othersChessColor = _player.usingChess == ChessColor.White ? ChessColor.Black : ChessColor.White;
        addBarrierPercent = 
            GameManager.Instance.TargetPlayer(othersChessColor).kingBuffType == KingBuff.MadKing 
            ? 60.0f : 30.0f;
    }

    public bool TryAddBarrier()
    {
        if (!canAddBarrierInPercent) return false;
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        return randomValue < addBarrierPercent;
    }

    public override void ThirdLevel()
    {
        winWentKingCrossTheBoard = true;
    }

}

public class MadKing : BuffBasic
{
    public override ChessType buffChess => ChessType.King;
    public override string buffName => "MadKing";
    public override void Choose() => _player.allTheBuff.kingBuffType = KingBuff.MadKing;

    public int extraFindRange = 1;
    public bool cantReSpawn = false;

    public bool canThroughAndEatAllChess { get; private set; } = false;
    public bool canMoveItAgain { get; private set; } = false;


    public override void ResetBuff()
    {
        cantReSpawn = false;
        canThroughAndEatAllChess = false;
        canMoveItAgain = false;
    }

    public override void FirstLevel()
    {
        cantReSpawn = true;
    }
    public override void SecondLevel()
    {
        extraFindRange = 2;
        canThroughAndEatAllChess = true;
        
    }
    public override void ThirdLevel()
    {
        canMoveItAgain = true;
    }

}


public class King : ChessBasic
{
    public override ChessType type => ChessType.King;
    public override int findRange { get;} = 1;
    private bool isMoveAgain = false;
    private bool canReSpawn = true;
    public override string ChessName() { return "King"; }

    private Player _enemy;

    public override void ChessInit(Player player)
    {
        base.ChessInit(player);

        ChessColor othersChessColor = _player.usingChess == ChessColor.White ? ChessColor.Black : ChessColor.White;
        _enemy = GameManager.Instance.TargetPlayer(othersChessColor);
    }

    public override HashSet<Vector2Int> directions => new HashSet<Vector2Int>
    {Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right,
        new Vector2Int(1, 1),new Vector2Int(1, -1),new Vector2Int(-1, 1),new Vector2Int(-1, -1) };

    private void SageKing_Level2_AddBarrier()
    {
        if (!_player.sageKing.TryAddBarrier()) return;
        else GotExtraLife(true);
    }

    private bool MadKing_Level2_ThroughAndEatAllChess(Vector2Int nowPosition, Vector2Int moveTo, out Queue<ChessBasic> eatqueue)
    {
        eatqueue = null;
        if(!_player.madKing.canThroughAndEatAllChess) return false;
        eatqueue = new();
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
    private void MadKing_Level3_MoveAgain()
    {
        if (!_player.madKing.canMoveItAgain) 
        {
            _player.nowPlayerStage = PlayerStage.ReadytoEnd;
            return;
        }

        if (!isMoveAgain)
        {
            isMoveAgain = true;
            StartCoroutine(_player.playerInPut.OneMoreMove(this));
        }
        else
        {
            isMoveAgain = false;
            _player.nowPlayerStage = PlayerStage.ReadytoEnd;

        }

    }
    private void MadKing_MoveTo(Vector2Int moveTo)
    {
        _player.nowPlayerStage = PlayerStage.MovingChess;
        ReturnPick();
        if (!CanMoveTo(moveTo, out ChessBasic chess))
        {
            _player.nowPlayerStage = PlayerStage.ReadytoEnd;
            return;
        }
        bool pathThroughHaveChess = MadKing_Level2_ThroughAndEatAllChess(position, moveTo, out Queue<ChessBasic> eatqueue);

        if(chess == null && !pathThroughHaveChess)
        {
            _chessBoard.MoveTo(this, moveTo);
            _player.nowPlayerStage = PlayerStage.ReadytoEnd;
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

    private ChessType CanKillKingType()
    {
        int count = Enum.GetValues(typeof(ChessType)).Length;

        for (int i = count - 1; i > 0; i--)
        {
            ChessType chessType = (ChessType)i;

            if (_enemy.ChessListByType(chessType).Count > 0)
                return chessType;
        }

        return ChessType.King;
    }

    private void Respawn()
    {
        Vector2Int targetSpawn = color == ChessColor.White ? 
            _chessBoard.white_KingChessSpawn : _chessBoard.black_KingChessSpawn;

        if (targetSpawn == new Vector2Int(-1, -1))
        {
            Debug.LogError("No Spawn Location");
            return;
        }

        if (!_chessBoard.board.TryGetValue(targetSpawn, out ChessBasic chess))
        {
            _chessBoard.StartGenChessProcess(targetSpawn, new Pair<ChessColor, ChessType>(color, ChessType.King));
            return;
        }
        ChessType killerType = CanKillKingType();

        if (chess.type != killerType)
        {
            chess.GotEaten();

            _chessBoard.StartGenChessProcess(targetSpawn,new Pair<ChessColor, ChessType>(color, ChessType.King), _player);

            return;
        }

        GameManager.Instance.EndInGame(color);

    }

    public override void GotEaten()
    {
        base.GotEaten();
        Respawn();
    }


    public override void Move(Vector2Int moveTo)
    {
        SageKing_Level2_AddBarrier();
        if (_player.madKing.nowBuffLevel <= 1)
            base.Move(moveTo);
        else MadKing_MoveTo(moveTo);


    }


}
