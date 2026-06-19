using UnityEngine;
using System.Collections.Generic;
using System.Collections;

using static Player;
using NUnit.Framework;
using Unity.VisualScripting;
using static UnityEditor.Experimental.GraphView.GraphView;


public enum PlayerStage { NoMyTurn,TurnInit,Ready,MovingChess,EatingChess,ReadytoEnd,End }

public class Player : MonoBehaviour
{
    private ChessBoard _chessBoard => ChessBoard.Instance;


    public ChessColor usingChess;
    public PlayerStage nowPlayerStage;

    public PlayerInPut playerInPut;
    

    public Dictionary<Vector2Int, ChessBasic> allTheChess { get; private set; } = new Dictionary<Vector2Int, ChessBasic>();
    public void UpdateChessDict(Vector2Int oldPos, Vector2Int newPos, ChessBasic chess)
    {
        if (allTheChess.ContainsKey(oldPos)) allTheChess.Remove(oldPos);
        allTheChess[newPos] = chess;
    }
    public void AllChessInit(Dictionary<Vector2Int, ChessBasic> targetDict)
    {
        allTheChess = targetDict;
        foreach (ChessBasic chess in allTheChess.Values)
        {
            chess.ChessInit(this);
        }
    }
    public List<ChessBasic> ChessListByType(ChessType chessType)
    {
        List<ChessBasic> chessList = new List<ChessBasic>();
        foreach (ChessBasic chessBasic in allTheChess.Values)
        {
            if (chessBasic.type == chessType)
            {
                chessList.Add(chessBasic);
            }
        }
        return chessList;
    }


    #region Buff Chess

    #region King
    [Header("King Buff")]
    public KingBuff kingBuffType;
    public enum KingBuff { None, MadKing, SageKing }
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
    public enum QueenBuff { None, Witcher, Beauty }
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

    #region Knight

    [Header("Knight Buff")]
    public KnightBuff knightBuffType;
    public enum KnightBuff { None, Charger, Skirmisher }
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

    #region Bishop
    [Header("Bishop Buff")]
    public BishopBuff bishopBuffType;
    public enum BishopBuff { None, Sorcerer, Monk };
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
    public enum RookBuff { None, Rusher, Guardian };
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

    private HashSet<Vector2Int> guardianProtectArea = new HashSet<Vector2Int>();
    public void AddToProtectArea(HashSet<Vector2Int> addToProtectArea) => guardianProtectArea.AddRange(addToProtectArea);
    public void UpdateGuardianProtectArea()
    {
        if (rookBuffType != RookBuff.Guardian) return;
        foreach(Vector2Int area in guardianProtectArea)
        {
            if (!_chessBoard.board.TryGetValue(area, out ChessBasic chess) || chess.color != usingChess) continue;
            chess.haveExtraLife = false;
        }
        guardianProtectArea.Clear();
        
        List<ChessBasic> rookList = ChessListByType(ChessType.Rook);

        if (rookList.Count == 0) return;
        foreach(ChessBasic chess in rookList)
        {
            Debug.Log("4");
            if (!chess.TryGetComponent<Rook>(out Rook rook))
            {
                Debug.LogError(" NonRook store in the Rook List");
                return;
            }
            rook.GuardianBuff();

        }
    }
    public bool IsProTectedByRook_Guardian(Vector2Int targetChessPos) => guardianProtectArea.Contains(targetChessPos);
    #endregion

    #region Pawn
    [Header("Pawn Buff")]
    public PawnBuff pawnBuffType;
    public enum PawnBuff { None, Scout, Substitute }
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
    #endregion

    private Dictionary<AllBuffCard, BuffBasic> cardBuffMap;
    private void BuffInit(ChessType chessType, BuffBasic buffBasic1, BuffBasic buffBasic2)
    {
        if(buffBasic1.buffChess!= chessType|| buffBasic2.buffChess != chessType)
        {
            Debug.LogError($"{chessType.ToString()} : {buffBasic1.buffName} , {buffBasic2.buffName}");
            return;
        }
        switch (chessType)
        {
            case ChessType.King:kingBuffType = KingBuff.None;break;
            case ChessType.Queen:queenBuffType = QueenBuff.None;break;
            case ChessType.Knight:knightBuffType = KnightBuff.None;break;
            case ChessType.Bishop:bishopBuffType = BishopBuff.None;break;
            case ChessType.Rook:rookBuffType = RookBuff.None;break;
            case ChessType.Pawn:pawnBuffType = PawnBuff.None;break;
        }
        buffBasic1.BuffInit(this);
        buffBasic2.BuffInit(this);
    }
    private void AllTheBuffInit()
    {
        BuffInit(ChessType.King, madKing, sageKing);
        BuffInit(ChessType.Queen, witcher, beauty);
        BuffInit(ChessType.Knight, charger, skirmisher);
        BuffInit(ChessType.Bishop, sorcerer, monk);
        BuffInit(ChessType.Rook, rusher, guardian);
        BuffInit(ChessType.Pawn, scout, substitute);

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

    public void CurrentBuffLevelUp(ChessType chessType)
    {
        BuffBasic buff = chessType switch
        {
            ChessType.King => TargetBuff_King(kingBuffType),
            ChessType.Queen => TargetBuff_Queen(queenBuffType),
            ChessType.Knight => TargetBuff_Knight(knightBuffType),
            ChessType.Bishop => TargetBuff_Bishop(bishopBuffType),
            ChessType.Rook => TargetBuff_Rook(rookBuffType),
            ChessType.Pawn => TargetBuff_Pawn(pawnBuffType),
            _ => null
        };

        buff.LevelUp(out bool success);
        if (!success)
        {
            Debug.LogError(buff.buffName + " LevelUp No success");
            return;
        }
    }

    public void ChooseBuff(AllBuffCard choseBuff)
    {
        cardBuffMap[choseBuff].Choose();
        cardBuffMap[choseBuff].LevelUpToTargetLevel(1, out bool success);
        if (!success)
        {
            Debug.LogError(cardBuffMap[choseBuff].buffName+" LevelUp No success");
            return;
        }
    }


    #endregion


    public void Player_Init(ChessColor targetChess)
    {
        usingChess = targetChess;
        AllTheBuffInit();
        playerInPut.Init(this);
    }

    public void Player_ChessInit(Dictionary<Vector2Int, ChessBasic> targetDict)
    {
        AllChessInit(targetDict);

        //rookBuffType = RookBuff.Rusher;
        //rusher.LevelUpToTargetLevel(3, out bool a);
    }

    private void Player_ChessDictUpdate()
    {
        List<Vector2Int> removeKeys = new List<Vector2Int>();

        foreach(Vector2Int pos in allTheChess.Keys)
        {
            if(!_chessBoard.board.ContainsKey(pos) || !allTheChess[pos].gameObject.activeSelf)
            {
                removeKeys.Add(pos);
            }

        }
        foreach(Vector2Int pos in removeKeys)
        {
            allTheChess.Remove(pos);
        }


        UpdateGuardianProtectArea();
    }

    public Coroutine turnStart;
    
    private bool turnCanEnd = false;
    private IEnumerator TurnStart()
    {
        Player_ChessDictUpdate();
        yield return null;
        nowPlayerStage = PlayerStage.Ready;
        turnCanEnd = false;
        playerInPut.StartInPutSystem();
        while (!turnCanEnd)
        {
            switch (nowPlayerStage)
            {
                case PlayerStage.ReadytoEnd:turnCanEnd = true; break;
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
        playerInPut.StartInput();
    }

}
