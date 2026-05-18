using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Charger : BuffBasic
{
    public override ChessType buffChess => ChessType.Knight;
    public override string buffName => "Charger";

    public HashSet<Vector2Int> extraCanGoArea = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> firstExtraArea = new HashSet<Vector2Int>()
    {
        new Vector2Int(0,1),new Vector2Int(0,-1),
    };
    public int extraCanGoRange = 1;

    public bool canMoveItAgain {  get; private set; } = false;

    public override void ResetBuff()
    {
        extraCanGoRange = 1;
        extraCanGoArea.Clear();
    }

    public override void FirstLevel()
    {
        extraCanGoArea.AddRange(firstExtraArea);
    }
    public override void SecondLevel()
    {
        extraCanGoRange = 3;
    }

    public override void ThirdLevel()
    {
        canMoveItAgain = true;
    }

}

public class Skirmisher : BuffBasic
{
    public override ChessType buffChess => ChessType.Knight;
    public override string buffName => "Skirmisher";

    public HashSet<Vector2Int> extraCanGoArea = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> firstExtraArea = new HashSet<Vector2Int>()
    {
        new Vector2Int(1,0),new Vector2Int(-1,0),
    };
    public int extraCanGoRange = 1;

    public bool canEatNextChess { get; private set; } = false;


    public override void ResetBuff()
    {
        extraCanGoRange = 1;
        extraCanGoArea.Clear();
    }

    public override void FirstLevel()
    {
        extraCanGoArea.AddRange(firstExtraArea);
    }
    public override void SecondLevel()
    {
        extraCanGoRange = 3;
    }

    public override void ThirdLevel()
    {
        canEatNextChess = true;
    }

}


public class Knight : ChessBasic
{
    public override ChessType type => ChessType.Knight;
    public override string ChessName() { return "Knight"; }
    public Charger charger = new Charger();
    public Skirmisher skirmisher = new Skirmisher();

    public override void ChessInit()
    {
        charger.BuffInit(this);
        skirmisher.BuffInit(this);

    }

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
                    canEatChessPosition.Add(targetPos);
                }
            }
            else
            {
                possibleMoveList.Add(targetPos);
            }
        }

        if (charger.extraCanGoArea.Count > 0)
        {
            foreach (var dir in charger.extraCanGoArea)
            {
                for (int i = 1; i <= charger.extraCanGoRange; i++)
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
                            canEatChessPosition.Add(targetPos);
                        }
                        break;
                    }
                    else
                    {
                        possibleMoveList.Add(targetPos);
                    }
                }
            }
        }
        if (skirmisher.extraCanGoArea.Count > 0)
        {
            foreach (var dir in skirmisher.extraCanGoArea)
            {
                for (int i = 1; i <= skirmisher.extraCanGoRange; i++)
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
                            canEatChessPosition.Add(targetPos);
                        }
                        break;
                    }
                    else
                    {
                        possibleMoveList.Add(targetPos);
                    }
                }
            }
        }

        _chessBoard.ShowCanGo(possibleMoveList);
    }

}
