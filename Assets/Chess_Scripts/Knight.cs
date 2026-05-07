using UnityEngine;
using System.Collections.Generic;

public class Knight : ChessBasic
{
    public override ChessType type => ChessType.Knight;
    public override string ChessName() { return "Knight"; }

    private List<Vector2Int> directions = new List<Vector2Int>
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

    public override void FindPossibleMove()
    {
        possibleMoveList.Clear();

        foreach (var move in directions)
        {
            Vector2Int targetPos = position + move;

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

}
