using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.VisualScripting;

/// <summary>
/// 1人分のプレイヤー状態、所有駒、バフ、入力、UIを管理します。
/// 駒色に対応する盤面上の駒を同期し、駒種別のバフへアクセスする窓口を提供します。
/// 手番開始時の入力切り替え、手番終了時の呪われた駒の除去、
/// Guardian保護範囲の更新、カメラ切り替え、ゲーム進行への通知を担当します。
/// </summary>
public class Player : MonoBehaviour
{
    /// <summary>ゲーム全体を管理する共有インスタンスを取得します。</summary>
    private GameManager _gameManager => GameManager.Instance;
    /// <summary>盤面と駒配置を管理するオブジェクトを取得します。</summary>
    private ChessBoard _chessBoard => _gameManager.chessBoard;
    /// <summary>両プレイヤーの入力ステージを管理するオブジェクトを取得します。</summary>
    private InPutManager _inPutManager => _gameManager.inPutManager;
    /// <summary>このプレイヤーが操作する駒色です。</summary>
    public ChessColor usingChess;
    /// <summary>このプレイヤーのマウス・ゲームパッド入力を管理します。</summary>
    public PlayerInPut playerInPut;
    /// <summary>このプレイヤーの対局UIを管理します。</summary>
    public PlayerCanvas playerCanvas;
    /// <summary>このプレイヤーが現在ポーズ中かどうかを取得します。</summary>
    public bool isPause => playerCanvas.isPause;
    /// <summary>このプレイヤーが所有する盤面座標と駒の対応表を取得します。</summary>
    public Dictionary<Vector2Int, ChessBasic> allTheChess { get; private set; } = new();
    /// <summary>所有駒辞書を指定内容へ置き換え、各駒へこのプレイヤーを設定します。</summary>
    /// <param name="targetDict">このプレイヤーが所有する駒の辞書です。</param>
    public void AllChessInit(Dictionary<Vector2Int, ChessBasic> targetDict)
    {
        allTheChess.Clear();
        allTheChess.AddRange(targetDict);
        foreach (ChessBasic chess in allTheChess.Values)
        {
            chess.ChessInit(this);
        }
    }
    /// <summary>所有駒から指定種類の駒だけを抽出します。</summary>
    /// <param name="chessType">抽出する駒の種類です。</param>
    /// <returns>指定種類に一致する所有駒の一覧です。</returns>
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
    /// <summary>このプレイヤーのキングが盤面上に存在するかどうかを示します。</summary>
    public bool haveKing = true;
    /// <summary>このプレイヤーが所有するすべての駒バフです。</summary>
    public AllTheBuff allTheBuff = new();
    /// <summary>すべての駒バフをこのプレイヤー用に初期化します。</summary>
    public void AllTheBuffInit() => allTheBuff.AllTheBuffInit(this);

    #region King
    /// <summary>現在選択中のキングバフを取得します。</summary>
    public KingBuff kingBuffType => allTheBuff.kingBuffType;
    /// <summary>MadKingバフを取得します。</summary>
    public MadKing madKing => allTheBuff.madKing;
    /// <summary>SageKingバフを取得します。</summary>
    public SageKing sageKing => allTheBuff.sageKing;
    #endregion

    #region Queen
    /// <summary>現在選択中のクイーンバフを取得します。</summary>
    public QueenBuff queenBuffType => allTheBuff.queenBuffType;
    /// <summary>Witcherバフを取得します。</summary>
    public Witcher witcher => allTheBuff.witcher;
    /// <summary>Beautyバフを取得します。</summary>
    public Beauty beauty => allTheBuff.beauty;
    #endregion

    #region Bishop
    /// <summary>現在選択中のビショップバフを取得します。</summary>
    public BishopBuff bishopBuffType => allTheBuff.bishopBuffType;
    /// <summary>Sorcererバフを取得します。</summary>
    public Sorcerer sorcerer => allTheBuff.sorcerer;
    /// <summary>Monkバフを取得します。</summary>
    public Monk monk => allTheBuff.monk;
    #endregion

    #region Rook
    /// <summary>現在選択中のルークバフを取得します。</summary>
    public RookBuff rookBuffType => allTheBuff.rookBuffType;
    /// <summary>Rusherバフを取得します。</summary>
    public Rusher rusher => allTheBuff.rusher;
    /// <summary>Guardianバフを取得します。</summary>
    public Guardian guardian => allTheBuff.guardian;

    /// <summary>
    /// 指定駒をGuardianの保護対象へ追加します。
    /// </summary>
    public void AddGuardianProtectedChess(ChessBasic chess) => allTheBuff.AddGuardianProtectedChess(chess);
    /// <summary>
    /// 所有する全ルークの現在位置からGuardianの保護範囲を再計算します。
    /// </summary>
    public void UpdateGuardianProtectArea() => allTheBuff.UpdateGuardianProtectArea(ChessListByType(ChessType.Rook));
    #endregion

    #region Knight
    /// <summary>現在選択中のナイトバフを取得します。</summary>
    public KnightBuff knightBuffType => allTheBuff.knightBuffType;
    /// <summary>Chargerバフを取得します。</summary>
    public Charger charger => allTheBuff.charger;
    /// <summary>Skirmisherバフを取得します。</summary>
    public Skirmisher skirmisher => allTheBuff.skirmisher;
    #endregion

    #region Pawn
    /// <summary>現在選択中のポーンバフを取得します。</summary>
    public PawnBuff pawnBuffType => allTheBuff.pawnBuffType;
    /// <summary>Scoutバフを取得します。</summary>
    public Scout scout => allTheBuff.scout;
    /// <summary>Substituteバフを取得します。</summary>
    public Substitute substitute => allTheBuff.substitute;

    /// <summary>
    /// 所有ポーンからキングの身代わりにできる駒を取得します。
    /// </summary>
    public bool IsProtectbySubstitute(out ChessBasic chess) =>
        allTheBuff.IsProtectbySubstitute(ChessListByType(ChessType.Pawn), out chess);
    #endregion

    /// <summary>カード種別とバフの対応表を取得します。</summary>
    public Dictionary<AllBuffCard, BuffBasic> cardBuffMap => allTheBuff.cardBuffMap;
    /// <summary>このプレイヤーが選択済みのバフカードを取得します。</summary>
    public List<AllBuffCard> choseBuffs => allTheBuff.choseBuffs;
    /// <summary>
    /// 現在選択中のすべてのバフを1段階上げます。
    /// </summary>
    public void AllBuffLevelUp() => allTheBuff.AllBuffLevelUp();
    /// <summary>指定バフカードを選択して初期レベルを設定します。</summary>
    public void ChooseBuff(AllBuffCard choseBuff) => allTheBuff.ChooseBuff(choseBuff);
    #endregion

    /// <summary>このプレイヤーのカメラを指定状態へ切り替えます。</summary>
    public void TurnCamera(PlayerCameraStage playerCameraStage)=>
        _gameManager.PlayerCameraTurn(new Pair<ChessColor, PlayerCameraStage>(usingChess, playerCameraStage));

    /// <summary>
    /// 操作する駒色と入力管理を初期化します。
    /// </summary>
    public void Player_Init(ChessColor targetChess)
    {
        usingChess = targetChess;
        playerInPut.Init(this);
        playerInPut.StartInput();
    }
    /// <summary>
    /// 対局開始時の所有駒、UI、入力、バフ関連状態を準備します。
    /// </summary>
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
    /// <summary>
    /// 所有する呪われた駒をすべて盤面から取り除きます。
    /// </summary>
    private void EatAllTheCurseChess()
    {
        List<ChessBasic> allTheCurseChess = new();
        foreach(ChessBasic chess in allTheChess.Values)
            if (chess.gotCurse) 
                allTheCurseChess.Add(chess);
        // 列挙中の辞書変更を避けるため、収集後に駒を取り除きます。
        foreach (ChessBasic chess in allTheCurseChess)
        {
            _chessBoard.DeadEffect(chess);
            chess.GotEaten();

        }
    }
    /// <summary>
    /// 盤面から最新の所有駒を再取得し、Guardianの保護範囲を更新します。
    /// </summary>
    private void Player_ChessDictUpdate()
    {
        allTheChess.Clear();
        allTheChess.AddRange(_chessBoard.ColorChessDict(usingChess));


        UpdateGuardianProtectArea();
    }

    /// <summary>このプレイヤーへの入力受付を停止します。</summary>
    public void Player_StopAllInput() => playerInPut.RejectInput();
    /// <summary>
    /// 対局入力を開始し、Guardianの保護範囲を初期適用します。
    /// </summary>
    private void Player_InGameStart()
    {
        playerInPut.StartInput();
        UpdateGuardianProtectArea();
    }
    /// <summary>
    /// このプレイヤーの手番入力を待機状態にし、所有駒を更新します。
    /// </summary>
    public void Player_TurnStart()
    {
        _inPutManager.PlayerInputStage(usingChess, InputStage.Waiting);
        Player_ChessDictUpdate();
    }
    /// <summary>
    /// 呪われた駒を除去し、所有駒を更新してゲーム管理へ手番終了を通知します。
    /// </summary>
    public void Player_TurnEnd()
    {
        EatAllTheCurseChess();
        Player_ChessDictUpdate();
        _gameManager.EndTurn();
    }


}
