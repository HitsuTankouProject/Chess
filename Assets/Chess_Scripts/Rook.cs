using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Rook : ChessBasic
{
    public override ChessType type => ChessType.Rook;

    public override string ChessName() { return "Rook"; }

    private List<Vector2Int> directions = new List<Vector2Int>
    { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
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
