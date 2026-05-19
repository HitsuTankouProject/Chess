using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


public class SageKing : BuffBasic
{
    public override ChessType buffChess => ChessType.King;
    public override string buffName => "SageKing";

    public bool canChangeSpawnWithWhiteBlock = false;
    public bool canPurifyChess = false;
    public bool winWentPosIsOthersKingStartingPoint = false;

   
    public override void ResetBuff()
    {
        canChangeSpawnWithWhiteBlock = false;
        canPurifyChess = false;
        winWentPosIsOthersKingStartingPoint = false;
    }
    public override void FirstLevel()
    {
        canChangeSpawnWithWhiteBlock = true;
    }
    public override void SecondLevel()
    {
        canPurifyChess = true;
    }
    public override void ThirdLevel()
    {
        winWentPosIsOthersKingStartingPoint = true;
    }

}


public class MadKing : BuffBasic
{
    public override ChessType buffChess => ChessType.King;
    public override string buffName => "MadKing";

    public const int extraFindRange = 3;
    public Sorcerer sorcerer = new Sorcerer();
    public Charger charger = new Charger();

    public override void ResetBuff()
    {
        //_buffChess.findRange = 1;
        sorcerer.LevelUpToTargetLevel(0, out bool successSorcerer);
        charger.LevelUpToTargetLevel(0, out bool successCharger);

    }

    public override void FirstLevel()
    {
        //_buffChess.findRange = extraFindRange;
    }
    public override void SecondLevel()
    {
        sorcerer.LevelUpToTargetLevel(3, out bool success);

    }
    public override void ThirdLevel()
    {
        charger.LevelUpToTargetLevel(3, out bool success);

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

    public SageKing sageKing = new SageKing();
    public MadKing madKing = new MadKing();



    private List<Vector2Int> directions = new List<Vector2Int>
    {Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right,
        new Vector2Int(1, 1),new Vector2Int(1, -1),new Vector2Int(-1, 1),new Vector2Int(-1, -1) };

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
