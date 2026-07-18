using System.Collections.Generic;
using UnityEngine;

public enum KingBuff { None, MadKing, SageKing }
public enum QueenBuff { None, Witcher, Beauty }
public enum BishopBuff { None, Sorcerer, Monk };
public enum RookBuff { None, Rusher, Guardian };
public enum KnightBuff { None, Charger, Skirmisher }
public enum PawnBuff { None, Scout, Substitute }

[System.Serializable]
public class AllTheBuff
{
    #region King
    [Header("King Buff")]
    public KingBuff kingBuffType;
    public MadKing madKing = new MadKing();
    public SageKing sageKing = new SageKing();

    private BuffBasic TargetBuff_King(KingBuff buff)
    {
        return buff switch
        {
            KingBuff.MadKing => madKing,
            KingBuff.SageKing => sageKing,
            _ => null
        };
    }

    #endregion

    #region Queen
    [Header("Queen Buff")]
    public QueenBuff queenBuffType;
    public Witcher witcher = new Witcher();
    public Beauty beauty = new Beauty();
    private BuffBasic TargetBuff_Queen(QueenBuff buff)
    {
        return buff switch
        {
            QueenBuff.Witcher => witcher,
            QueenBuff.Beauty => beauty,
            _ => null
        };
    }

    #endregion

    #region Bishop
    [Header("Bishop Buff")]
    public BishopBuff bishopBuffType;
    public Sorcerer sorcerer = new Sorcerer();
    public Monk monk = new Monk();
    private BuffBasic TargetBuff_Bishop(BishopBuff buff)
    {
        return buff switch
        {
            BishopBuff.Sorcerer => sorcerer,
            BishopBuff.Monk => monk,
            _ => null
        };
    }

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
    #endregion

    #region Rook
    [Header("Rook Buff")]
    public RookBuff rookBuffType;
    public Rusher rusher = new Rusher();
    public Guardian guardian = new Guardian();
    private BuffBasic TargetBuff_Rook(RookBuff buff)
    {
        return buff switch
        {
            RookBuff.Rusher => rusher,
            RookBuff.Guardian => guardian,
            _ => null
        };
    }

    public HashSet<ChessBasic> protectedByGuardianChesses { get; private set; } = new();

    public void AddGuardianProtectedChess(ChessBasic chess)
    {
        if (chess == null) return;
        if (protectedByGuardianChesses.Add(chess)) chess.GotExtraLife(true);
    }
    public void UpdateGuardianProtectArea(List<ChessBasic> rookList)
    {
        if (rookBuffType != RookBuff.Guardian)
            return;

        foreach (ChessBasic chess in protectedByGuardianChesses)
        {
            if (chess != null)
            {
                chess.GotExtraLife(false);
            }
        }

        protectedByGuardianChesses.Clear();
        foreach (ChessBasic chess in rookList)
        {
            if (!chess.TryGetComponent(out Rook rook))
            {
                Debug.LogError("NonRook stored in the Rook List");
                continue;
            }
            rook.GuardianBuff();
        }
    }



    #endregion

    #region Knight

    [Header("Knight Buff")]
    public KnightBuff knightBuffType;
    public Charger charger = new Charger();
    public Skirmisher skirmisher = new Skirmisher();
    private BuffBasic TargetBuff_Knight(KnightBuff buff)
    {
        return buff switch
        {
            KnightBuff.Charger => charger,
            KnightBuff.Skirmisher => skirmisher,
            _ => null
        };
    }

    #endregion

    #region Pawn
    [Header("Pawn Buff")]
    public PawnBuff pawnBuffType;
    public Scout scout = new Scout();
    public Substitute substitute = new Substitute();
    private BuffBasic TargetBuff_Pawn(PawnBuff buff)
    {
        return buff switch
        {
            PawnBuff.Scout => scout,
            PawnBuff.Substitute => substitute,
            _ => null
        };
    }

    public bool IsProtectbySubstitute(List<ChessBasic> pawnList, out ChessBasic chess)
    {
        chess = null;

        if (substitute == null|| !substitute.cantKillKingWhenPawnExist
            || pawnList == null || pawnList.Count == 0)
            return false;
        chess = pawnList[Random.Range(0, pawnList.Count)];
        return chess != null;
    }



    #endregion

    public Dictionary<AllBuffCard, BuffBasic> cardBuffMap { get; private set; } = new();
    public List<AllBuffCard> choseBuffs { get; private set; } = new();
    private void BuffInit(Player player,ChessType chessType, BuffBasic buffBasic1, BuffBasic buffBasic2)
    {
        if (buffBasic1.buffChess != chessType || buffBasic2.buffChess != chessType)
        {
            Debug.LogError($"{chessType.ToString()} : {buffBasic1.buffName} , {buffBasic2.buffName}");
            return;
        }
        switch (chessType)
        {
            case ChessType.King: kingBuffType = KingBuff.None; break;
            case ChessType.Queen: queenBuffType = QueenBuff.None; break;
            case ChessType.Knight: knightBuffType = KnightBuff.None; break;
            case ChessType.Bishop: bishopBuffType = BishopBuff.None; break;
            case ChessType.Rook: rookBuffType = RookBuff.None; break;
            case ChessType.Pawn: pawnBuffType = PawnBuff.None; break;
        }
        buffBasic1.BuffInit(player);
        buffBasic2.BuffInit(player);
    }
    public void AllTheBuffInit(Player player)
    {
        BuffInit(player,ChessType.King, madKing, sageKing);
        BuffInit(player, ChessType.Queen, witcher, beauty);
        BuffInit(player, ChessType.Knight, charger, skirmisher);
        BuffInit(player, ChessType.Bishop, sorcerer, monk);
        BuffInit(player, ChessType.Rook, rusher, guardian);
        BuffInit(player, ChessType.Pawn, scout, substitute);

        cardBuffMap = new Dictionary<AllBuffCard, BuffBasic>
        {
            { AllBuffCard.SageKing, sageKing },
            { AllBuffCard.MadKing, madKing },

            { AllBuffCard.Witcher, witcher },
            { AllBuffCard.Beauty, beauty },

            { AllBuffCard.Charger, charger },
            { AllBuffCard.Skirmisher, skirmisher },

            { AllBuffCard.Sorcerer, sorcerer },
            { AllBuffCard.Monk, monk },

            { AllBuffCard.Rusher, rusher },
            { AllBuffCard.Guardian, guardian },

            { AllBuffCard.Scout, scout },
            { AllBuffCard.Substitute, substitute },
        };
    }

    public void AllBuffLevelUp()
    {
        BuffBasic[] buffs =
        {
            TargetBuff_King(kingBuffType),
            TargetBuff_Queen(queenBuffType),
            TargetBuff_Bishop(bishopBuffType),
            TargetBuff_Rook(rookBuffType),
            TargetBuff_Knight(knightBuffType),
            TargetBuff_Pawn(pawnBuffType),
        };

        foreach (BuffBasic buff in buffs)
        {
            if (buff == null) continue;
            buff.LevelUp(out bool success);
            //Debug.Log(buff.buffName + "'s Level :" + buff.nowBuffLevel);
            if (!success)
            {
                Debug.LogWarning($"{buff.buffName} already at max level.");
                continue;
            }
        }

    }

    public void ChooseBuff(AllBuffCard choseBuff)
    {
        cardBuffMap[choseBuff].Choose();
        cardBuffMap[choseBuff].LevelUpToTargetLevel(1, out bool success);
        choseBuffs.Add(choseBuff);
        if (!success)
        {
            Debug.LogError(cardBuffMap[choseBuff].buffName + " LevelUp No success");
            return;
        }
    }

}
