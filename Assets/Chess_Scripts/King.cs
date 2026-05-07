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

}
