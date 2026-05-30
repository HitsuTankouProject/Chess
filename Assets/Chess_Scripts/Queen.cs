using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Witcher : BuffBasic
{
    public override ChessType buffChess => ChessType.Queen;
    public override string buffName => "Witcher";
    public override void Choose() => _player.queenBuffType = Player.QueenBuff.Witcher;

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
    public override void Choose() => _player.queenBuffType = Player.QueenBuff.Beauty;

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
    public override int findRange { get; } = 8;
    private int witcherFindRange => _player.witcher.canGoRange;

    public override HashSet<Vector2Int> directions => new HashSet<Vector2Int>()
    { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

    public override void FindPossibleMove()
    {
        possibleMoveList.Clear();
        possibleEatList.Clear();

        int range = _player.queenBuffType == Player.QueenBuff.Witcher ? witcherFindRange : findRange;

        foreach (var dir in directions)
        {
            for (int i = 1; i < range; i++)
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

    private void WitcherBuff()
    {
        if (_player.witcher.nowBuffLevel < 2) return;




    }
    private void Witcher_Move(Vector2Int moveTo)
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
        // ワールド座標へ移動
        this.transform.position = _chessBoard.ReturnChessBlockPosition(moveTo);
        _chessBoard.BoardUpdate(this, moveTo, ChessAction.Move);

        if (_player.IsProTectedByRook_Guardian(position))
        {
            haveExtraLife = true;
        }
        else haveExtraLife = false;





        _player.nowPlayerStage = PlayerStage.ReadytoEnd;

    }
    private void Beauty_Move(Vector2Int moveTo)
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
        // ワールド座標へ移動
        this.transform.position = _chessBoard.ReturnChessBlockPosition(moveTo);
        // 盤面情報更新
        _chessBoard.BoardUpdate(this, moveTo, ChessAction.Move);

        if (_player.IsProTectedByRook_Guardian(position))
        {
            haveExtraLife = true;
        }
        else haveExtraLife = false;
        _player.nowPlayerStage = PlayerStage.ReadytoEnd;
    }

    public override void Move(Vector2Int moveTo)
    {
        Player.QueenBuff queenBuff = _player.queenBuffType;
        switch (queenBuff)
        {
            case Player.QueenBuff.None:base.Move(moveTo); break;
            case Player.QueenBuff.Witcher: Witcher_Move(moveTo); break;
            case Player.QueenBuff.Beauty: Beauty_Move(moveTo); break;
        }

    }



}
