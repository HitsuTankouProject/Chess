using UnityEngine;
using System.Collections.Generic;
using System.Collections;

using static Player;
using NUnit.Framework;
using Unity.VisualScripting;


public enum PlayerStage { NoMyTurn,TurnInit,Ready,MovingChess,EatingChess,ReadytoEnd,End}

public class Player : MonoBehaviour
{
    public ChessColor usingChess;
    public PlayerStage nowPlayerStage;

    public PlayerInPut playerInPut;

    public Dictionary<ChessType, List<ChessBasic>> allTheChess { get; private set; } = new Dictionary<ChessType, List<ChessBasic>>();
    public void AllChessInit(HashSet<ChessBasic> chess)
    {
        foreach (ChessBasic target in chess)
        {
            if (!allTheChess.ContainsKey(target.type))
            {
                allTheChess[target.type] = new List<ChessBasic>();
            }

            allTheChess[target.type].Add(target);
            target.ChessInit(this);
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

    public bool IsProtectedByRook_Guardian(Vector2Int targetChess)
    {
        if (rookBuffType != RookBuff.Guardian|| allTheChess[ChessType.Rook].Count == 0) return false;
        
        foreach (ChessBasic chess in allTheChess[ChessType.Rook])
        {
            if (chess == null) continue;
            chess.TryGetComponent<Rook>(out Rook rook);
            if (rook != null)
            {
                if (rook.GuardianBuff(targetChess)) return true;
            }
        }

        return false;
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
        kingBuffType = KingBuff.None;
        madKing.BuffInit(this);
        sageKing.BuffInit(this);

        queenBuffType = QueenBuff.None;
        witcher.BuffInit(this);
        beauty.BuffInit(this);

        knightBuffType = KnightBuff.None;
        charger.BuffInit(this);
        skirmisher.BuffInit(this);

        bishopBuffType = BishopBuff.None;
        sorcerer.BuffInit(this);
        monk.BuffInit(this);

        rookBuffType = RookBuff.None;
        rusher.BuffInit(this);
        guardian.BuffInit(this);

        pawnBuffType = PawnBuff.None;
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
        AllTheBuffInit();

    }

    public void Player_ChessInit(HashSet<ChessBasic> targetList)
    {
        AllChessInit(targetList);

        rookBuffType = RookBuff.Rusher;
        rusher.LevelUpToTargetLevel(3, out bool a);

        //bishopBuffType = BishopBuff.Monk;
        //monk.LevelUpToTargetLevel(3, out bool b);

        //knightBuffType = KnightBuff.Charger;
        //charger.LevelUpToTargetLevel(2, out bool c);

    }


    public Coroutine turnStart;
    
    private bool turnCanEnd = false;
    private IEnumerator TurnStart()
    {
        List<ChessType> removeKeys = new List<ChessType>();
        foreach (var pair in allTheChess)
        {
            pair.Value.RemoveAll(chess => chess == null);

            if (pair.Value.Count == 0)
            {
                removeKeys.Add(pair.Key);
            }
        }
        foreach (ChessType key in removeKeys)
        {
            allTheChess.Remove(key);
        }
        yield return null;
        nowPlayerStage = PlayerStage.Ready;
        turnCanEnd = false;
        while (!turnCanEnd)
        {
            switch (nowPlayerStage)
            {
                case PlayerStage.Ready:
                    playerInPut.InPutSystem_Update();
                    break;
                case PlayerStage.ReadytoEnd:
                    turnCanEnd = true;
                    break;

                default:break;
            }

            yield return null;

        }

        nowPlayerStage = PlayerStage.End;

        InGame.Instance.StartTurnChange();
    }

    public void Player_TurnStart()
    {
        nowPlayerStage = PlayerStage.TurnInit;
        turnStart = StartCoroutine(TurnStart());

    }


    private void Update()
    {
        
    }


}
