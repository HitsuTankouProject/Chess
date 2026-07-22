using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

/// <summary>  ゲーム全体の進行状態を表します。  </summary>
public enum GameStage
{
    /// <summary>　起動時の初期化処理中です。　</summary>
    Loading,
    /// <summary>タイトル画面を表示しています。</summary>
    GameTitle,
    /// <summary>ゲーム説明画面を表示しています。</summary>
    GameDescription,
    /// <summary>新しいゲームを開始しています。</summary>
    GameStart,
    /// <summary>ターン開始処理中です。</summary>
    TurnStart,
    /// <summary>プレイヤーがスキルを選択しています。</summary>
    SkillChoose,
    /// <summary>対局が進行中です。</summary>
    InGame,
    /// <summary>操作するプレイヤーを交代しています。</summary>
    TurnChange,
    /// <summary>現在のターンを終了しています。</summary>
    TurnEnd,
    /// <summary>ゲーム全体の終了処理中です。</summary>
    GameEnd,
    /// <summary>最終結果を表示しています。</summary>
    Release,
    /// <summary>進行不能なエラー状態です。</summary>
    Error
}

/// <summary> メインカメラの位置、角度、画角を保持します。 </summary>
public struct MainCameraView
{
    /// <summary>カメラのワールド座標を取得します。</summary>
    public Vector3 position { get; private set; }
    /// <summary>カメラのオイラー角を取得します。</summary>
    public Vector3 angle { get; private set; }
    /// <summary>カメラの画角を取得します。</summary>
    public float fieldOfView { get; private set; }
    /// <summary>  メインカメラの表示設定を初期化します。  </summary>
    public MainCameraView(Vector3 targetPos, Vector3 targetAngle, float targetFOV)
    {
        position = targetPos;
        angle = targetAngle;
        fieldOfView = targetFOV;
    }

}

/// <summary> プレイヤーカメラの表示状態を表します。 </summary>
public enum PlayerCameraStage 
{
    /// <summary>プレイヤー視点の通常表示です。</summary>
    Normal,
    /// <summary>駒を選択するための俯瞰表示です。</summary>
    Pick
}

/// <summary> プレイヤーカメラの位置と角度を保持します。 </summary>
public struct PlayerCameraView
{
    /// <summary>カメラのワールド座標を取得します。</summary>
    public Vector3 position { get; private set; }
    /// <summary>カメラのオイラー角を取得します。</summary>
    public Vector3 angle { get; private set; }
    /// <summary> プレイヤーカメラの表示設定を初期化します。 </summary>
    public PlayerCameraView(Vector3 targetPos, Vector3 targetAngle)
    {
        position = targetPos;
        angle = targetAngle;
    }

}

/// <summary>
/// ゲームの進行、カメラ、画面表示を一元管理します。
/// ゲームステージの切り替えを起点として、タイトル、スキル選択、対局、
/// ターン終了、リザルト表示までの一連の処理を制御します。
/// また、プレイヤーと盤面の初期化、手番と勝敗の管理、
/// メインカメラおよびプレイヤーカメラの切り替え、
/// 各種 UI パネル、言語設定、BGM、効果音の連携も担当します。
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>ゲームマネージャーの共有インスタンスを取得します。</summary>
    public static GameManager Instance {  get; private set; }
    /// <summary>ゲーム内で共有する画像、音声などのリソースです。</summary>
    public ResourcesData resourcesData;
    /// <summary>プレイヤー入力を管理するコンポーネントです。</summary>
    public InPutManager inPutManager;
    /// <summary>チェス盤と駒の状態を管理するコンポーネントです。</summary>
    public ChessBoard chessBoard;
    /// <summary>BGM と効果音を管理するコンポーネントです。</summary>
    public AudioManager audioManager;
    /// <summary>表示言語と翻訳リソースを管理するコンポーネントです。</summary>
    public LanguageManager languageManager;

    [Header("Cameras")]
    /// <summary>タイトルや説明画面で使用するメインカメラです。</summary>
    public Camera mainCamera;
    /// <summary>白プレイヤー側の盤面を表示するカメラです。</summary>
    public Camera player01Camera;
    /// <summary>黒プレイヤー側の盤面を表示するカメラです。</summary>
    public Camera player02Camera;
    // ===== Camera Views =====
    private MainCameraView gameTitleCameraView = new(new Vector3(-65, 89.1f, -57), new Vector3(55, 0, 0), 32.0f);
    private MainCameraView gameDescriptionCameraView = new(new Vector3(-242, 70, 80), new Vector3(90, -90, 0),50f);
    private MainCameraView turnStartCameraView = new(new Vector3(0, 75, 0), new Vector3(90, 0, 0), 80f);
    private MainCameraView skillChooseCameraView = new(new Vector3(-242, 70, 80), new Vector3(90, -90, 0), 50f);
    private MainCameraView releaseCameraView = new(new Vector3(242, 70, 80), new Vector3(90, 90, 0), 50f);
    private const float cameraTurnTime = 0.75f;

    // ===== Player =====
    [Header("Players")]
    /// <summary>白の駒を操作するプレイヤーです。</summary>
    public Player player01;
    /// <summary>黒の駒を操作するプレイヤーです。</summary>
    public Player player02;

    /// <summary>
    /// 指定した駒色に対応するプレイヤーを取得します。
    /// </summary
    public Player TargetPlayer(ChessColor chessColor)
    {
        if (chessColor == ChessColor.White) return player01;
        else return player02;
    }


    // ===== Player Camera Views =====
    private PlayerCameraView whiteView = new (new Vector3(0, 70, -44), new Vector3(55, 0, 0));
    private PlayerCameraView pickView_White = new (new Vector3(0, 90, 0), new Vector3(90, 0, 0));
    private PlayerCameraView blackView = new (new Vector3(0, 70, 44), new Vector3(55, 180, 0));
    private PlayerCameraView pickView_Black = new (new Vector3(0, 90, 0), new Vector3(90, 180, 0));

    /// <summary>現在のゲーム進行状態です。</summary>
    public GameStage nowGameStage;

    /// <summary>
    /// ゲームステージを切り替え、対応する処理を開始します。
    /// </summary>
    private async UniTask SwitchStage(GameStage targetStage)
    {
        await UniTask.Yield();
        nowGameStage = targetStage;
        switch (nowGameStage)
        {
            case GameStage.Loading:             Loading().Forget();             break;
            case GameStage.GameTitle:           GameTitle().Forget();           break;
            case GameStage.GameDescription:     GameDescription().Forget();     break;
            case GameStage.GameStart:           GameStart().Forget();           break;
            case GameStage.TurnStart:           TurnStart().Forget();           break;
            case GameStage.SkillChoose:         SkillChoose().Forget();         break;
            case GameStage.InGame:              InGame().Forget();              break;
            case GameStage.TurnEnd:             TurnEnd().Forget();             break;
            case GameStage.GameEnd:             GameEnd().Forget();             break;
            case GameStage.Release:             Release().Forget();             break;

            default:
                Debug.LogError("GameManager.SwitchStage: Invalid game stage.");
                return;
        }
    }

    #region MainCameraView
    /// <summary>
    /// ゲームステージに対応するメインカメラ設定を取得します。
    /// </summary>
    private MainCameraView TargetMainCameraView(GameStage gameStage)
    {
        switch (gameStage)
        {
            case GameStage.GameTitle:
                return gameTitleCameraView;
            case GameStage.GameDescription:
                return gameDescriptionCameraView;
            case GameStage.SkillChoose:
                return skillChooseCameraView;
            case GameStage.TurnStart:
                return turnStartCameraView;
            case GameStage.Release:
                return releaseCameraView;
            default:
                return new MainCameraView(
                    mainCamera.transform.position, 
                    mainCamera.transform.rotation.eulerAngles, 
                    mainCamera.fieldOfView);
        }
    }

    /// <summary>
    /// メインカメラに指定された表示設定を適用します。
    /// </summary
    private void SetMainCameraView(MainCameraView targetView)
    {
        mainCamera.transform.position = targetView.position;
        mainCamera.transform.rotation = Quaternion.Euler(targetView.angle);
        mainCamera.fieldOfView = targetView.fieldOfView;
    }
    /// <summary>
    /// メインカメラを指定ステージの表示設定へ補間移動します。
    /// </summary
    private async UniTask MainCameraTurn(GameStage gameStage)
    {
        MainCameraView targetView = TargetMainCameraView(gameStage);

        if (Vector3.Distance(mainCamera.transform.position, targetView.position) < 0.01f)
        {
            SetMainCameraView(targetView);
            return;
        }

        MainCameraView nowView =
            new MainCameraView(
                    mainCamera.transform.position, mainCamera.transform.rotation.eulerAngles, mainCamera.fieldOfView);

        float timer = 0;

        while (timer < cameraTurnTime)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / cameraTurnTime);

            mainCamera.transform.position =
                Vector3.Lerp(nowView.position, targetView.position, t);

            mainCamera.transform.rotation =
                Quaternion.Slerp(
                    Quaternion.Euler(nowView.angle), Quaternion.Euler(targetView.angle), t);

            mainCamera.fieldOfView = Mathf.Lerp(nowView.fieldOfView, targetView.fieldOfView, t);

            await UniTask.Yield();
        }
        SetMainCameraView(targetView);
    }


    #endregion

    #region PlayerCameraView 

    private const float playerCameraOpenTime = 0.5f;

    /// <summary>
    /// 指定したプレイヤーカメラの開閉アニメーションを実行します。
    /// </summary>
    private async UniTask TargetPlayerCameraOpen(Camera playerCamera, bool isOpen)
    {
        bool isWhite = playerCamera == player01Camera;

        float openedX = isWhite ? 0f : 0.5f;
        float closedX = isWhite ? -0.5f : 1f;

        bool isAlreadyOpen = playerCamera.enabled 
            && Mathf.Approximately(playerCamera.rect.x, openedX);
        if (isOpen && isAlreadyOpen) return;

        bool isAlreadyClosed = !playerCamera.enabled
            && Mathf.Approximately(playerCamera.rect.x, closedX);
        if (!isOpen && isAlreadyClosed) return;


        if (isOpen) playerCamera.enabled = true;

        Rect start = playerCamera.rect;

        Rect target = new Rect(isOpen ? openedX : closedX, 0f, 0.5f, 1f);

        float timer = 0f;

        while (timer < playerCameraOpenTime)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / playerCameraOpenTime);
            float x = Mathf.Lerp(start.x, target.x, progress);
            playerCamera.rect = new Rect(x, 0f, 0.5f, 1f);
            await UniTask.Yield();
        }

        playerCamera.rect = target;

        if (!isOpen) playerCamera.enabled = false;
    }
    /// <summary>
    /// 両プレイヤーカメラの開閉アニメーションを実行します。
    /// </summary
    public async UniTask PlayerCameraOpen(bool isOpen)
    {
        await TargetPlayerCameraOpen(player01Camera, isOpen);
        await TargetPlayerCameraOpen(player02Camera, isOpen);
    }
    /// <summary>
    /// 指定した駒色に対応するプレイヤーカメラを取得します。
    /// </summary>
    private Camera TargetCamera(ChessColor chessColor)
    {
        if(chessColor == ChessColor.White) return player01Camera;
        else return player02Camera;
    }
    /// <summary>
    /// 駒色とカメラ状態に対応する表示設定を取得します。
    /// </summary>
    private PlayerCameraView TargetPlayerCameraView(ChessColor color, PlayerCameraStage cameraStage)
    {
        switch (color)
        {
            case ChessColor.White:
                return cameraStage == PlayerCameraStage.Normal ? whiteView : pickView_White;
            case ChessColor.Black:
                return cameraStage == PlayerCameraStage.Normal ? blackView : pickView_Black;
            default:
                Debug.LogError("TargetPlayerCameraView: Invalid chess color.");
                return default;
        }
    }

    /// <summary>
    /// 指定したプレイヤーカメラを目標の表示設定へ補間移動します。
    /// </summary>
    private async UniTask PlayerCameraTurn(Camera playerCamera, Pair<ChessColor, PlayerCameraStage> playerCameraStagePair)
    {
        if (playerCamera != player01Camera && playerCamera != player02Camera)
        {
            Debug.LogError("PlayerCameraTurn: Invalid player camera.");
            return;
        }

        PlayerCameraView targetView = TargetPlayerCameraView(playerCameraStagePair.first, playerCameraStagePair.second);
        if (Vector3.Distance(playerCamera.transform.position, targetView.position) < 0.01f)
        {
            playerCamera.transform.position = targetView.position;
            playerCamera.transform.rotation = Quaternion.Euler(targetView.angle);
            return;
        }
        PlayerCameraView nowView =
            new PlayerCameraView(playerCamera.transform.position, playerCamera.transform.rotation.eulerAngles);

        float timer = 0;
        while (timer < cameraTurnTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / cameraTurnTime);

            playerCamera.transform.position =
                Vector3.Lerp(nowView.position, targetView.position, t);
            playerCamera.transform.rotation =
                Quaternion.Slerp(Quaternion.Euler(nowView.angle), Quaternion.Euler(targetView.angle), t);
            await UniTask.Yield();
        }
        playerCamera.transform.position = targetView.position;
        playerCamera.transform.rotation = Quaternion.Euler(targetView.angle);
    }
    
    /// <summary>
    /// 指定したプレイヤーのカメラ移動を開始します。
    /// </summary>
    public void PlayerCameraTurn(Pair<ChessColor, PlayerCameraStage> playerCameraStagePair)
    {
        Camera camera = TargetCamera(playerCameraStagePair.first);
        PlayerCameraTurn(camera, playerCameraStagePair).Forget();
    }


    #endregion

    #region Button
    /// <summary>
    /// ボタン押下時の効果音を再生します。
    /// </summary>
    public void PlayButtonSfx() => audioManager.PlaySfx(resourcesData.sfx_PressButton);
    /// <summary>
    /// すべての主要パネルを閉じます。
    /// </summary>
    private void AllPanelClose()
    {
        gameTitlePanel.SetActive(false);
        gameDescriptionPanel.gameObject.SetActive(false);
        gameReleasePanel.SetActive(false);
        inGamePanel.gameObject.SetActive(false);
    }
    /// <summary>
    /// ゲームタイトル画面へ戻ります。
    /// </summary>
    public void Button_BackToGameTitle()
    {
        PlayButtonSfx();
        AllPanelClose();
        SwitchStage(GameStage.GameTitle).Forget();
    }
    /// <summary>
    /// ゲーム説明画面へ戻ります。
    /// </summary>
    public void Button_BackToGameDescription()
    {
        PlayButtonSfx();
        AllPanelClose();
        SwitchStage(GameStage.GameDescription).Forget();

    }
    /// <summary>
    /// ゲーム開始処理へ進みます。
    /// </summary>
    public void Button_BackToGameStart()
    {
        PlayButtonSfx();
        AllPanelClose();
        SwitchStage(GameStage.GameStart).Forget();
    }
    /// <summary>
    /// リザルト画面へ戻ります。
    /// </summary>
    public void Button_BackToRelease()
    {
        PlayButtonSfx();
        AllPanelClose();
        SwitchStage(GameStage.Release).Forget();
    }
    /// <summary>
    /// アプリケーションを終了します。
    /// </summary>
    public void Button_Exit() => Application.Quit();
    /// <summary>
    /// 表示言語を日本語へ変更します。
    /// </summary>
    public void Button_ChangeToJapanese()
    {
        audioManager.PlaySfx(resourcesData.sfx_ChangeLanguage);
        languageManager.ChangeLanguage(Language.Japanese);
        ChangeLanguage();
    }
    /// <summary>
    /// 表示言語を英語へ変更します。
    /// </summary>
    public void Button_ChangeToEnglish()
    {
        audioManager.PlaySfx(resourcesData.sfx_ChangeLanguage);
        languageManager.ChangeLanguage(Language.English);
        ChangeLanguage();
    }


    #endregion

    #region Loading
    [Header("Loading")]
    /// <summary>読み込み中に表示するオブジェクトです。</summary>
    public GameObject loading;
    /// <summary>ゲーム開始ボタンの画像です。</summary>
    public Image button_GameStart;
    /// <summary>ゲーム説明ボタンの画像です。</summary>
    public Image button_GameDescription;
    /// <summary>リザルト画面の勝者ロゴ画像です。</summary>
    public Image button_WinnerLogo;
    /// <summary>タイトルへ戻るボタンの画像です。</summary>
    public Image button_GameTitle;
    /// <summary>再開ボタンの画像です。</summary>
    public Image button_Resume;
    /// <summary>終了ボタンの画像です。</summary>
    public Image button_Quit;

    /// <summary>
    /// ゲームで使用する各種データと管理オブジェクトを初期化します。
    /// </summary>
    private async UniTask Loading()
    {
        await UniTask.Yield();
        loading.SetActive(true);
        resourcesData.ResourcesInit();

        languageManager.Init();
        inPutManager.Init();

        chessBoard.ChessBoard_Init();

        TargetPlayer(ChessColor.White).Player_Init(ChessColor.White);
        TargetPlayer(ChessColor.Black).Player_Init(ChessColor.Black);

        await SwitchStage(GameStage.GameTitle);

    }
    /// <summary>
    /// 現在の言語設定に合わせて UI 画像を更新します。
    /// </summary>
    private void ChangeLanguage()
    {
        button_GameStart.sprite = languageManager.sp_GameStart;
        button_GameDescription.sprite = languageManager.sp_Description;
        button_WinnerLogo.sprite = languageManager.sp_Release_Winner;
        button_GameTitle.sprite = languageManager.sp_GameTitle;
        button_Resume.sprite = languageManager.sp_Button_Resume;
        button_Quit.sprite = languageManager.sp_Button_Quit;
    }

    #endregion

    #region GameTitle
    [Header("GameTitle")]
    /// <summary>タイトル画面全体のパネルです。</summary>
    public GameObject gameTitlePanel;
    /// <summary>
    /// ゲームタイトル画面を表示します。
    /// </summary>
    private async UniTask GameTitle()
    {
        await UniTask.Yield();
        await PlayerCameraOpen(false);
        ChangeLanguage();
        await MainCameraTurn(GameStage.GameTitle);
        if (!audioManager.IsBgmPlaying())
        {
            audioManager.PlayMusic(resourcesData.bgm_game);
        }
        loading.SetActive(false);
        gameTitlePanel.SetActive(true);

        WaitGamePadInput_GameTitle().Forget();

    }
    /// <summary>
    /// タイトル画面でゲームパッド入力を待機します。
    /// </summary>
    private async UniTask WaitGamePadInput_GameTitle()
    {
        while(nowGameStage == GameStage.GameTitle)
        {
            ButtonControl button =  await inPutManager.WaitForGamePadButtonInput();
            if (button == null) continue;
            switch (button.name)
            {
                case "start":           Button_BackToGameDescription();     return;
                case "buttonSouth":     Button_BackToGameStart();           return;

                case "rightShoulder":   Button_ChangeToEnglish();           continue;
                case "leftShoulder":   Button_ChangeToJapanese();           continue;


                default:                await UniTask.Yield();              continue;
            }
        }
    }



    #endregion

    #region GameDescription
    [Header("GameDescription")]
    /// <summary>ルールや操作方法を表示する説明パネルです。</summary>
    public DescriptionPanel gameDescriptionPanel;
    /// <summary>
    /// ゲーム説明画面を表示します。
    /// </summary>
    private async UniTask GameDescription()
    {
        await UniTask.Yield();
        await MainCameraTurn(GameStage.GameDescription);
        gameDescriptionPanel.gameObject.SetActive(true);
        gameDescriptionPanel.Init();

    }

    #endregion

    #region GameStart
    /// <summary>
    /// 対局開始時の状態を初期化します。
    /// </summary>
    private async UniTask GameStart()
    {
        audioManager.PlaySfx(resourcesData.sfx_GameStart);
        player01.AllTheBuffInit();
        player02.AllTheBuffInit();


        await MainCameraTurn(GameStage.TurnStart);
        //await PlayerCameraOpen(true);
        nowTurnCount = 1;
        turnResult.Clear();
        await SwitchStage(GameStage.TurnStart);
    }

    #endregion

    #region TurnStart
    /// <summary>
    /// 新しいターンに必要な状態を初期化します。
    /// </summary>
    private async UniTask TurnInit()
    {
        player01.haveKing = true;
        player02.haveKing = true;

        await chessBoard.ChessBoard_TurnInit(nowTurnCount);
    }

    /// <summary>
    /// バフの最大選択数
    /// </summary>
    private const int maxBuffCount = 3;
    /// <summary>
    /// 両プレイヤーが最大数のバフを選択済みか判定します。
    /// </summary>
    private bool IsMaxBuffCount()
        => player01.choseBuffs.Count >= maxBuffCount && player02.choseBuffs.Count >= maxBuffCount;
    /// <summary>
    /// ターンを開始し、次のゲームステージへ進めます。
    /// </summary>
    private async UniTask TurnStart()
    {
        await TurnInit();

        if (!IsMaxBuffCount()) 
            SwitchStage(GameStage.SkillChoose).Forget();
        else 
        SwitchStage(GameStage.InGame).Forget();

    }

    #endregion

    #region SkillChoose
    [Header("SkillChoose")]
    /// <summary>両プレイヤーのスキル選択を管理するパネルです。</summary>
    public ChooseSkillPanel chooseSkillPanel;
    /// <summary>
    /// スキル選択画面を
    /// <summary>
    private async UniTask SkillChoose()
    {
        await PlayerCameraOpen(false);
        await MainCameraTurn(GameStage.SkillChoose);
        chooseSkillPanel.gameObject.SetActive(true);
        chooseSkillPanel.Init();
    }

    /// <summary>
    /// 対局画面へ移行するためのカメラ準備を行います。
    /// </summary>
    private async UniTask ReadyForInGame()
    {
        await MainCameraTurn(GameStage.TurnStart);
        await PlayerCameraOpen(true);

        await SwitchStage(GameStage.InGame);
    }

    /// <summary>
    /// 選択されたバフを各プレイヤーへ適用し、対局を開始します。
    /// </summary>
    public void EndSkillChoose(AllBuffCard player01Pick, AllBuffCard player02Pick)
    {
        player01.ChooseBuff(player01Pick);
        player02.ChooseBuff(player02Pick);

        ReadyForInGame().Forget();

    }

    #endregion

    #region InGame
    [Header("InGame")]
    /// <summary>対局中の情報と操作 UI を表示するパネルです。</summary>
    public IngamePanel inGamePanel;
    /// <summary>現在操作中のプレイヤーの駒色を取得します。</summary>
    public ChessColor nowTurn { get; private set; } = ChessColor.White;
    /// <summary> 最大ターン数 </summary>
    private const int maxTurnCount = 3;
    /// <summary> 今のターン </summary>
    private int nowTurnCount = 1;
    /// <summary>
    /// 手番を交代し、次のプレイヤーのターンを開始します。
    /// </summary>
    private async UniTask TurnChange()
    {

        nowGameStage = GameStage.TurnChange;
        nowTurn = nowTurn == ChessColor.White ? ChessColor.Black : ChessColor.White;

        audioManager.PlaySfx(resourcesData.sfx_GameStart);
        await inGamePanel.TurnChange(nowTurn);

        TargetPlayer(nowTurn).Player_TurnStart();
        nowGameStage = GameStage.InGame;
    }
    /// <summary>
    /// 現在の手番を終了し、勝敗判定または手番交代を行います。
    /// </summary>
    public void EndTurn()
    {
        chessBoard.UpdatePlayerChose(new Vector2Int(-1, -1));

        inPutManager.PlayerInputStage(nowTurn, InputStage.None);

        if (!player01.haveKing || !player02.haveKing) EndInGame(nowTurn);
        else if (chessBoard.board.Count <= 2) EndInGame(ChessColor.None);
        else TurnChange().Forget();
    }
    /// <summary>
    /// 盤面とプレイヤーの駒情報を初期化して対局を開始します。
    /// </summary>
    private async UniTask InGame()
    {
        await UniTask.Yield();

        Dictionary<Vector2Int, ChessBasic> whiteChess = new Dictionary<Vector2Int, ChessBasic>();
        Dictionary<Vector2Int, ChessBasic> blackChess = new Dictionary<Vector2Int, ChessBasic>();

        //Debug.Log(chessBoard.board.Count);

        foreach (Vector2Int chessPos in chessBoard.board.Keys)
        {
            //Debug.Log($"{chessBoard.board[chessPos].color.ToString()} : {chessBoard.board[chessPos].type.ToString()} + {chessPos}");

            if (chessBoard.board[chessPos].color == ChessColor.White)
                whiteChess[chessPos] = chessBoard.board[chessPos];
            else if (chessBoard.board[chessPos].color == ChessColor.Black)
                blackChess[chessPos] = chessBoard.board[chessPos];
        }
        player01.Player_ChessInit(whiteChess);
        player02.Player_ChessInit(blackChess);

        nowTurn = ChessColor.White;
        TargetPlayer(nowTurn).Player_TurnStart();

        inGamePanel.gameObject.SetActive(true);
        inGamePanel.Init();
    }

    /// <summary>
    /// 盤面とプレイヤーの駒情報を初期化して対局を開始します。
    /// </summary
    public void EndInGame(ChessColor chessColor)
    {
        turnResult[nowTurnCount] = chessColor;
        SwitchStage(GameStage.TurnEnd).Forget();
    }

    #endregion

    #region TurnEnd 
    /// <summary>　勝者　</summary>
    private ChessColor winner;
    /// <summary>
    /// 規定ターン数と勝利数からゲーム終了条件を判定します。
    /// </summary>
    private bool IsGameEnd()
    {
        if (nowTurnCount < maxTurnCount) return false;
        int whiteWinCount = 0;
        int blackWinCount = 0;
        foreach (var result in turnResult.Values)
        {
            if (result == ChessColor.White) whiteWinCount++;
            else if (result == ChessColor.Black) blackWinCount++;
        }
        bool bothLessThenTwo = whiteWinCount < 2 && blackWinCount < 2;

        return !bothLessThenTwo;

    }
    /// <summary>
    /// 記録された対局結果から最終勝者を決定します。
    /// </summary>
    private void DetermineWinner()
    {
        int whiteWinCount = 0;
        int blackWinCount = 0;
        foreach (var result in turnResult.Values)
        {
            if (result == ChessColor.White) whiteWinCount++;
            else if (result == ChessColor.Black) blackWinCount++;
        }
        winner = whiteWinCount > blackWinCount ? ChessColor.White : ChessColor.Black;
    }
    /// <summary>
    /// ターン終了後の勝敗判定と次ターンの準備を行います。
    /// </summary>
    private async UniTask TurnEnd()
    {
        await UniTask.Yield();
        AllPanelClose();
        if (IsGameEnd())
        {
            //Debug.Log("Real GameSet");
            DetermineWinner();
            SwitchStage(GameStage.GameEnd).Forget();
            return;

        }

        //Debug.Log("GameSet!!");
        nowTurnCount++;
        player01.AllBuffLevelUp();
        player02.AllBuffLevelUp();

        SwitchStage(GameStage.TurnStart).Forget();

    }
    /// <summary>
    /// 指定したプレイヤーの投了を処理して勝者を決定します。
    /// </summary>
    public void Surrender(ChessColor surrender)
    {
        Debug.Log("Real GameSet");
        winner = surrender == ChessColor.White ? ChessColor.Black : ChessColor.White;
        SwitchStage(GameStage.GameEnd).Forget();
    }


    #endregion

    #region GameEnd
    /// <summary>
    /// ゲーム終了時の後処理を行い、リザルト画面へ進みます。
    /// </summary>
    private async UniTask GameEnd()
    {

        await UniTask.Yield();
        inGamePanel.gameObject.SetActive(false);
        player01.Player_StopAllInput();
        player02.Player_StopAllInput();
        await PlayerCameraOpen(false);
        await MainCameraTurn(GameStage.Release);
        chessBoard.CleanTheBoard();
        SwitchStage(GameStage.Release).Forget();

    }


    #endregion

    #region Release
    [Header("Release")]
    /// <summary>最終結果を表示するリザルトパネルです。</summary>
    public GameObject gameReleasePanel;
    /// <summary>勝者を示すタグ画像です。</summary>
    public Image winnerTag;
    /// <summary>白プレイヤーの勝利数を示す画像です。</summary>
    public Image whiteChessResult;
    /// <summary>黒プレイヤーの勝利数を示す画像です。</summary>
    public Image blackChessResult;

    

    /// <summary>　全ての結果　 </summary>
    private Dictionary<int, ChessColor> turnResult = new();
    /// <summary>
    /// 対局結果と勝者をリザルト画面へ反映します。
    /// </summary>
    private void ShowGameResult()
    {
        int whiteWinCount = 0;
        int blackWinCount = 0;
        foreach (var result in turnResult.Values)
        {
            if (result == ChessColor.White) whiteWinCount++;
            else if (result == ChessColor.Black) blackWinCount++;
        }
 
        whiteChessResult.sprite = resourcesData.allSprite.sp_NumberSprites[whiteWinCount];
        blackChessResult.sprite = resourcesData.allSprite.sp_NumberSprites[blackWinCount];

        winnerTag.sprite = resourcesData.PlayerSprite(winner);

    }
    /// <summary>
    /// リザルト画面を表示します。
    /// </summary>
    private async UniTask Release()
    {
        await UniTask.Yield();
        audioManager.PlaySfx(resourcesData.sfx_Release);
        ShowGameResult();
        gameReleasePanel.SetActive(true);
        WaitGamePadInput_Release().Forget();

    }
    /// <summary>
    /// リザルト画面でゲームパッド入力を待機します。
    /// </summary>
    private async UniTask WaitGamePadInput_Release()
    {
        while (nowGameStage == GameStage.Release)
        {
            ButtonControl button = await inPutManager.WaitForGamePadButtonInput();
            await UniTask.Yield();
            if (button == null) continue;

            switch (button.name)
            {
                case "leftShoulder":    Button_BackToGameTitle();   return;
                //case "start":           Button_BackToGameStart();   return;
                case "rightShoulder":   Button_Exit();              return;

                default: await UniTask.Yield(); continue;
            }
        }
    }



    #endregion

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    
    private void Start()
    {
        SwitchStage(GameStage.Loading).Forget();
    }


}
