using UnityEngine;
using System.Collections.Generic;
using System.Collections;

using static Player;


public class Player : MonoBehaviour
{
    public ChessColor usingChess;

    public Dictionary<ChessType, HashSet<ChessBasic>> allTheChess { get; private set; };
    public void AllChessInit(HashSet<ChessBasic> chess)
    {
        foreach(ChessBasic target in chess)
        {
            if (!allTheChess.ContainsKey(target.type))
            {
                allTheChess[target.type] = new HashSet<ChessBasic>();
            }

            allTheChess[target.type].Add(target);

        }


    }



    #region Buff Chess

    [Header("King Buff")]
    public KingBuff kingBuffType;
    public enum KingBuff { None, MadKing, SageKing }
    public MadKing madKing = new MadKing();
    public SageKing sageKing = new SageKing();

    private void LevelUp_KingBuff()
    {
        if (kingBuffType == KingBuff.None) return;
        bool success;
        bool isMadKing = kingBuffType == KingBuff.MadKing;
        if (isMadKing) madKing.LevelUp(out success);
        else sageKing.LevelUp(out success);

        if (!success) Debug.LogError("KingBuff LevelUp failed" + kingBuffType.ToString());

    }

    [Header("Queen Buff")]
    public QueenBuff queenBuffType;
    public enum QueenBuff { None, Witcher, Beauty }
    public Witcher witcher = new Witcher();
    public Beauty beauty = new Beauty();

    private void LevelUp_QueenBuff()
    {
        if (queenBuffType == QueenBuff.None) return;
        bool success;
        bool isWitcher = queenBuffType == QueenBuff.Witcher;
        if (isWitcher) witcher.LevelUp(out success);
        else beauty.LevelUp(out success);

        if (!success) Debug.LogError("QueenBuff LevelUp failed" + queenBuffType.ToString());

    }


    [Header("Knight Buff")]
    public KnightBuff knightBuffType;
    public enum KnightBuff { None, Charger, Skirmisher }
    public Charger charger = new Charger();
    public Skirmisher skirmisher = new Skirmisher();

    private void LevelUp_KnightBuff()
    {
        if (knightBuffType == KnightBuff.None) return;

        bool success;
        bool isCharger = knightBuffType == KnightBuff.Charger;

        if (isCharger) charger.LevelUp(out success);
        else skirmisher.LevelUp(out success);

        if (!success)
        {
            Debug.LogError("KnightBuff LevelUp failed " + knightBuffType.ToString());
        }
    }


    [Header("Bishop Buff")]
    public BishopBuff bishopBuffType;
    public enum BishopBuff { None, Sorcerer, Monk };
    public Sorcerer sorcerer = new Sorcerer();
    public Monk monk = new Monk();

    private void LevelUp_BisHopBuff()
    {
        if (bishopBuffType == BishopBuff.None) return;

        bool success;
        bool isSorcerer = bishopBuffType == BishopBuff.Sorcerer;

        if (isSorcerer) sorcerer.LevelUp(out success);
        else monk.LevelUp(out success);

        if (!success)
        {
            Debug.LogError("BishopBuff LevelUp failed " + bishopBuffType.ToString());
        }
    }

    [Header("Rook Buff")]
    public RookBuff rookBuffType;
    public enum RookBuff { None, Rusher, Guardian };
    public Rusher rusher = new Rusher();
    public Guardian guardian = new Guardian();
    private void LevelUp_RookBuff()
    {
        if (rookBuffType == RookBuff.None) return;

        bool success;
        bool isRusher = rookBuffType == RookBuff.Rusher;

        if (isRusher) rusher.LevelUp(out success);
        else guardian.LevelUp(out success);

        if (!success)
        {
            Debug.LogError("RookBuff LevelUp failed " + rookBuffType.ToString());
        }
    }

    [Header("Pawn Buff")]
    public PawnBuff pawnBuffType;
    public enum PawnBuff { None, Scout, Substitute }
    public Scout scout = new Scout();
    public Substitute substitute = new Substitute();

    private void LevelUp_PawnBuff()
    {
        if (pawnBuffType == PawnBuff.None) return;

        bool success;
        bool isScout = pawnBuffType == PawnBuff.Scout;

        if (isScout) scout.LevelUp(out success);
        else substitute.LevelUp(out success);

        if (!success)
        {
            Debug.LogError("PawnBuff LevelUp failed " + pawnBuffType.ToString());
        }
    }
    private void AllTheBuffInit()
    {
        madKing.BuffInit(this);
        sageKing.BuffInit(this);

        witcher.BuffInit(this);
        beauty.BuffInit(this);

        charger.BuffInit(this);
        skirmisher.BuffInit(this);

        sorcerer.BuffInit(this);
        monk.BuffInit(this);

        rusher.BuffInit(this);
        guardian.BuffInit(this);

        scout.BuffInit(this);
        substitute.BuffInit(this);
    }

    public void AllTheBuffTryLevelUp()
    {
        LevelUp_KingBuff();
        LevelUp_QueenBuff();
        LevelUp_KnightBuff();
        LevelUp_BisHopBuff();
        LevelUp_RookBuff();
        LevelUp_PawnBuff();
    }

    #endregion





    public void Player_Init(ChessColor targetChess)
    {
        usingChess = targetChess;
    }

    public void Player_TurnInit(List<ChessBasic> targetList)
    {
        allTheChess = targetList;
        foreach (ChessBasic chess in allTheChess)
        {
            chess.ChessInit(this);
        }

        rookBuffType = RookBuff.Rusher;
        rusher.LevelUpToTargetLevel(3, out bool a);

        bishopBuffType = BishopBuff.Monk;
        monk.LevelUpToTargetLevel(3, out bool b);
    }

}
