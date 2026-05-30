using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sorcerer : BuffBasic
{
    public override ChessType buffChess => ChessType.Bishop;
    public override string buffName => "Sorcerer";
    public override void Choose() => _player.bishopBuffType = Player.BishopBuff.Sorcerer;


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

        _enemy = _player != InGame.Instance.whiteChessPlayer ? 
            InGame.Instance.whiteChessPlayer : InGame.Instance.blackChessPlayer;
    }

    public void CurseChess()
    {
        if (!canCurseChess) return;

        if (_enemy == null)
        {
            Debug.LogError("[Sorcerer] _enemy == null");
            return;
        }


        List<ChessType> canCurseTypes = new List<ChessType>();
        foreach (var pair in _enemy.allTheChess)
        {
            if (pair.Value == null || pair.Value.Count == 0) continue;

            canCurseTypes.Add(pair.Key); 
        }
        if (canCurseTypes.Count == 0) return;

        ChessType chessType = canCurseTypes[Random.Range(0, canCurseTypes.Count)];
        List<ChessBasic> targetChessList = _enemy.allTheChess[chessType];


        int randomIndex = Random.Range(0, targetChessList.Count);
        targetChessList[randomIndex].CurseThisChess();
    }

}

public class Monk : BuffBasic
{
    public override ChessType buffChess => ChessType.Bishop;
    public override string buffName => "Monk";
    public override void Choose() => _player.bishopBuffType = Player.BishopBuff.Monk;

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
        if (!canPurificChess || purificChesses.Count == 0) ;
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
        if (_player.bishopBuffType == Player.BishopBuff.None) return;
        HashSet<Vector2Int> extraCanGoArea = new HashSet<Vector2Int>();
        int extraCanGoRange;

        if (_player.bishopBuffType == Player.BishopBuff.Sorcerer)
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

                if (!_chessBoard.IsOutOfBoard(targetPos)) break;


                if (_chessBoard.board.TryGetValue(targetPos, out ChessBasic chess))
                {
                    if (chess.color != this.color)
                    {
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


    public void PurificChess()
    {
        possibleMoveList.Clear();
        HashSet<ChessBasic> purificChesses = new HashSet<ChessBasic>();
        FindCanMove(true);
        ExtraFindPossibleMove(true);

        foreach (Vector2Int targetPos in possibleMoveList)
        {
            if (_chessBoard.board.TryGetValue(targetPos, out ChessBasic chess))
            {
                if (chess.color == color && chess.gotCurse) purificChesses.Add(chess);
            }
        }

        _player.monk.PurificChess(purificChesses);
    }

    public override void Move(Vector2Int moveTo)
    {
        base.Move(moveTo);
        switch (_player.bishopBuffType)
        {
            case Player.BishopBuff.None: return;
            case Player.BishopBuff.Sorcerer:_player.sorcerer.CurseChess();return;
            case Player.BishopBuff.Monk:PurificChess();return;
        }
    }


}
