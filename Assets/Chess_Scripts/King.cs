using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


public class SageKing : BuffBasic
{
    public override ChessType buffChess => ChessType.King;
    public override string buffName => "SageKing";

    public bool cantReSpawn = false;
    public bool canAddBarrierInPercent = false;
    private float addBarrierPercent = 30.0f;
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
    }

    public bool TryAddBarrier()
    {
        if (!canAddBarrierInPercent) return false;
        float randomValue = Random.Range(0f, 100f);
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

    public const int extraFindRange = 3;
    public bool cantReSpawn = false;

    public bool canThroughAndEatChess { get; private set; } = false;
    public bool canMoveItAgain { get; private set; } = false;


    public override void ResetBuff()
    {
        cantReSpawn = false;
        canThroughAndEatChess = false;
        canMoveItAgain = false;
    }

    public override void FirstLevel()
    {
        cantReSpawn = true;
    }
    public override void SecondLevel()
    {
        canThroughAndEatChess = true;
    }
    public override void ThirdLevel()
    {
        canMoveItAgain = true;
    }

}


public class King : ChessBasic
{
    public override ChessType type => ChessType.King;
    public override int findRange { get; protected set; } = 1;

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

    private void MadKing_SorcererBuff()
    {

    }

    private void MadKing_FindPossibleMove()
    {
        bool haveSorcerer = _player.madKing.nowBuffLevel >= 2;

        foreach (var dir in directions)
        {
            for (int i = 1; i <= MadKing.extraFindRange; i++)
            {
                Vector2Int targetPos = position + dir * i;

                if (targetPos.x < 0 || targetPos.x >= 8 ||
                    targetPos.y < 0 || targetPos.y >= 8)
                    break;

                if (_chessBoard.board.TryGetValue(targetPos, out ChessBasic chess))
                {
                    if (chess.color != this.color)
                    {
                        possibleMoveList.Add(targetPos);
                    }
                    break;
                }
                else
                {
                    possibleMoveList.Add(targetPos);
                }
                if (!haveSorcerer) break;
            }

        }



    }


    public override void FindPossibleMove()
    {
        possibleMoveList.Clear();

        foreach (var dir in directions)
        {
            Vector2Int targetPos = position + dir;

            if (targetPos.x < 0 || targetPos.x >= 8 ||
                targetPos.y < 0 || targetPos.y >= 8)
                continue;

            if (_chessBoard.board.TryGetValue(targetPos, out ChessBasic chess))
            {
                if (chess.color != this.color)
                {
                    possibleMoveList.Add(targetPos);
                }
            }
            else
            {
                possibleMoveList.Add(targetPos);
            }
        }

        _chessBoard.ShowCanGo(possibleMoveList);
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


}
