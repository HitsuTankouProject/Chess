using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Collections;
using static UnityEngine.Analytics.IAnalytic;

public class Charger : BuffBasic
{
    public override ChessType buffChess => ChessType.Knight;
    public override string buffName => "Charger";
    public override void Choose() => _player.knightBuffType = Player.KnightBuff.Charger;


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
    public override void Choose() => _player.knightBuffType = Player.KnightBuff.Skirmisher;


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
    private bool isMoveAgain = false;
    public override int findRange { get; } = 1;
    public override HashSet<Vector2Int> directions => new HashSet<Vector2Int>()
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

    public override void FindCanMove(bool isThrougt)
    {
        base.FindCanMove(isThrougt);
    }
    public override void ExtraFindPossibleMove(bool isthrough)
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

                if (IsOutOfBoard(targetPos)) break;
                if (_chessBoard.board.TryGetValue(targetPos, out ChessBasic chess))
                {
                    if (chess.color != this.color)
                    {
                        possibleMoveList.Add(targetPos);
                    }
                    if(!isthrough) break;
                }
                else
                {
                    possibleMoveList.Add(targetPos);
                }

            }


        }
    }

    public override void Move(Vector2Int moveTo)
    {
        Player.KnightBuff knightBuff = _player.knightBuffType;

        if (knightBuff == Player.KnightBuff.None)
        {
            base.Move(moveTo);
            return;
        }

        bool canMove = CanMoveTo(moveTo, out ChessBasic posHaveChess);
        bool isEatTheChess = posHaveChess != null;
        if (!canMove)
        {
            _player.nowPlayerStage = PlayerStage.ReadytoEnd;
            return;
        }
        MoveOnly(moveTo);

        if (knightBuff == Player.KnightBuff.Skirmisher) SkirmisherFinalBuff();
        else if (knightBuff == Player.KnightBuff.Charger) ChargerFinalBuff(isEatTheChess);
       
    }



    private void ChargerFinalBuff(bool moveAgain)
    {
        bool haveChargerFinalBuff = _player.knightBuffType == Player.KnightBuff.Charger && _player.charger.canMoveItAgain;

        if (!haveChargerFinalBuff || !moveAgain)
        {
            _player.nowPlayerStage = PlayerStage.ReadytoEnd;
            return;
        }


        if (!isMoveAgain)
        {
            isMoveAgain = true;
            StartCoroutine(_player.playerInPut.OneMoreMove(this));
        }
        else
        {
            isMoveAgain = false;
            _player.nowPlayerStage = PlayerStage.ReadytoEnd;

        }


    }
    private void SkirmisherFinalBuff()
    {
        if (_player.knightBuffType != Player.KnightBuff.Skirmisher) return;
        if (_player.skirmisher.nowBuffLevel != 3) return;

        Queue<ChessBasic> eatQueue = new Queue<ChessBasic>();

        foreach(Vector2Int dir in _player.skirmisher.extraCanGoArea)
        {
            Vector2Int targetPos = position + dir;
            if (_chessBoard.board.TryGetValue(targetPos, out ChessBasic chess))
            {
                if (chess.color != this.color)
                {
                    eatQueue.Enqueue(chess);
                }
            }

        }

        while (eatQueue.Count>0) eatQueue.Dequeue().GotEaten();

        _player.nowPlayerStage = PlayerStage.ReadytoEnd;
    }

}
