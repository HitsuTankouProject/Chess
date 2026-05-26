using System.Collections.Generic;
using System;
using UnityEngine;
using static Player;


public class SageKing : BuffBasic
{
    public override ChessType buffChess => ChessType.King;
    public override string buffName => "SageKing";

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
        addBarrierPercent = others.kingBuffType == Player.KingBuff.MadKing ? 30.0f : 60.0f;
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
        canThroughAndEatAllChess = true;
    }
    public override void ThirdLevel()
    {
        extraFindRange = 3;
        canMoveItAgain = true;
    }

}


public class King : ChessBasic
{
    public override ChessType type => ChessType.King;
    public override int findRange { get; protected set; } = 1;
    private bool isMoveAgain = false;

    public override string ChessName() { return "King"; }

    public bool haveBarrier { get; private set; } = false;
    public void AddBarrier()
    {
        if (haveBarrier) return;
        haveBarrier = true;
    }

    public override List<Vector2Int> directions => new List<Vector2Int>
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
    private void MadKing_MoveTo(Vector2Int moveTo)
    {
        _player.nowPlayerStage = PlayerStage.MovingChess;
        ReturnPick();
        if (!CanMoveTo(moveTo,out ChessBasic chessCanMoveTo)) 
        {
            _player.nowPlayerStage = PlayerStage.ReadytoEnd;
            return;
        }


        Queue<ChessBasic> eatqueue = new Queue<ChessBasic>();
        Vector2Int dir = new Vector2Int(Math.Sign(moveTo.x - position.x), Math.Sign(moveTo.y - position.y));
        Vector2Int targetPos = position;
        for (int i = 1; targetPos != moveTo; i++)
        {
            targetPos = position + dir * i;
            bool posHaveChess = _chessBoard.board.TryGetValue(targetPos, out ChessBasic chess);

            if (posHaveChess && chess != null) eatqueue.Enqueue(chess);
        }
        while (eatqueue.Count > 0)
        {
            ChessBasic chessBasic = eatqueue.Dequeue();
            bool chessHaveBeenProtected = chessBasic.haveExtraLife;

            if (chessHaveBeenProtected) continue;
            else chessBasic.GotEaten();
        }
        this.transform.position = _chessBoard.ReturnChessBlockPosition(moveTo);
        _chessBoard.BoardUpdate(this, moveTo, ChessAction.Move);

        _player.nowPlayerStage = PlayerStage.ReadytoEnd;
    }


    private void Normal_FindPossibleMove()
    {
        foreach (var dir in directions)
        {
            Vector2Int targetPos = position + dir;

            if (IsOutOfBoard(targetPos))continue;

            if (_chessBoard.board.TryGetValue(targetPos, out ChessBasic chess))
            {
                if (chess.color != this.color)
                {
                    possibleMoveList.Add(targetPos);
                    possibleEatList.Add(targetPos);

                }
            }
            else
            {
                possibleMoveList.Add(targetPos);
            }
        }
    }

    public override void FindPossibleMove()
    {
        possibleMoveList.Clear();
        possibleEatList.Clear();

        bool isMadKing = _player.kingBuffType == Player.KingBuff.MadKing;
        if (isMadKing) MadKing_FindPossibleMove();
        else Normal_FindPossibleMove();

        _chessBoard.ShowActive(ChessBlockStage.CanGo, possibleMoveList);
        _chessBoard.ShowActive(ChessBlockStage.CanEat, possibleEatList);
    }

    public override void GotEaten()
    {

       base.GotEaten();
       ChessBlock targetSpawn = color == ChessColor.White ? _chessBoard.white_KingChessSpawn : _chessBoard.black_KingChessSpawn;
       if (targetSpawn == null)
        {
            Debug.LogError("No Spawn Location");
            return;
        }


        bool isSpawnHaveChess = _chessBoard.board.ContainsKey(targetSpawn.position);
        if (!isSpawnHaveChess)
        {
            _chessBoard.GenChess(targetSpawn.position, new Pair<ChessColor, ChessType>(color, ChessType.King));
        }
        else
        {
            ChessBasic spawnChess = _chessBoard.board[targetSpawn.position];
            bool sameColor = _chessBoard.board[targetSpawn.position].color == color;
            bool isPawn = _chessBoard.board[targetSpawn.position].type == ChessType.Pawn;

            if (!isPawn && !sameColor)
            {
                spawnChess.GotEaten();
                _chessBoard.GenChess(targetSpawn.position, new Pair<ChessColor, ChessType>(color, ChessType.King));
            }





        }

    }

    public override void Move(Vector2Int moveTo)
    {

    }


}
