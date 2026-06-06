using System.Collections.Generic;
using System;
using UnityEngine;
using static Player;
using Unity.VisualScripting;

public class SageKing : BuffBasic
{
    public override ChessType buffChess => ChessType.King;
    public override string buffName => "SageKing";
    public override void Choose() => _player.kingBuffType = Player.KingBuff.SageKing;


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

        Player others = _player == InGame.Instance.whiteChessPlayer ? 
            InGame.Instance.blackChessPlayer : InGame.Instance.whiteChessPlayer;
        addBarrierPercent = others.kingBuffType == Player.KingBuff.MadKing ? 60.0f : 30.0f;
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
    public override void Choose() => _player.kingBuffType = Player.KingBuff.MadKing;

    public int extraFindRange = 2;
    public bool cantReSpawn = false;

    public bool canThroughAndEatAllChess { get; private set; } = false;
    public bool canMoveItAgain { get; private set; } = false;


    public override void ResetBuff()
    {
        cantReSpawn = false;
        canThroughAndEatAllChess = false;
        extraFindRange = 2;
        canMoveItAgain = false;
    }

    public override void FirstLevel()
    {
        cantReSpawn = true;
    }
    public override void SecondLevel()
    {
        canMoveItAgain = true;
    }
    public override void ThirdLevel()
    {
        extraFindRange = 3;
        canThroughAndEatAllChess = true;
    }

}


public class King : ChessBasic
{
    public override ChessType type => ChessType.King;
    public override int findRange { get;} = 1;
    private bool isMoveAgain = false;
    public override string ChessName() { return "King"; }

    private Player _enemy;

    public override void ChessInit(Player player)
    {
        base.ChessInit(player);

        _enemy = player == InGame.Instance.whiteChessPlayer ? 
            InGame.Instance.blackChessPlayer : InGame.Instance.whiteChessPlayer;
    }


    public bool haveBarrier = false;
    public void AddBarrier()
    {
        if (haveBarrier) return;
        haveBarrier = true;
    }

    public override HashSet<Vector2Int> directions => new HashSet<Vector2Int>
    {Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right,
        new Vector2Int(1, 1),new Vector2Int(1, -1),new Vector2Int(-1, 1),new Vector2Int(-1, -1) };

    private void MadKing_FindPossibleMove()
    {
        bool canThroughAndEatAllChess = _player.madKing.canThroughAndEatAllChess;

        foreach (var dir in directions)
        {
            for (int i = 1; i <= _player.madKing.extraFindRange; i++)
            {
                Vector2Int targetPos = position + dir * i;

                if (IsOutOfBoard(targetPos)) break;

                if (_chessBoard.board.TryGetValue(targetPos, out ChessBasic chess))
                {
                    if (canThroughAndEatAllChess)
                    {
                        possibleMoveList.Add(targetPos);
                        possibleEatList.Add(targetPos);

                    }
                    else if (chess.color != this.color)
                    {
                        possibleMoveList.Add(targetPos);
                        possibleEatList.Add(targetPos);
                        break;
                    }
                }
                else
                {
                    possibleMoveList.Add(targetPos);
                }

            }

        }

    }


    private void MadKing_Level2_MoveAgain(bool moveAgain)
    {
        if (_player.kingBuffType != Player.KingBuff.MadKing && !_player.madKing.canMoveItAgain)
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
    private void MadKing_Level3_ThroughAndEatAllChess(Vector2Int nowPosition , Vector2Int moveTo, out Queue<ChessBasic> eatqueue)
    {
        if (_player.kingBuffType != Player.KingBuff.MadKing && !_player.madKing.canThroughAndEatAllChess)
        {
            eatqueue = null;
            return;
        }
        eatqueue = new Queue<ChessBasic>();
        Vector2Int dir = new Vector2Int(Math.Sign(moveTo.x - nowPosition.x), Math.Sign(moveTo.y - nowPosition.y));

        while (nowPosition != moveTo)
        {
            nowPosition += dir;
            bool posHaveChess = _chessBoard.board.TryGetValue(nowPosition, out ChessBasic chess);
            if (posHaveChess && !chess.haveExtraLife) eatqueue.Enqueue(chess);
        }
    }
    private void MadKing_MoveTo(Vector2Int moveTo)
    {
        bool canMoveAgain = _player.madKing.canMoveItAgain;
        bool canThroughAndEatAllChess = _player.madKing.canThroughAndEatAllChess;

        if (!canMoveAgain && !canThroughAndEatAllChess)
        {
            base.Move(moveTo);
            return;
        }

        ReturnPick();
        bool canMoveTo = CanMoveTo(moveTo, out ChessBasic chessCanMoveTo);
        Vector2Int nowPosition = position;
        bool isEatTheChess = chessCanMoveTo != null;

        if (!canMoveTo)
        {
            _player.nowPlayerStage = PlayerStage.ReadytoEnd;
            return;
        }

        MadKing_Level3_ThroughAndEatAllChess(nowPosition, moveTo, out Queue<ChessBasic> eatqueue);
        MoveOnly(moveTo);
        while (eatqueue.Count > 0) eatqueue.Dequeue().GotEaten();

        MadKing_Level2_MoveAgain(isEatTheChess);


    }

    public override void FindPossibleMove()
    {
        possibleMoveList.Clear();
        possibleEatList.Clear();

        bool isMadKing = _player.kingBuffType == Player.KingBuff.MadKing;
        if (isMadKing) MadKing_FindPossibleMove();
        else FindCanMove(false);

        _chessBoard.ShowActive(ChessBlockStage.CanGo, possibleMoveList);
        _chessBoard.ShowActive(ChessBlockStage.CanEat, possibleEatList);
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
            _chessBoard.GenChess(targetSpawn, new Pair<ChessColor, ChessType>(color, ChessType.King));
            return;
        }

        if (chess.color == this.color)
        {
            InGame.Instance.GameSet();
            return;
        }

        ChessType killerType = CanKillKingType();

        if (chess.type != killerType)
        {
            chess.GotEaten();

            _chessBoard.GenChess(
                targetSpawn,
                new Pair<ChessColor, ChessType>(color, ChessType.King));

            return;
        }

        InGame.Instance.GameSet();

    }

    public override void GotEaten()
    {
        base.GotEaten();
        Respawn();
    }

    public override void Move(Vector2Int moveTo)
    {
        if(_player.kingBuffType != Player.KingBuff.MadKing) base.Move(moveTo);
        MadKing_MoveTo(moveTo);
    }


}
