using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Witcher : BuffBasic
{
    public override ChessType buffChess => ChessType.Queen;
    public override string buffName => "Witcher";
    public override void Choose() => _player.queenBuffType = Player.QueenBuff.Witcher;

    public int canGoRange { get; private set; } = 2;
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
    public override int findRange
    {
        get
        {
            if(_player.queenBuffType == Player.QueenBuff.Witcher)
                return _player.witcher.canGoRange;
             else return 8;
        }
    }

    public override HashSet<Vector2Int> directions => new HashSet<Vector2Int>()
    { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

    
    public override void EatChess(ChessBasic chess)
    {
        if (chess == null) return;
        Vector2Int thisPos = position;
        Vector2Int chessPos = chess.position;
        ChessType chessType = chess.type;

        base.EatChess(chess);

        if (_player.queenBuffType == Player.QueenBuff.Witcher) CurseBlock();
        else if (_player.queenBuffType == Player.QueenBuff.Beauty) CharmChess(chessType, thisPos);

        Player.QueenBuff queenBuff = _player.queenBuffType;
        
    }

    public override void Move(Vector2Int moveTo)
    {
        _player.nowPlayerStage = PlayerStage.MovingChess;
        ReturnPick();
        if (!CanMoveTo(moveTo, out ChessBasic chess))
        {
            _player.nowPlayerStage = PlayerStage.ReadytoEnd;
            return;
        }
        if (chess == null) _chessBoard.MoveTo(this, moveTo);
        else
        {
            EatChess(chess);

        }

        if (_player.IsProTectedByRook_Guardian(position))
            GotExtraLife(true);
        else GotExtraLife(false);
        _player.nowPlayerStage = PlayerStage.ReadytoEnd;
    }




    private const float canCharmPercent = 50.0f;
    private bool CanProtectByKnight(out ChessBasic knight)
    {
        knight = null;
        if (_player.queenBuffType != Player.QueenBuff.Beauty) return false;
        List<ChessBasic> knightList = _player.ChessListByType(ChessType.Knight);
        if (knightList.Count == 0) return false;

        if(_player.beauty.nowBuffLevel == 1)
        {
            foreach (ChessBasic chess in knightList)
            {
                HashSet<Vector2Int> knightPossibleMove = chess.PossibleMove(false);

                if (knightPossibleMove.Contains(position))
                {
                    knight = chess;
                    return true;
                }
            }
        }
        else
        {
            knight = knightList[Random.Range(0, knightList.Count)];
            return true;
        }
        return false;

    }
    private void ProtectByKnight(ChessBasic knight)
    {
        SwapPosition(knight);
        knight.GotEaten();
    }
    private void CharmChess(ChessType chessType, Vector2Int spawnKnightPos)
    {
        if (_player.queenBuffType != Player.QueenBuff.Beauty ||
            !_player.beauty.canCharmChess
            || _chessBoard.IsKingChessSpawn(spawnKnightPos)) return;

        if (chessType != ChessType.Knight)
        {
            float isCanCharm = Random.Range(0.0f, 100.0f);
            if (isCanCharm > canCharmPercent) return;
        }

        Pair<ChessColor, ChessType> promotionInfo = new Pair<ChessColor, ChessType>(color, ChessType.Knight);
        _chessBoard.GenChess(spawnKnightPos, promotionInfo, out ChessBasic genChess);

        Debug.Log(genChess.name + " : " + spawnKnightPos);
        Debug.Log(_chessBoard.board[spawnKnightPos].gameObject.name);

        if (genChess != null) genChess.ChessInit(_player);

    }

    private void CurseBlock()
    {
        if (_player.queenBuffType != Player.QueenBuff.Witcher) return;
        if (_player.witcher.nowBuffLevel == 2)
        {
            _chessBoard.ChessBlock(position).CurseTheBlock(this);
        }
        else if(_player.witcher.nowBuffLevel == 3)
        {
            HashSet<Vector2Int> cursePossibleMove = PossibleMove(false);
            foreach (Vector2Int pos in cursePossibleMove)
            {
                _chessBoard.ChessBlock(pos).CurseTheBlock(this);

            }
        }

    }

    public override void GotEaten()
    {
        if (!CanProtectByKnight(out ChessBasic knight)) base.GotEaten();
        else ProtectByKnight(knight);

    }



}
