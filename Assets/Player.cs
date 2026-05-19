using UnityEngine;
using System.Collections.Generic;


public class Player : MonoBehaviour
{
    public ChessColor usingChess;
    public List<ChessBasic> allTheChess = new List<ChessBasic>();



    [Header("King Buff")]
    public KingBuff kingBuffType;
    public enum KingBuff { None, MadKing, SageKing }
    public MadKing madKing = new MadKing();
    public SageKing sageKing = new SageKing();

    [Header("Queen Buff")]
    public QueenBuff queenBuffType;
    public enum QueenBuff { None, Witcher, Beauty }
    public Witcher witcher = new Witcher();
    public Beauty beauty = new Beauty();

    [Header("Knight Buff")]
    public KnightBuff knightBuffType;
    public enum KnightBuff { None, Charger, Skirmisher }
    public Charger charger = new Charger();
    public Skirmisher skirmisher = new Skirmisher();

    [Header("BisHop Buff")]
    public BisHopBuff bisHopBuffType;
    public enum BisHopBuff { None, Sorcerer, Monk };
    public Sorcerer sorcerer = new Sorcerer();
    public Monk monk = new Monk();

    [Header("Rook Buff")]
    public RookBuff rookBuffType;
    public enum RookBuff { None, Rusher, Guardian };
    public Rusher rusher = new Rusher();
    public Guardian guardian = new Guardian();

    [Header("Pawn Buff")]
    public PawnBuff pawnBuffType;
    public enum PawnBuff { None, Scout, Shapeshifter }
    public Scout scout = new Scout();
    public Shapeshifter shapeshifter = new Shapeshifter();

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
    }

}
