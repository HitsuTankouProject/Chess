using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

public enum PlayerStage { NoMyTurn,TurnInit,Ready,MovingChess,EatingChess,ReadytoEnd,End }

public class Player : MonoBehaviour
{
    private GameManager _gameManager => GameManager.Instance;
    private ChessBoard _chessBoard => _gameManager.chessBoard;
    private InPutManager _inPutManager => _gameManager.inPutManager;

    public ChessColor usingChess;

    public PlayerStage nowPlayerStage;

    public PlayerInPut playerInPut;
    public PlayerCanvas playerCanvas;


    public Dictionary<Vector2Int, ChessBasic> allTheChess { get; private set; } = new();

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

    #region Chess Buff
    public AllTheBuff allTheBuff = new();
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

    public HashSet<Vector2Int> guardianProtectArea { get; private set; } = new();
    public void AddToProtectArea(HashSet<Vector2Int> addToProtectArea) => guardianProtectArea.AddRange(addToProtectArea);
    public void UpdateGuardianProtectArea()
    {
        if (rookBuffType != RookBuff.Guardian) return;
        foreach (Vector2Int area in guardianProtectArea)
        {
            if (!_chessBoard.board.TryGetValue(area, out ChessBasic chess) || chess.color != usingChess) continue;
            chess.GotExtraLife(false);
        }
        guardianProtectArea.Clear();

        List<ChessBasic> rookList = ChessListByType(ChessType.Rook);

        if (rookList.Count == 0) return;
        foreach (ChessBasic chess in rookList)
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
    #region Knight
    public KnightBuff knightBuffType => allTheBuff.knightBuffType;
    public Charger charger => allTheBuff.charger;
    public Skirmisher skirmisher => allTheBuff.skirmisher;
    #endregion
    #region Pawn
    public PawnBuff pawnBuffType => allTheBuff.pawnBuffType;
    public Scout scout => allTheBuff.scout;
    public Substitute substitute => allTheBuff.substitute;
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
        allTheBuff.AllTheBuffInit(this);
        playerInPut.Init(this);
        playerInPut.StartInput();
    }

    public void Player_ChessInit(Dictionary<Vector2Int, ChessBasic> targetDict)
    {
        AllChessInit(targetDict);
        Player_ChessDictUpdate();
        playerCanvas.Init(this, choseBuffs);

        //queenBuffType = QueenBuff.Witcher;
        //witcher.LevelUpToTargetLevel(3, out bool a);

        //kingBuffType = KingBuff.MadKing;
        //madKing.LevelUpToTargetLevel(3, out bool a);

    }

    private void Player_ChessDictUpdate()
    {
        allTheChess = _chessBoard.ColorChessDict(usingChess);
        UpdateGuardianProtectArea();
    }

    public Coroutine turnStart;
    
    private bool turnCanEnd = false;
    private IEnumerator TurnStart()
    {
        
        yield return null;
        nowPlayerStage = PlayerStage.Ready;
        turnCanEnd = false;
        playerInPut.StartGame();
        while (!turnCanEnd)
        {
            switch (nowPlayerStage)
            {
                case PlayerStage.ReadytoEnd:
                    Debug.Log("ReadytoEnd");
                    turnCanEnd = true; 
                    break;
                default:break;
            }

            yield return null;

        }

        Player_ChessDictUpdate();
        nowPlayerStage = PlayerStage.End;

        _gameManager.EndTurn();
    }

    public void Player_TurnStart()
    {
        
        nowPlayerStage = PlayerStage.TurnInit;
        _inPutManager.PlayerInputStage(usingChess, InputStage.Waiting);
        turnStart = StartCoroutine(TurnStart());
        playerInPut.StartInput();
    }

}
