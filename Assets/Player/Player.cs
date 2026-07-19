using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.VisualScripting;


public class Player : MonoBehaviour
{
    private GameManager _gameManager => GameManager.Instance;
    private ChessBoard _chessBoard => _gameManager.chessBoard;
    private InPutManager _inPutManager => _gameManager.inPutManager;

    public ChessColor usingChess;

    public PlayerInPut playerInPut;
    public PlayerCanvas playerCanvas;
    public bool isPause => playerCanvas.isPause;

    public Dictionary<Vector2Int, ChessBasic> allTheChess { get; private set; } = new();
    public void AllChessInit(Dictionary<Vector2Int, ChessBasic> targetDict)
    {
        allTheChess.Clear();
        allTheChess.AddRange(targetDict);
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

    public bool haveKing = true;
    #region Chess Buff
    public AllTheBuff allTheBuff = new();
    public void AllTheBuffInit() => allTheBuff.AllTheBuffInit(this);



    #region King
    public KingBuff kingBuffType => allTheBuff.kingBuffType;
    public MadKing madKing => allTheBuff.madKing;
    public SageKing sageKing => allTheBuff.sageKing;
    #endregion
    #region Queen
    public QueenBuff queenBuffType => allTheBuff.queenBuffType;
    public Witcher witcher => allTheBuff.witcher;
    public Beauty beauty => allTheBuff.beauty;



    #endregion   
    #region Bishop
    public BishopBuff bishopBuffType => allTheBuff.bishopBuffType;
    public Sorcerer sorcerer => allTheBuff.sorcerer;
    public Monk monk => allTheBuff.monk;


    #endregion
    #region Rook
    public RookBuff rookBuffType => allTheBuff.rookBuffType;
    public Rusher rusher => allTheBuff.rusher;
    public Guardian guardian => allTheBuff.guardian;

    public void AddGuardianProtectedChess(ChessBasic chess) =>
        allTheBuff.AddGuardianProtectedChess(chess);
    public void UpdateGuardianProtectArea() =>
        allTheBuff.UpdateGuardianProtectArea(ChessListByType(ChessType.Rook));

    #endregion
    #region Knight
    public KnightBuff knightBuffType => allTheBuff.knightBuffType;
    public Charger charger => allTheBuff.charger;
    public Skirmisher skirmisher => allTheBuff.skirmisher;



    #endregion
    #region Pawn
    public PawnBuff pawnBuffType => allTheBuff.pawnBuffType;
    public Scout scout => allTheBuff.scout;
    public Substitute substitute => allTheBuff.substitute;

    public bool IsProtectbySubstitute(out ChessBasic chess) =>
        allTheBuff.IsProtectbySubstitute(ChessListByType(ChessType.Pawn), out chess);

    #endregion
    public Dictionary<AllBuffCard, BuffBasic> cardBuffMap => allTheBuff.cardBuffMap;

    public List<AllBuffCard> choseBuffs => allTheBuff.choseBuffs;
    public void AllBuffLevelUp() => allTheBuff.AllBuffLevelUp();
    public void ChooseBuff(AllBuffCard choseBuff)=> allTheBuff.ChooseBuff(choseBuff);

    #endregion

    public void TurnCamera(PlayerCameraStage playerCameraStage)=>
        _gameManager.PlayerCameraTurn(new Pair<ChessColor, PlayerCameraStage>(usingChess, playerCameraStage));

    public void Player_Init(ChessColor targetChess)
    {
        usingChess = targetChess;
        playerInPut.Init(this);
        playerInPut.StartInput();
    }

    public void Player_ChessInit(Dictionary<Vector2Int, ChessBasic> targetDict)
    {

        //allTheBuff.kingBuffType = KingBuff.MadKing;
        //madKing.LevelUpToTargetLevel(1, out bool a);

        //allTheBuff.pawnBuffType = PawnBuff.Substitute;
        //substitute.LevelUpToTargetLevel(3, out bool b);

        AllChessInit(targetDict);

        Player_ChessDictUpdate();
        playerCanvas.Init(this, choseBuffs);
        Player_InGameStart();
    }

    private void EatAllTheCurseChess()
    {
        List<ChessBasic> allTheCurseChess = new();
        foreach(ChessBasic chess in allTheChess.Values)
            if (chess.gotCurse) 
                allTheCurseChess.Add(chess);
        foreach (ChessBasic chess in allTheCurseChess)
        {
            _chessBoard.DeadEffect(chess);
            chess.GotEaten();

        }
    }

    private void Player_ChessDictUpdate()
    {
        allTheChess.Clear();
        allTheChess.AddRange(_chessBoard.ColorChessDict(usingChess));


        UpdateGuardianProtectArea();
    }

    public void Player_StopAllInput() => playerInPut.RejectInput();

    private void Player_InGameStart()
    {
        playerInPut.StartInput();
        UpdateGuardianProtectArea();
    }

    public void Player_TurnStart()
    {
        _inPutManager.PlayerInputStage(usingChess, InputStage.Waiting);
        Player_ChessDictUpdate();
    }
    public void Player_TurnEnd()
    {
        EatAllTheCurseChess();
        Player_ChessDictUpdate();
        _gameManager.EndTurn();
    }


}
