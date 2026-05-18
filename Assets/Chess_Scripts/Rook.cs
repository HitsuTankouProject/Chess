using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Rusher : BuffBasic
{
    public override ChessType buffChess => ChessType.Rook;
    public override string buffName => "Rusher";

    public bool canThroughSameColor = true;
    public bool canThroughNonSameColor = true;
    public bool canEatThroughNonSameColorChess = false;

    public override void ResetBuff()
    {
        canThroughSameColor = false;
        canThroughNonSameColor = false;
        canEatThroughNonSameColorChess = false;
    }
    public override void FirstLevel()
    {
        canThroughSameColor = true;
    }
    public override void SecondLevel()
    {
        canThroughNonSameColor = true;
    }
    public override void ThirdLevel()
    {
        canEatThroughNonSameColorChess = true;
    }
}

public class Guardian : BuffBasic
{
    public override ChessType buffChess => ChessType.Rook;
    public override string buffName => "Guardian";

    public HashSet<Vector2Int> protectArea = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> firstProtectArea = new HashSet<Vector2Int>() 
    {
        Vector2Int.left, Vector2Int.right
    };
    private readonly HashSet<Vector2Int> secondProtectArea = new HashSet<Vector2Int>()
    {
        Vector2Int.up, Vector2Int.down
    };
    private readonly HashSet<Vector2Int> thridProtectArea = new HashSet<Vector2Int>() 
    {
        new Vector2Int(-1, 1), 
        new Vector2Int(1, 1) , 
        new Vector2Int(-1, 1), 
        new Vector2Int(-1, -1) 
    
    };

    public override void BuffInit(ChessBasic target)
    {
        base.BuffInit(target);
    }

    public override void ResetBuff()
    {
        protectArea = new HashSet<Vector2Int>();
    }

    public override void FirstLevel()
    {
        protectArea.AddRange(firstProtectArea);
    }
    public override void SecondLevel()
    {
        protectArea.AddRange(secondProtectArea);
    }
    public override void ThirdLevel()
    {
        protectArea.AddRange(thridProtectArea);
    }
}


public class Rook : ChessBasic
{
    public override ChessType type => ChessType.Rook;

    public override string ChessName() { return "Rook"; }

    private readonly List<Vector2Int> directions = new List<Vector2Int>
    { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    private Rusher rusherBuff = new Rusher();
    private Guardian guardianBuff = new Guardian();

    public override void ChessInit()
    {
        rusherBuff.BuffInit(this);
        guardianBuff.BuffInit(this);
    }


    private enum MoveJudgment
    {
        Stop, KeepGoOn, CanEatAndStop,CanEatAndThrough, Error
    }
    MoveJudgment MoveJudgmentResult(ChessBasic chess)
    {
        if(!rusherBuff.canThroughSameColor)return MoveJudgment.Stop;
        bool sameColor = chess.color == color;
        if (sameColor) return MoveJudgment.KeepGoOn;
        else if (!sameColor && !rusherBuff.canThroughNonSameColor) return MoveJudgment.CanEatAndStop;
        else if(!sameColor && rusherBuff.canThroughNonSameColor) return MoveJudgment.CanEatAndThrough;

        return MoveJudgment.Error;

    }

    public override void FindPossibleMove()
    {
        possibleMoveList.Clear();
        canEatChessPosition.Clear();


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
                    MoveJudgment judgment = MoveJudgmentResult(chess);

                    if (judgment == MoveJudgment.Stop) break;
                    else if(judgment == MoveJudgment.KeepGoOn) continue;
                    else if (judgment == MoveJudgment.CanEatAndStop)
                    {
                        possibleMoveList.Add(targetPos);
                        canEatChessPosition.Add(targetPos);
                        break;
                    }
                    else if (judgment == MoveJudgment.CanEatAndThrough)
                    {
                        possibleMoveList.Add(targetPos);
                        canEatChessPosition.Add(targetPos);
                        continue;
                    }

                }
                else
                {
                    possibleMoveList.Add(targetPos);
                }
            }
        }

    _chessBoard.ShowCanGo(possibleMoveList);

    }

    public bool GuardianBuff(ChessBasic gotEatenChess)
    {
        if (guardianBuff.protectArea.Count == 0) return false;
        foreach(Vector2Int protectSpot in guardianBuff.protectArea)
        {
            Vector2Int target = position + protectSpot;
            if(gotEatenChess.position== target)
            {
                return true;
            }

        }

        return false;
    }



}
