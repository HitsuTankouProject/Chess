using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Rusher : BuffBasic
{
    public override ChessType buffChess => ChessType.Rook;
    public override string buffName => "Rusher";
    public override void Choose() => _player.allTheBuff.rookBuffType = RookBuff.Rusher;
    
    public bool canThroughSameColor = false;
    public bool canThroughNonSameColor = false;
    public int findRange = 8;
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
        findRange = 4;
        canEatThroughNonSameColorChess = true;
    }
}

public class Guardian : BuffBasic
{
    public override ChessType buffChess => ChessType.Rook;
    public override string buffName => "Guardian";
    public override void Choose() => _player.allTheBuff.rookBuffType = RookBuff.Guardian;


    public HashSet<Vector2Int> protectArea = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> firstProtectArea = new HashSet<Vector2Int>() 
    {
        Vector2Int.left, Vector2Int.right
    };
    private readonly HashSet<Vector2Int> secondProtectArea = new HashSet<Vector2Int>()
    {
        Vector2Int.up, Vector2Int.down
    };
    private readonly HashSet<Vector2Int> thirdProtectArea = new HashSet<Vector2Int>() 
    {
        new Vector2Int(-1, 1),
        new Vector2Int(1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1, -1)

    };

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
        protectArea.AddRange(thirdProtectArea);
    }


}


public class Rook : ChessBasic
{
    public override ChessType type => ChessType.Rook;

    public override string ChessName() { return "Rook"; }
    public override int findRange { get; } = 8;

    public override HashSet<Vector2Int> directions => new HashSet<Vector2Int>
    { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    private bool CanThrough(ChessColor color)
    {
        bool canThroughSameColor = _player.rusher.canThroughSameColor;
        bool canThroughNonSameColor = _player.rusher.canThroughNonSameColor;
        if (!canThroughSameColor && !canThroughNonSameColor) return false;

        if(color == this.color&& canThroughSameColor) return true;
        else if (color != this.color && canThroughNonSameColor) return true;

        return false;


    }
    private void Rusher_FindCanMove()
    {
        foreach (var dir in directions)
        {
            for (int i = 1; i <=_player.rusher.findRange; i++)
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
                    if (!CanThrough(chess.color)) break;
                }
                else
                {
                    possibleMoveList.Add(targetPos);
                }
            }
        }

    }
    public void GuardianBuff()
    {
        if (_player.rookBuffType != RookBuff.Guardian || _player.guardian.protectArea.Count == 0) return;
        foreach (Vector2Int protectedDir in _player.guardian.protectArea)
        {
            Vector2Int spot = position + protectedDir;
            if (_chessBoard.IsOutOfBoard(spot)) continue;
            if (!_chessBoard.board.TryGetValue(spot, out ChessBasic chess)
                || chess.color != this.color) continue;
            _player.AddGuardianProtectedChess(chess);
        }
    }

    public override void FindCanMove(bool isThrough)
    {
        if (_player.rookBuffType != RookBuff.Rusher) base.FindCanMove(isThrough);
        else Rusher_FindCanMove();
    }

    private void Rusher_MoveTo(Vector2Int moveTo)
    {
        if (!_player.rusher.canEatThroughNonSameColorChess)
        {
            base.Move(moveTo);
            return;
        }
        bool canMoveTo = CanMoveTo(moveTo, out ChessBasic chessCanMoveTo);
        Vector2Int nowPosition = position;
        ReturnPick();

        if (!canMoveTo)
        {
            _player.nowPlayerStage = PlayerStage.ReadytoEnd;
            return;
        }

        Queue<ChessBasic> eatqueue = new Queue<ChessBasic>();
        Vector2Int dir = new Vector2Int(Math.Sign(moveTo.x - nowPosition.x), Math.Sign(moveTo.y - nowPosition.y));

        while(nowPosition!= moveTo)
        {
            nowPosition += dir;
            bool posHaveChess = _chessBoard.board.TryGetValue(nowPosition, out ChessBasic chess);
            if (posHaveChess && chess.color != color && !chess.haveExtraLife) eatqueue.Enqueue(chess);
        }
        MoveOnly(moveTo);

        while (eatqueue.Count > 0)
        {
            ChessBasic chess = eatqueue.Dequeue();
            _chessBoard.DeadEffect(chess);
            chess.GotEaten();
        }

        _player.nowPlayerStage = PlayerStage.ReadytoEnd;
    }


    public override void Move(Vector2Int moveTo)
    {
        if (_player.rookBuffType == RookBuff.None)
        {
            base.Move(moveTo);
            return;
        }
        else if(_player.rookBuffType == RookBuff.Rusher)
        {
            Rusher_MoveTo(moveTo);
        }
        else if (_player.rookBuffType == RookBuff.Guardian)
        {
            MoveOnly(moveTo);
            _player.UpdateGuardianProtectArea();
            _player.nowPlayerStage = PlayerStage.ReadytoEnd;
            return;
        }

    }

}
