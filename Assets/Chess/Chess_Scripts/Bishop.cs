using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sorcerer : BuffBasic
{
    public override ChessType buffChess => ChessType.Bishop;
    public override string buffName => "Sorcerer";
    public override void Choose() => _player.allTheBuff.bishopBuffType = BishopBuff.Sorcerer;


    public HashSet<Vector2Int> extraCanGoArea = new HashSet<Vector2Int>();
    private readonly Vector2Int firstExtraDirections = Vector2Int.up;
    private readonly Vector2Int secondExtraDirections = Vector2Int.down;
    public readonly int extraCanGoRange = 2;

    public bool canCurseChess;
    private Player _enemy;

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
        ChessColor othersChessColor = _player.usingChess == ChessColor.White ? ChessColor.Black : ChessColor.White;

        _enemy = GameManager.Instance.TargetPlayer(othersChessColor);
    }

    public void CurseChess()
    {
        if (!canCurseChess) return;

        if (_enemy == null)
        {
            Debug.LogError("[Sorcerer] _enemy == null");
            return;
        }

        List<ChessBasic> canCurseChessList = new List<ChessBasic>();
        foreach (ChessBasic chess in _enemy.allTheChess.Values)
        {
            if(chess.type == ChessType.King) continue;
            canCurseChessList.Add(chess);
        }
        int randomIndex = Random.Range(0, canCurseChessList.Count);
        canCurseChessList[randomIndex].CurseThisChess();
    }

}

public class Monk : BuffBasic
{
    public override ChessType buffChess => ChessType.Bishop;
    public override string buffName => "Monk";
    public override void Choose() => _player.allTheBuff.bishopBuffType = BishopBuff.Monk;

    public HashSet<Vector2Int> extraCanGoArea = new HashSet<Vector2Int>();
    private readonly Vector2Int firstExtraDirections = Vector2Int.left;
    private readonly Vector2Int secondExtraDirections = Vector2Int.right;
    public readonly int extraCanGoRange = 2;
    public bool canPurificChess { get; private set; } = false;


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
        canPurificChess = true;
    }

    public void PurificChess(HashSet<ChessBasic> purificChesses)
    {
        if (!canPurificChess || purificChesses.Count == 0) return;
        foreach(ChessBasic chess in purificChesses)
        {
            if (chess.gotCurse) chess.PurifyThisChess();
        }
    }


}

public class Bishop : ChessBasic
{
    public override ChessType type => ChessType.Bishop;
    public override string ChessName() { return "Bishop"; }
    public override int findRange { get;} = 8;

    public override HashSet<Vector2Int> directions => new HashSet<Vector2Int>()
    {
         new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
    };

    public override void ExtraFindPossibleMove(bool isThrougt)
    {
        if (_player.bishopBuffType == BishopBuff.None) return;
        HashSet<Vector2Int> extraCanGoArea = new HashSet<Vector2Int>();
        int extraCanGoRange;

        if (_player.bishopBuffType == BishopBuff.Sorcerer)
        {
            extraCanGoArea = _player.sorcerer.extraCanGoArea;
            extraCanGoRange = _player.sorcerer.extraCanGoRange;
        }
        else
        {
            extraCanGoArea = _player.monk.extraCanGoArea;
            extraCanGoRange = _player.monk.extraCanGoRange;
        }

        foreach (Vector2Int dir in extraCanGoArea)
        {
            for (int i = 1; i <= extraCanGoRange; i++)
            {
                Vector2Int targetPos = position + dir * i;

                if (_chessBoard.IsOutOfBoard(targetPos)) break;

                if (_chessBoard.board.TryGetValue(targetPos, out ChessBasic chess))
                {
                    if (chess.color != this.color)
                    {
                        possibleMoveList.Add(targetPos);
                        possibleMoveList.Add(targetPos);
                    }
                    if(!isThrougt) break;
                }
                else
                {
                    possibleMoveList.Add(targetPos);
                }
            }
        }
    }

    public override void CurseThisChess()
    {
        if (_player.bishopBuffType == BishopBuff.Sorcerer) PurifyThisChess();
        else base.CurseThisChess();
    }


    public void PurificChess()
    {
        HashSet<ChessBasic> purificChesses = new HashSet<ChessBasic>();
        HashSet<Vector2Int> findPurific = PossibleMove(true);
        Debug.Log(findPurific.Count);

        foreach (Vector2Int targetPos in findPurific)
        {
            if (_chessBoard.board.TryGetValue(targetPos, out ChessBasic chess))
            {
                Debug.Log(chess.chessInfo.first.ToString());
                Debug.Log(chess.chessInfo.second.ToString());

                if (chess.color == color && chess.gotCurse)
                {
                    Debug.Log(targetPos);
                    purificChesses.Add(chess);
                }
            }
        }

        _player.monk.PurificChess(purificChesses);
    }

    public override void Move(Vector2Int moveTo)
    {
        base.Move(moveTo);
        switch (_player.bishopBuffType)
        {
            case BishopBuff.None: return;
            case BishopBuff.Sorcerer:_player.sorcerer.CurseChess();return;
            case BishopBuff.Monk:PurificChess();return;
        }
    }


}
