using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sorcerer : BuffBasic
{
    public override ChessType buffChess => ChessType.Bishop;
    public override string buffName => "Sorcerer";

    public HashSet<Vector2Int> extraCanGoArea = new HashSet<Vector2Int>();
    private readonly Vector2Int firstExtraDirections = Vector2Int.up;
    private readonly Vector2Int secondExtraDirections = Vector2Int.down;
    public readonly int extraCanGoRange = 2;

    public bool canCurseChess;

    public override void ResetBuff()
    {
        extraCanGoArea.Clear();
        canCurseChess = false;
    }


    public override void FirstLevel()
    {
        extraCanGoArea.Add(firstExtraDirections);
    }
    public override void SecondLevel()
    {
        extraCanGoArea.Add(secondExtraDirections);

    }
    public override void ThirdLevel()
    {
        canCurseChess = true;
    }

    public void CurseChess()
    {
        if (!canCurseChess) return;
        List<ChessBasic> curseList = new List<ChessBasic>();

        foreach(ChessBasic target in ChessBoard.Instance.board.Values)
        {
            if (target.color != _buffChess.color)
            {
                curseList.Add(target);
            }
        }

        int random_Chess_Index = Random.Range(0, curseList.Count);

        curseList[random_Chess_Index].CurceThisChess();
    }

}

public class Monk : BuffBasic
{
    public override ChessType buffChess => ChessType.Bishop;
    public override string buffName => "Monk";

    public HashSet<Vector2Int> extraCanGoArea = new HashSet<Vector2Int>();
    private readonly Vector2Int firstExtraDirections = Vector2Int.left;
    private readonly Vector2Int secondExtraDirections = Vector2Int.right;
    public readonly int extraCanGoRange = 2;

    public override void ResetBuff()
    {
        extraCanGoArea.Clear();
    }


    public override void FirstLevel()
    {
        extraCanGoArea.Add(firstExtraDirections);
    }
    public override void SecondLevel()
    {
        extraCanGoArea.Add(secondExtraDirections);

    }
    public override void ThirdLevel()
    {
        AddBarrierToKing();
    }

    public void AddBarrierToKing()
    {
        King king;
        Pair<ChessColor, ChessType> kingInform = new Pair<ChessColor, ChessType>(_buffChess.color, ChessType.King);
        foreach (ChessBasic target in ChessBoard.Instance.board.Values)
        {
            if (target.chessInfo == kingInform)
            {
                king = target.GetComponent<King>();
                king.AddBarrier();
                return;
            }
        }

    }


}

public class Bishop : ChessBasic
{
    public override ChessType type => ChessType.Bishop;
    public override string ChessName() { return "Bishop"; }

    public Sorcerer sorcerer = new Sorcerer();
    public Monk monk = new Monk();


    public override void ChessInit()
    {
        sorcerer.BuffInit(this);
        monk.BuffInit(this);
    }


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

        if (sorcerer.extraCanGoArea.Count > 0)
        {
            foreach (var dir in sorcerer.extraCanGoArea)
            {
                for (int i = 1;i <= sorcerer.extraCanGoRange; i++)
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

        if (monk.extraCanGoArea.Count > 0)
        {
            foreach (var dir in monk.extraCanGoArea)
            {
                for (int i = 1; i <= monk.extraCanGoRange; i++)
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
