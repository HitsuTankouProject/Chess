using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static Player;

public class Rusher : BuffBasic
{
    public override ChessType buffChess => ChessType.Rook;
    public override string buffName => "Rusher";

    public bool canThroughSameColor = false;
    public bool canThroughNonSameColor = false;
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
        protectArea.UnionWith(firstProtectArea);
    }
    public override void SecondLevel()
    {
        protectArea.UnionWith(secondProtectArea);
    }
    public override void ThirdLevel()
    {
        protectArea.UnionWith(thirdProtectArea);
    }

    public bool InProtectArea(Vector2Int targetPos)
    {
        if (_player.rookBuffType != RookBuff.Guardian) return false;
        if (!_player.allTheChess.TryGetValue(ChessType.Rook, out List<ChessBasic> rooks)) return false;
        if (rooks.Count == 0) return false;

        for (int i = 0; i < rooks.Count; i++)
        {
            foreach (Vector2Int direction in protectArea)
            {
                Vector2Int protectPos = rooks[i].position + direction;

                if (targetPos == protectPos)
                {
                    return true;
                }
            }
        }

        return false;

    }

}


public class Rook : ChessBasic
{
    public override ChessType type => ChessType.Rook;

    public override string ChessName() { return "Rook"; }
    public override int findRange { get; protected set; } = 8;

    public override List<Vector2Int> directions => new List<Vector2Int>
    { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    private enum MoveJudgment
    {
        Stop, KeepGoOn, CanEatAndStop,CanEatAndThrough, Error
    }
    MoveJudgment MoveJudgmentResult(ChessBasic chess)
    {
        bool sameColor = chess.color == color;
        bool isRusher = _player.rookBuffType == RookBuff.Rusher;
        if (!isRusher)
        {
            if (sameColor) return MoveJudgment.Stop;
            else return MoveJudgment.CanEatAndStop;
        }

        bool canThroughSameColor = _player.rusher.canThroughSameColor;
        bool canThroughNonSameColor = _player.rusher.canThroughNonSameColor;

        if (sameColor&&canThroughSameColor) return MoveJudgment.KeepGoOn;
        else if (!sameColor && !canThroughNonSameColor) return MoveJudgment.CanEatAndStop;
        else if (!sameColor && canThroughNonSameColor) return MoveJudgment.CanEatAndThrough;
        return MoveJudgment.Error;

    }
    public bool IsProtectedByGuardian(Vector2Int targetChessPos)
    {
        if (_player.rookBuffType != RookBuff.Guardian || _player.guardian.protectArea.Count == 0) return false;
        foreach (Vector2Int protectedDir in _player.guardian.protectArea)
        {
            Vector2Int spot = position + protectedDir;
            if (spot == targetChessPos) return true;
        }
        return false;

    }

    public override void FindPossibleMove()
    {
        possibleMoveList.Clear();
        possibleEatList.Clear();


        foreach (var dir in directions)
        {
            for (int i = 1; i < findRange; i++)
            {
                Vector2Int targetPos = position + dir * i;
                if (IsOutOfBoard(targetPos)) break;

                if (_chessBoard.board.TryGetValue(targetPos, out ChessBasic chess))
                {
                    MoveJudgment judgment = MoveJudgmentResult(chess);

                    if (judgment == MoveJudgment.Stop) break;
                    else if (judgment == MoveJudgment.KeepGoOn) continue;
                    else if (judgment == MoveJudgment.CanEatAndStop)
                    {
                        possibleMoveList.Add(targetPos);
                        possibleEatList.Add(targetPos);

                        break;
                    }
                    else if (judgment == MoveJudgment.CanEatAndThrough)
                    {
                        possibleMoveList.Add(targetPos);
                        possibleEatList.Add(targetPos);

                        continue;
                    }

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

    public void ReMoveGuardianBuff(Vector2Int beforeMoveTo)
    {
        if (_player.rookBuffType != RookBuff.Guardian || _player.guardian.protectArea.Count == 0) return;

        HashSet<Vector2Int> protectedSpots = new HashSet<Vector2Int>();

        foreach (Vector2Int protectedDir in _player.guardian.protectArea)
        {
            Vector2Int spot = beforeMoveTo + protectedDir;

            bool haveChess = _chessBoard.board.TryGetValue(spot, out ChessBasic chess);

            if (!haveChess) continue;
            if (chess.color != color) continue;

            protectedSpots.Add(spot);
        }

        foreach (Vector2Int protectedSpot in protectedSpots)
        {
            bool stillProtected = false;

            foreach (ChessBasic rook in _player.allTheChess[ChessType.Rook])
            {
                if (rook == this) continue;

                foreach (Vector2Int protectDir in _player.guardian.protectArea)
                {
                    Vector2Int protectPos = rook.position + protectDir;

                    if (protectPos == protectedSpot)
                    {
                        stillProtected = true;
                        break;
                    }
                }

                if (stillProtected) break;
            }

            if (!stillProtected)
            {
                ChessBasic targetChess = _chessBoard.board[protectedSpot];

                targetChess.haveExtraLife= false;
            }
        }
    }

    public void GuardianBuff()
    {
        if (_player.rookBuffType != RookBuff.Guardian || _player.guardian.protectArea.Count == 0) return;

        foreach(Vector2Int protectedDir in _player.guardian.protectArea)
        {
            Vector2Int spot = position + protectedDir;
            bool haveChess = _chessBoard.board.TryGetValue(spot, out ChessBasic chess);
            if (!haveChess) continue;
            if (chess.color == this.color) chess.haveExtraLife = true;


        }


    }


    private void RusherBuffMove(Vector2Int moveTo)
    {
        _player.nowPlayerStage = PlayerStage.MovingChess;
        ReturnPick();
        if (!CanMoveTo(moveTo, out ChessBasic chessCanMoveTo))
        {
            _player.nowPlayerStage = PlayerStage.ReadytoEnd;
            return;
        }
        Queue<ChessBasic> eatqueue = new Queue<ChessBasic>();
        Vector2Int dir = new Vector2Int(Math.Sign(moveTo.x - position.x), Math.Sign(moveTo.y - position.y));
        Vector2Int targetPos = position;

        for (int i = 1; targetPos != moveTo; i++)
        {
            targetPos = position + dir * i;
            bool posHaveChess = _chessBoard.board.TryGetValue(targetPos, out ChessBasic chess);

            if (posHaveChess && chess != null && chess.color != color)
            {
                eatqueue.Enqueue(chess);
            }
        }
        while (eatqueue.Count > 0)
        {
            ChessBasic chessBasic = eatqueue.Dequeue();
            bool chessHaveBeenProtected = chessBasic.haveExtraLife;

            if (chessHaveBeenProtected) continue;
            else chessBasic.GotEaten();
        }
        this.transform.position = _chessBoard.ReturnChessBlockPosition(moveTo);
        _chessBoard.BoardUpdate(this, moveTo, ChessAction.Move);

        _player.nowPlayerStage = PlayerStage.ReadytoEnd;
    }

    public void BasicMove(Vector2Int moveTo)
    {
        _player.nowPlayerStage = PlayerStage.MovingChess;
        ReturnPick();

        if (!CanMoveTo(moveTo, out ChessBasic chess))
        {
            _player.nowPlayerStage = PlayerStage.ReadytoEnd;
            return;
        }
        else
        {
            if (chess != null)
            {
                _player.nowPlayerStage = PlayerStage.EatingChess;
                chess.GotEaten();
            }
        }

        ReMoveGuardianBuff(position);
        // ワールド座標へ移動
        this.transform.position = _chessBoard.ReturnChessBlockPosition(moveTo);
        // 盤面情報更新
        _chessBoard.BoardUpdate(this, moveTo, ChessAction.Move);
        GuardianBuff();
    }


    public override void Move(Vector2Int moveTo)
    {
        if(_player.rookBuffType != RookBuff.Rusher|| !_player.rusher.canThroughNonSameColor)
        {
            BasicMove(moveTo);
            _player.nowPlayerStage = PlayerStage.ReadytoEnd;
            return;
        }

        else if (_player.rookBuffType == RookBuff.Rusher&& _player.rusher.canThroughNonSameColor)
        {
            RusherBuffMove(moveTo);
            _player.nowPlayerStage = PlayerStage.ReadytoEnd;
        }
        

    }

    public override void GotEaten()
    {
        ReMoveGuardianBuff(position);
        base.GotEaten();
    }
}
