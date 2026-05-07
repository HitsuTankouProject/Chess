using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessBasic
{
    public override ChessType type => ChessType.Pawn;
    private bool isFirstMove = true;
    public override string ChessName() { return "Pawn"; }

    private List<Vector2Int> attackDirs = new List<Vector2Int>
    { new Vector2Int(1, 1), new Vector2Int(-1, 1) };
    public override void FindPossibleMove()
    {
        possibleMoveList.Clear();

        int direction = (color == ChessColor.White) ? 1 : -1;

        Vector2Int forward = position + new Vector2Int(0, direction);

        if (!_chessBoard.board.ContainsKey(forward))
        {
            possibleMoveList.Add(forward);

            if (isFirstMove)
            {
                Vector2Int doubleForward = position + new Vector2Int(0, direction * 2);

                if (!_chessBoard.board.ContainsKey(doubleForward))
                {
                    possibleMoveList.Add(doubleForward);
                }
            }
        }

        foreach (var dir in attackDirs)
        {
            Vector2Int attackPos = position + dir;

            if (_chessBoard.board.TryGetValue(attackPos, out ChessBasic chess))
            {
                if (chess.color != this.color)
                {
                    possibleMoveList.Add(attackPos);
                }
            }
        }

        _chessBoard.ShowCanGo(possibleMoveList);
    }

}
