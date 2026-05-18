using System.Collections.Generic;
using UnityEngine;


public class Scout : BuffBasic
{
    public override ChessType buffChess => ChessType.Pawn;
    public override string buffName => "Evolver";

    public bool cantPromotion = false;
    public bool canReceiveMoveAreaFromYouAteChess = false;
    public List<Vector2Int> extraMoveArea = new List<Vector2Int>();

    public override void ResetBuff()
    {
        cantPromotion = false;
        canReceiveMoveAreaFromYouAteChess = false;
        extraMoveArea.Clear();
        _buffChess.findRange = 1;

    }

    public override void FirstLevel()
    {
        cantPromotion = true;
    }
    public override void SecondLevel()
    {
        canReceiveMoveAreaFromYouAteChess = true;
    }
    public override void ThirdLevel()
    {
        _buffChess.findRange = 3;
    }
}


public class Shapeshifter : BuffBasic
{
    public override ChessType buffChess => ChessType.Pawn;
    public override string buffName => "Shapeshifter";
    public bool cantPromotion = false;
    public bool cantKillChessExceptKing = false;
    public bool cantKillKingWhenPawnExist = false;

    public override void ResetBuff()
    {
        cantPromotion = false;
        cantKillChessExceptKing = false;
        cantKillKingWhenPawnExist = false;
    }

    public override void FirstLevel()
    {
        cantPromotion = true;
    }
    public override void SecondLevel()
    {
        cantKillChessExceptKing = true;
    }
    public override void ThirdLevel()
    {
        cantKillKingWhenPawnExist = true;
    }
}



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
