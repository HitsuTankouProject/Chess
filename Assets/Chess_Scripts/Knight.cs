using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Collections;
using static UnityEngine.Analytics.IAnalytic;

public class Charger : BuffBasic
{
    public override ChessType buffChess => ChessType.Knight;
    public override string buffName => "Charger";

    public HashSet<Vector2Int> extraCanGoArea = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> firstExtraArea = new HashSet<Vector2Int>()
    {
        new Vector2Int(0,1),new Vector2Int(0,-1),
    };
    public int extraCanGoRange { get; private set; } = 1;

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
    public int extraCanGoRange { get; private set; } = 1;

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
    public override int findRange { get; protected set; } = 1;

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

    public override void ExtraFindPossibleMove()
    {
        if (_player.knightBuffType == Player.KnightBuff.None) return;

        HashSet<Vector2Int> extraCanGoArea = _player.knightBuffType == Player.KnightBuff.Charger ?
            _player.charger.extraCanGoArea : _player.skirmisher.extraCanGoArea;

        int extraCanGoRange = _player.knightBuffType == Player.KnightBuff.Charger ?
            _player.charger.extraCanGoRange : _player.skirmisher.extraCanGoRange;

        foreach (var dir in extraCanGoArea)
        {
            for (int i = 1; i <= extraCanGoRange; i++)
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

        ExtraFindPossibleMove();

        _chessBoard.ShowCanGo(possibleMoveList);
    }

    public void ChargerFinalBuff()
    {
        if (_player.knightBuffType != Player.KnightBuff.Charger) return;
        if (_player.charger.nowBuffLevel != 3) return;






    }
    public void ChargerSkirmisherBuff()
    {
        if (_player.knightBuffType != Player.KnightBuff.Skirmisher) return;
        if (_player.skirmisher.nowBuffLevel != 3) return;

        Queue<ChessBasic> eatqueue = new Queue<ChessBasic>();

        foreach(Vector2Int dir in _player.skirmisher.extraCanGoArea)
        {
            Vector2Int targetPos = position + dir;
            if (_chessBoard.board.TryGetValue(targetPos, out ChessBasic chess))
            {
                if (chess.color != this.color)
                {
                    eatqueue.Enqueue(chess);
                }
            }

        }

        while (eatqueue.Count>0) eatqueue.Dequeue().GotEaten();

    }

}
