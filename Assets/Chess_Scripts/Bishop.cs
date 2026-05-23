using System.Collections.Generic;
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
        foreach(ChessBasic king in _player.allTheChess[ChessType.King])
        {
            king.haveExtraLife = true;
        }

    }


}

public class Bishop : ChessBasic
{
    public override ChessType type => ChessType.Bishop;
    public override string ChessName() { return "Bishop"; }
    public override int findRange { get; protected set; } = 8;

    private List<Vector2Int> directions = new List<Vector2Int>
     { new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

    public override void ExtraFindPossibleMove()
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

        foreach (var dir in extraCanGoArea)
        {
            for (int i = 1; i <= extraCanGoRange; i++)
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

    public override void FindPossibleMove()
    {
        possibleMoveList.Clear();
        canEatChessPosition.Clear();

        foreach (var dir in directions)
        {
            for (int i = 1; i < findRange; i++)
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

        ExtraFindPossibleMove();

        _chessBoard.ShowCanGo(possibleMoveList);
    }
    public override void Move(Vector2Int moveTo)
    {
        base.Move(moveTo);
        if(_player.bishopBuffType == Player.BishopBuff.Sorcerer)
        {
            _player.sorcerer.CurseChess();
        }

    }


}
