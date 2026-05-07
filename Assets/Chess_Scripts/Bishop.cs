using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Bishop : ChessBasic
{
    public override ChessType type => ChessType.Bishop;
    public override string ChessName() { return "Bishop"; }

    private List<Vector2Int> directions = new List<Vector2Int>
     { new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

    public override void FindPossibleMove()
    {
        possibleMoveList.Clear();

        foreach (var dir in directions)
        {
            for (int i = 1; i < 8; i++)
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
            }
        }
        _chessBoard.ShowCanGo(possibleMoveList);
    }

}
