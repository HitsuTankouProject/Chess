using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Witcher : BuffBasic
{
    public override ChessType buffChess => ChessType.Queen;
    public override string buffName => "Witcher";

    public int canGoRange { get; private set; } = 3;
    public bool cantGotCurse { get; private set; } = false;
    public bool canCurseBlock { get; private set; } = false;
    public bool canCurseAllTheBlockCanGo { get; private set; } = false;
    private HashSet<Vector2Int> curseAllTheBlockCanGoPos = new HashSet<Vector2Int>();

    public override void ResetBuff()
    {
        cantGotCurse = false;
        canCurseBlock = false;
        canCurseAllTheBlockCanGo = false;
    }

    public override void FirstLevel()
    {
        cantGotCurse = true;
    }
    public override void SecondLevel()
    {
        canCurseBlock = true;
    }

    public override void ThirdLevel()
    {
        canCurseAllTheBlockCanGo = true;
    }

    public void CurseAllTheBlockCanGo()
    {
        curseAllTheBlockCanGoPos.Clear();



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

    public override List<Vector2Int> directions => new List<Vector2Int>()
    { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

    public override void FindPossibleMove()
    {

        possibleMoveList.Clear();
        possibleEatList.Clear();
        foreach (var dir in directions)
        {
            for (int i = 1; i < 8; i++)
            {
                Vector2Int targetPos = position + dir * i;

                if (IsOutOfBoard(targetPos)) break;


                if (_chessBoard.board.TryGetValue(targetPos, out ChessBasic chess))
                {
                    if (chess.color != this.color)
                    {
                        possibleMoveList.Add(targetPos);
                        possibleEatList.Add(targetPos);
                    }

                    break;
                }
                else
                {
                    possibleMoveList.Add(targetPos);
                }
            }
        }

        _chessBoard.ShowActive(ChessBlockStage.CanGo, possibleMoveList);
        _chessBoard.ShowActive(ChessBlockStage.CanEat, possibleEatList);
    }
}
