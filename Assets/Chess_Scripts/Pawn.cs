using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;


public class Scout : BuffBasic
{
    public override ChessType buffChess => ChessType.Pawn;
    public override string buffName => "Scout";

    public bool cantPromotion = false;
    public bool canReceiveMoveAreaFromYouAteChess = false;
    public List<Vector2Int> extraMoveArea = new List<Vector2Int>();
    public int extraMoveRange { get; private set; } = 1;

    public override void ResetBuff()
    {
        cantPromotion = false;
        canReceiveMoveAreaFromYouAteChess = false;
        extraMoveArea.Clear();
        extraMoveRange = 1;
    }

    public override void FirstLevel()
    {
        cantPromotion = true;
    }
    public override void SecondLevel()
    {
        canReceiveMoveAreaFromYouAteChess = true;
    }
    public override void ThirdLevel()
    {
        extraMoveRange = 3;
    }

    public void AddExtraMoveArea(ChessBasic capturedChess)
    {
        foreach (Vector2Int moveDirection in capturedChess.directions)
        {
            if (!extraMoveArea.Contains(moveDirection))
            {
                extraMoveArea.Add(moveDirection);
            }
        }
    }

}


public class Substitute : BuffBasic
{
    public override ChessType buffChess => ChessType.Pawn;
    public override string buffName => "Substitute";
    public bool cantPromotion = false;
    public bool canOnlyKillKing = false;
    public bool cantKillKingWhenPawnExist = false;

    public override void ResetBuff()
    {
        cantPromotion = false;
        canOnlyKillKing = false;
        cantKillKingWhenPawnExist = false;
    }

    public override void FirstLevel()
    {
        cantPromotion = true;
    }
    public override void SecondLevel()
    {
        canOnlyKillKing = true;
    }
    public override void ThirdLevel()
    {
        cantKillKingWhenPawnExist = true;
    }
}



public class Pawn : ChessBasic
{
    public override ChessType type => ChessType.Pawn;
    private bool isFirstMove = true;
    public override string ChessName() { return "Pawn"; }
    public override int findRange { get; protected set; } = 1;

    public override List<Vector2Int> directions => new List<Vector2Int>()
    {
        Vector2Int.up
    };
    private List<Vector2Int> attackDirs = new List<Vector2Int>
    { new Vector2Int(1, 1), new Vector2Int(-1, 1) };

    public override void ExtraFindPossibleMove()
    {
        if (_player.pawnBuffType != Player.PawnBuff.Scout
            || _player.scout.extraMoveArea.Count == 0) return;

        foreach (Vector2Int direction in _player.scout.extraMoveArea)
        {
            for (int i = 1; i <= _player.scout.extraMoveRange; i++)
            {
                Vector2Int targetPosition = position + direction * i;
                bool haveChess = _chessBoard.board.TryGetValue(targetPosition, out ChessBasic chess);
                if (!haveChess)
                {
                    possibleMoveList.Add(targetPosition);
                    continue;
                }

                if (chess.color != this.color)
                {
                    possibleMoveList.Add(targetPosition);
                    canEatChessPosition.Add(targetPosition);
                }

                break;
            }
        }


    }

    public override void FindPossibleMove()
    {
        possibleMoveList.Clear();
        canEatChessPosition.Clear();

        int moveDirectionValue =(color == ChessColor.White) ? 1 : -1;
        findRange = isFirstMove ? 2 : 1;

        foreach (Vector2Int moveDirection in directions)
        {
            Vector2Int moveOffset = moveDirection * moveDirectionValue;
            for (int distance = 1; distance <= findRange; distance++)
            {
                Vector2Int targetPosition = position + moveOffset * distance;
                bool haveChess = _chessBoard.board.ContainsKey(targetPosition);

                if (!haveChess) possibleMoveList.Add(targetPosition);
                else break;
            }
        }

        foreach (Vector2Int attackDirection in attackDirs)
        {
            Vector2Int targetPosition = position + attackDirection * moveDirectionValue;
            bool haveChess = _chessBoard.board.TryGetValue(targetPosition, out ChessBasic targetChess);
            if (!haveChess) continue;

            if (targetChess.color != color)
            {
                possibleMoveList.Add(targetPosition);
                canEatChessPosition.Add(targetPosition);
            }
        }

        ExtraFindPossibleMove();

        _chessBoard.ShowCanGo(possibleMoveList);

    }

    private void ScoutSecondBuff(ChessBasic chess)
    {
        if (_player.pawnBuffType != Player.PawnBuff.Scout || !_player.scout.canReceiveMoveAreaFromYouAteChess) return;
        _player.scout.AddExtraMoveArea(chess);
    }


    public override bool CanEatChess(ChessBasic chess)
    {

        if (chess.haveExtraLife) return false;
        bool canOnlyKillKing = _player.pawnBuffType == Player.PawnBuff.Substitute
            && _player.substitute.canOnlyKillKing;
        bool isKing = chess.type == ChessType.King;
        if (canOnlyKillKing && !isKing) return false;
        bool haveBarrier = chess.TryGetComponent<King>(out King king) && king.haveBarrier;
        return !haveBarrier;
    }


    public override void Move(Vector2Int moveTo)
    {
        if (isFirstMove) isFirstMove = false;

        _player.nowPlayerStage = PlayerStage.MovingChess;
        ReturnPick();
        bool posHaveChess = _chessBoard.board.TryGetValue(moveTo, out ChessBasic chess);
        if (posHaveChess)
        {
            if (!CanEatChess(chess))
            {
                _player.nowPlayerStage = PlayerStage.ReadytoEnd;
                return;
            }
            _player.nowPlayerStage = PlayerStage.EatingChess;
            ScoutSecondBuff(chess);
            _chessBoard.board[moveTo].GotEaten();
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


}
