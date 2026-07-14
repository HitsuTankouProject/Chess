using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Linq;
using UniRx;

public enum GameStage
{ 
    Loading, 
    GameTitle,
    GameDescription,
    GameStart,
    TurnStart,
    SkillChoose,
    InGame,
    TurnChange,
    TurnEnd,
    GameEnd, 
    Release,
    Error
}

[System.Serializable]
public struct Pair<F, S>
{
    //public Pair()
    //{ }
    public Pair(F f, S s)
    {
        this.first = f;
        this.second = s;
    }
    public F first;
    public S second;

    public override bool Equals(object obj)
    {
        if (obj is Pair<F, S> other)
        {
            return EqualityComparer<F>.Default.Equals(first, other.first)
                && EqualityComparer<S>.Default.Equals(second, other.second);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(first, second);
    }

    public static bool operator ==(Pair<F, S> a, Pair<F, S> b)
    {
        return a.first.Equals(b.first) && a.second.Equals(b.second);
    }
    public static bool operator !=(Pair<F, S> a, Pair<F, S> b)
    {
        return !a.first.Equals(b.first) || !a.second.Equals(b.second);
    }

}
public struct MainCameraView
{
    public Vector3 position { get; private set; }
    public Vector3 angle { get; private set; }
    public float fieldOfView { get; private set; }
    public MainCameraView(Vector3 targetPos, Vector3 targetAngle, float targetFOV)
    {
        position = targetPos;
        angle = targetAngle;
        fieldOfView = targetFOV;
    }

}
public enum PlayerCameraStage { Normal, Pick }
public struct PlayerCameraView
{
    public Vector3 position { get; private set; }
    public Vector3 angle { get; private set; }
    public PlayerCameraView(Vector3 targetPos, Vector3 targetAngle)
    {
        position = targetPos;
        angle = targetAngle;
    }

}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {  get; private set; }
    public ResourcesData resourcesData;
    public InPutManager inPutManager;
    public ChessBoard chessBoard;



    [Header("Cameras")]
    public Camera mainCamera;
    public Camera player01Camera;
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
    public Player player01; 
    public Player player02;
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

    public GameStage nowGameStage;

    private async UniTask SwitchStage(GameStage targetStage)
    {
        nowGameStage = targetStage;
        loading.SetActive(true);
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
        loading.SetActive(false);
    }


    #region MainCameraView
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
    private void SetMainCameraView(MainCameraView targetView)
    {
        mainCamera.transform.position = targetView.position;
        mainCamera.transform.rotation = Quaternion.Euler(targetView.angle);
        mainCamera.fieldOfView = targetView.fieldOfView;
    }
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

    private const float openRectX = 0.5f;
    private const float closeRectX = -1f;
    private async UniTask TargetPlayerCameraOpen(Camera playerCamera, bool isOpen)
    {
        bool isWhite = playerCamera == player01Camera;

        Rect start = playerCamera.rect;
        Rect target = start;

        if (isWhite)
        {
            target.width = isOpen ? openRectX : closeRectX;
        }
        else
        {
            target.x = isOpen ? openRectX : 1f;
        }

        float timer = 0f;

        while (timer < playerCameraOpenTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / playerCameraOpenTime);

            Rect rect = start;

            if (isWhite)
            {
                rect.width = Mathf.Lerp(start.width, target.width, t);
            }
            else
            {
                rect.x = Mathf.Lerp(start.x, target.x, t);
            }

            playerCamera.rect = rect;

            await UniTask.Yield();
        }

        playerCamera.rect = target;
    }
    private async UniTask PlayerCameraOpen(bool isOpen)
    {
        await TargetPlayerCameraOpen(player01Camera, isOpen);
        await TargetPlayerCameraOpen(player02Camera, isOpen);
    }

    private Camera TargetCamera(ChessColor chessColor)
    {
        if(chessColor == ChessColor.White) return player01Camera;
        else return player02Camera;
    }

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


    public void PlayerCameraTurn(Pair<ChessColor, PlayerCameraStage> playerCameraStagePair)
    {
        Camera camera = TargetCamera(playerCameraStagePair.first);
        PlayerCameraTurn(camera, playerCameraStagePair).Forget();
    }


    #endregion

    #region Button


    private void AllPanelClose()
    {
        gameTitlePanel.SetActive(false);
        gameDescriptionPanel.gameObject.SetActive(false);
        gameReleasePanel.SetActive(false);
        inGamePanel.gameObject.SetActive(false);
    }
    public void Button_BackToGameTitle()
    {
        AllPanelClose();
        SwitchStage(GameStage.GameTitle).Forget();
    }
    public void Button_BackToGameDescription()
    {
        AllPanelClose();
        SwitchStage(GameStage.GameDescription).Forget();

    }
    public void Button_BackToGameStart()
    {
        AllPanelClose();
        SwitchStage(GameStage.GameStart).Forget();
    }
    public void Button_BackToRelease()
    {
        AllPanelClose();
        SwitchStage(GameStage.Release).Forget();
    }
    public void Button_Exit() => Application.Quit();



    #endregion

    #region Loading
    [Header("Loading")]
    public GameObject loading;

    private async UniTask Loading()
    {
        await UniTask.Yield();
        resourcesData.ResourcesInit();
        inPutManager.Init();

        chessBoard.ChessBoard_Init();
        TargetPlayer(ChessColor.White).Player_Init(ChessColor.White);
        TargetPlayer(ChessColor.Black).Player_Init(ChessColor.Black);

        SwitchStage(GameStage.GameTitle).Forget();
    }


    #endregion

    #region GameTitle
    [Header("GameTitle")]
    public GameObject gameTitlePanel;

    private async UniTask GameTitle()
    {
        await UniTask.Yield();
        await MainCameraTurn(GameStage.GameTitle);
        gameTitlePanel.SetActive(true);
        WaitGamePadInput_GameTitle().Forget();

    }

    private async UniTask WaitGamePadInput_GameTitle()
    {
        while(nowGameStage == GameStage.GameTitle)
        {
            ButtonControl button =  await inPutManager.WaitForGamePadButtonInput();
            if (button == null) continue;
            switch (button.name)
            {
                case "buttonWest":      Button_BackToGameDescription();     return;
                case "buttonSouth":     Button_BackToGameStart();           return;
                default:                await UniTask.Yield();              continue;
            }
        }
    }



    #endregion

    #region GameDescription
    [Header("GameDescription")]
    public DescriptionPanel gameDescriptionPanel;


    private async UniTask GameDescription()
    {
        await UniTask.Yield();
        await MainCameraTurn(GameStage.GameDescription);
        gameDescriptionPanel.gameObject.SetActive(true);
        gameDescriptionPanel.Init();

    }

    #endregion

    #region GameStart

    private async UniTask GameStart()
    {
        await MainCameraTurn(GameStage.TurnStart);
        //await PlayerCameraOpen(true);
        nowTurnCount = 1;
        turnResult.Clear();
        await SwitchStage(GameStage.TurnStart);
    }

    #endregion

    #region TurnStart

    private async UniTask TurnInit()
    {
        player01.haveKing = true;
        player02.haveKing = true;

        await chessBoard.ChessBoard_TurnInit(nowTurnCount);
    }


    private const int maxBuffCount = 3;
    private bool IsMaxBuffCount()
        => player01.choseBuffs.Count >= maxBuffCount && player02.choseBuffs.Count >= maxBuffCount;
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
    public ChooseSkillPanel chooseSkillPanel;
    public bool isPicking => chooseSkillPanel.isPicking;

    private async UniTask SkillChoose()
    {
        await PlayerCameraOpen(false);
        await MainCameraTurn(GameStage.SkillChoose);
        chooseSkillPanel.gameObject.SetActive(true);
        chooseSkillPanel.Init();
    }


    private async UniTask ReadyForInGame()
    {
        await MainCameraTurn(GameStage.TurnStart);
        await PlayerCameraOpen(true);

        await SwitchStage(GameStage.InGame);
    }


    public void EndSkillChoose(AllBuffCard player01Pick, AllBuffCard player02Pick)
    {
        player01.ChooseBuff(player01Pick);
        player02.ChooseBuff(player02Pick);

        ReadyForInGame().Forget();

    }

    #endregion

    #region InGame
    [Header("InGame")]
    public IngamePanel inGamePanel;

    public ChessColor nowTurn { get; private set; } = ChessColor.White;
    private const int maxTurnCount = 3;
    private int nowTurnCount = 1;

    private async UniTask TurnChange()
    {
        nowGameStage = GameStage.TurnChange;
        nowTurn = nowTurn == ChessColor.White ? ChessColor.Black : ChessColor.White;

        await inGamePanel.TurnChange(nowTurn);

        TargetPlayer(nowTurn).Player_TurnStart();
        nowGameStage = GameStage.InGame;
    }

    public void EndTurn()
    {
        chessBoard.UpdatePlayerChose(new Vector2Int(-1, -1));

        inPutManager.PlayerInputStage(nowTurn, InputStage.None);

        if (!player01.haveKing || !player02.haveKing) EndInGame(nowTurn);
        else if (chessBoard.board.Count <= 2) EndInGame(ChessColor.None);
        else TurnChange().Forget();
    }

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


    public void EndInGame(ChessColor chessColor)
    {
        turnResult[nowTurnCount] = chessColor;
        SwitchStage(GameStage.TurnEnd).Forget();
    }

    #endregion

    #region TurnEnd 
    private ChessColor winner;

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


    public void Surrender(ChessColor surrender)
    {
        Debug.Log("Real GameSet");
        winner = surrender == ChessColor.White ? ChessColor.Black : ChessColor.White;
        SwitchStage(GameStage.GameEnd).Forget();
    }


    #endregion

    #region GameEnd
    private async UniTask GameEnd()
    {
        await UniTask.Yield();
        await PlayerCameraOpen(false);
        await MainCameraTurn(GameStage.Release);
        chessBoard.CleanTheBoard();
        SwitchStage(GameStage.Release).Forget();
    }


    #endregion

    #region Release
    [Header("Release")]
    public GameObject gameReleasePanel;

    public Image winnerTag;
    public Image whiteChessResult;
    public Image blackChessResult;


    private Dictionary<int, ChessColor> turnResult = new();
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


    private async UniTask Release()
    {
        await UniTask.Yield();
        ShowGameResult();
        gameReleasePanel.SetActive(true);

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
