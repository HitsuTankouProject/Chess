using System.Collections.Generic;
using UnityEngine;

public class Witcher : BuffBasic
{
    public override ChessType buffChess => ChessType.Queen;
    public override string buffName => "Witcher";

    public bool canChangeToKing = false;
    public bool canCallKingToProtect = false;
    public King king;

    public override void ResetBuff()
    {
        //canChangeToKing = false;
        //canCallKingToProtect = false;
        //Pair<ChessColor, ChessType> kingInform = new Pair<ChessColor, ChessType>(_buffChess.color, ChessType.King);
        //foreach (ChessBasic target in ChessBoard.Instance.board.Values)
        //{
        //    if (target.chessInfo == kingInform)
        //    {
        //        king = target.GetComponent<King>();
        //        return;
        //    }
        //}
    }

    public override void FirstLevel()
    {
        //king.findRange = 2;
    }
    public override void SecondLevel()
    {
        canChangeToKing = true;
    }

    public override void ThirdLevel()
    {
        canCallKingToProtect = true;
    }

}

public class Beauty : BuffBasic
{
    public override ChessType buffChess => ChessType.Queen;
    public override string buffName => "Beauty";

    public bool canProtectByKnight = false;
    public bool removeTheAreaLimit = false;
    public bool canCharmChess = false;

    public override void ResetBuff()
    {

        canProtectByKnight = false;
        removeTheAreaLimit = false;
        canCharmChess = false;
    }


    public override void FirstLevel()
    {
        canProtectByKnight = true;

    }
    public override void SecondLevel()
    {
        removeTheAreaLimit = true;

    }

    public override void ThirdLevel()
    {
        canCharmChess = true;
    }

}


public class Queen : ChessBasic
{
    public override ChessType type => ChessType.Queen;
    public override string ChessName() { return "Queen"; }
    public override int findRange { get; protected set; } = 8;

    public Witcher witcher = new Witcher();
    public Beauty beauty = new Beauty();

    public override List<Vector2Int> directions => new List<Vector2Int>()
    { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

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
