using System.Collections.Generic;
using UnityEngine;

public class King : ChessBasic
{
    public override ChessType type => ChessType.King;

    public override string ChessName() { return "King"; }

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
