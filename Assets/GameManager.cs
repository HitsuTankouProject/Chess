using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameStage
{ 
    Loading, 
    GameTitle,
    GameDescription,
    ControllerChoose,
    GameStart,
    TurnStart,
    SkillChoose,
    InGame,
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
    private MainCameraView controllerChooseCameraView = new(new Vector3(-242, 70, -80), new Vector3(90, -90, 0), 50f);
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
    private IEnumerator SwitchStage(GameStage targetStage)
    {
        nowGameStage = targetStage;
        loading.SetActive(true);
        switch (nowGameStage)
        {
            case GameStage.Loading:             StartCoroutine(Loading());              break;
            case GameStage.GameTitle:           StartCoroutine(GameTitle());            break;
            case GameStage.GameDescription:     StartCoroutine(GameDescription());      break;
            case GameStage.ControllerChoose:    StartCoroutine(ControllerChoose());     break;
            case GameStage.GameStart:           StartCoroutine(GameStart());            break;
            case GameStage.TurnStart:           StartCoroutine(TurnStart());            break;
            case GameStage.SkillChoose:         StartCoroutine(SkillChoose());          break;
            case GameStage.InGame:              StartCoroutine(InGame());               break;
            case GameStage.TurnEnd:             StartCoroutine(TurnEnd());              break;
            case GameStage.GameEnd:             StartCoroutine(GameEnd());              break;
            case GameStage.Release:             StartCoroutine(Release());              break;

            default:
                Debug.LogError("GameManager.SwitchStage: Invalid game stage.");
                yield break;
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
            case GameStage.ControllerChoose:
                return controllerChooseCameraView;
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

    private IEnumerator MainCameraTurn(GameStage gameStage)
    {
        MainCameraView targetView = TargetMainCameraView(gameStage);

        if (Vector3.Distance(mainCamera.transform.position, targetView.position) < 0.01f)
        {
            SetMainCameraView(targetView);
            yield break;
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
                    Quaternion.Euler(nowView.angle),Quaternion.Euler(targetView.angle),t);

            mainCamera.fieldOfView = Mathf.Lerp(nowView.fieldOfView, targetView.fieldOfView, t);

            yield return null;
        }
        SetMainCameraView(targetView);
    }

    #endregion

    #region PlayerCameraView 
    
    private const float playerCameraOpenTime = 0.5f;

    private const float openRectX = 0.5f;
    private const float closeRectX = 0f;

    private IEnumerator TargetPlayerCameraOpen(Camera playerCamera, bool isOpen)
    {
        //playerCamera.gameObject.SetActive(false);
        //playerCamera.gameObject.SetActive(true);

        bool isWhite = playerCamera == player01Camera;

        Rect start = playerCamera.rect;
        Rect target = start;

        if (isWhite)
        {
            // WhiteF‘ü‰ü Width
            target.width = isOpen ? openRectX : closeRectX;
        }
        else
        {
            // BlackF‘ü‰ü X
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

            yield return null;
        }

        playerCamera.rect = target;
    }

    private IEnumerator PlayerCameraOpen(bool isOpen)
    {
        yield return StartCoroutine(TargetPlayerCameraOpen(player01Camera, isOpen));
        yield return StartCoroutine(TargetPlayerCameraOpen(player02Camera, isOpen));
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

    private IEnumerator PlayerCameraTurn(Camera playerCamera, Pair<ChessColor, PlayerCameraStage> playerCameraStagePair)
    {
        if (playerCamera != player01Camera && playerCamera != player02Camera)
        {
            Debug.LogError("PlayerCameraTurn: Invalid player camera.");
            yield break;
        }

        PlayerCameraView targetView = TargetPlayerCameraView(playerCameraStagePair.first, playerCameraStagePair.second);
        if (Vector3.Distance(playerCamera.transform.position, targetView.position) < 0.01f)
        {
            playerCamera.transform.position = targetView.position;
            playerCamera.transform.rotation = Quaternion.Euler(targetView.angle);
            yield break;
        }
        PlayerCameraView nowView =
            new PlayerCameraView( playerCamera.transform.position, playerCamera.transform.rotation.eulerAngles);

        float timer = 0;
        while (timer < cameraTurnTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / cameraTurnTime);

            playerCamera.transform.position =
                Vector3.Lerp(nowView.position, targetView.position, t);
            playerCamera.transform.rotation =
                Quaternion.Slerp( Quaternion.Euler(nowView.angle), Quaternion.Euler(targetView.angle), t);
            yield return null;
        }
        playerCamera.transform.position = targetView.position;
        playerCamera.transform.rotation = Quaternion.Euler(targetView.angle);
    }

    public void PlayerCameraTurn(Pair<ChessColor, PlayerCameraStage> playerCameraStagePair)
    {
        Camera camera = TargetCamera(playerCameraStagePair.first);
        StartCoroutine(PlayerCameraTurn(camera, playerCameraStagePair));
    }


    #endregion

    #region Button


    private void AllPanelClose()
    {
        gameTitlePanel.SetActive(false);
        gameDescriptionPanel.SetActive(false);
        gameReleasePanel.SetActive(false);
    }
    public void Button_BackToGameTitle()
    {
        AllPanelClose();
        StartCoroutine(SwitchStage(GameStage.GameTitle));
    }
    public void Button_BackToGameDescription()
    {
        AllPanelClose();
        StartCoroutine(SwitchStage(GameStage.GameDescription));
    }
    public void Button_BackToGameStart()
    {
        AllPanelClose();
        StartCoroutine(SwitchStage(GameStage.GameStart));
    }
    public void Button_BackToRelease()
    {
        AllPanelClose();
        StartCoroutine(SwitchStage(GameStage.Release));
    }

    public void Button_Exit() => Application.Quit();

    #endregion



    #region Loading
    [Header("Loading")]
    public GameObject loading;

    private IEnumerator Loading()
    {

        yield return null;
        resourcesData.ResourcesInit();
        inPutManager.Init();

        chessBoard.ChessBoard_Init();
        TargetPlayer(ChessColor.White).Player_Init(ChessColor.White);
        TargetPlayer(ChessColor.Black).Player_Init(ChessColor.Black);


        StartCoroutine(SwitchStage(GameStage.GameTitle));
    }

    #endregion

    #region GameTitle
    [Header("GameTitle")]
    public GameObject gameTitlePanel;

    private IEnumerator GameTitle()
    {
        yield return null;
        yield return StartCoroutine(MainCameraTurn(GameStage.GameTitle));
        gameTitlePanel.SetActive(true);
    }

    #endregion

    #region GameDescription
    [Header("GameDescription")]
    public GameObject gameDescriptionPanel;

    private IEnumerator GameDescription()
    {
        yield return StartCoroutine(MainCameraTurn(GameStage.GameDescription));

        gameDescriptionPanel.SetActive(true);
    }

    public void Button_GameDescription_NextPage()
    {

    }

    public void Button_GameDescription_ReturnPage()
    {

    }


    #endregion

    #region ControllerChoose

    [Header("ControllerChoose")]
    public ControllerChoosePanel controllerChoosePanel;

    private IEnumerator ControllerChoose()
    {
        yield return StartCoroutine(MainCameraTurn(GameStage.ControllerChoose));

        controllerChoosePanel.gameObject.SetActive(true);
        controllerChoosePanel.Init();
    }

    public void EndControllerChoose(GamepadType player01pick, GamepadType player02pick)
    {
        //TargetPlayer(ChessColor.White).playerInPut.SetUseGamepadType(player01pick);
        //TargetPlayer(ChessColor.Black).playerInPut.SetUseGamepadType(player01pick);
        //controllerChoosePanel.gameObject.SetActive(false);
        //StartCoroutine(SwitchStage(GameStage.GameStart));
    }


    #endregion

    #region GameStart

    private IEnumerator GameStart()
    {
        yield return StartCoroutine(MainCameraTurn(GameStage.TurnStart));

        yield return PlayerCameraOpen(true);
        nowTurnCount = 1;
        turnResult.Clear();
        StartCoroutine(SwitchStage(GameStage.TurnStart));
    }

    #endregion

    #region TurnStart
    private IEnumerator TurnInit()
    {
        yield return chessBoard.ChessBoard_TurnInit();
        Dictionary<Vector2Int, ChessBasic> whiteChess = new Dictionary<Vector2Int, ChessBasic>();
        Dictionary<Vector2Int, ChessBasic> blackChess = new Dictionary<Vector2Int, ChessBasic>();

        foreach (Vector2Int chessPos in chessBoard.board.Keys)
        {
            if (chessBoard.board[chessPos].color == ChessColor.White)
                whiteChess[chessPos] = chessBoard.board[chessPos];
            else blackChess[chessPos] = chessBoard.board[chessPos];
        }

        player01.Player_ChessInit(whiteChess);
        player02.Player_ChessInit(blackChess);
    }
    private IEnumerator TurnStart()
    {

        yield return TurnInit();
        StartCoroutine(SwitchStage(GameStage.SkillChoose));
    }

    #endregion

    #region SkillChoose
    [Header("SkillChoose")]
    public ChooseSkillPanel chooseSkillPanel;
    public bool isPicking => chooseSkillPanel.isPicking;
    private IEnumerator SkillChoose()
    {
        yield return PlayerCameraOpen(false);

        yield return StartCoroutine(MainCameraTurn(GameStage.SkillChoose));

        chooseSkillPanel.gameObject.SetActive(true);
        chooseSkillPanel.Init();

    }

    private IEnumerator ReadyForInGame()
    {
        yield return StartCoroutine(MainCameraTurn(GameStage.TurnStart));
        yield return PlayerCameraOpen(true);

        StartCoroutine(SwitchStage(GameStage.InGame));
    }



    public void EndSkillChoose(AllBuffCard player01Pick, AllBuffCard player02Pick)
    {
        player01.ChooseBuff(player01Pick);
        player02.ChooseBuff(player02Pick);

        Debug.Log(player01Pick.ToString());
        Debug.Log(player02Pick.ToString());

        StartCoroutine(ReadyForInGame());

    }

    #endregion

    public ChessColor nowTurn { get; private set; } = ChessColor.White;
    private const int maxTurnCount = 3;
    private int nowTurnCount = 1;

    #region InGame

    private void TurnChange()
    {
        nowTurn = nowTurn == ChessColor.White ? ChessColor.Black : ChessColor.White;
        TargetPlayer(nowTurn).Player_TurnStart();
    }

    private void StopPlayerTurn(ChessColor chessColor)
    {
        TargetPlayer(chessColor).nowPlayerStage = PlayerStage.NoMyTurn;
        if (TargetPlayer(chessColor).turnStart != null)
        {
            StopCoroutine(TargetPlayer(chessColor).turnStart);
            TargetPlayer(chessColor).turnStart = null;
        }
    }

    public void EndTurn()
    {
        chessBoard.UpdatePlayerChose(new Vector2Int(-1, -1));
        StopPlayerTurn(ChessColor.White);
        StopPlayerTurn(ChessColor.Black);
        TurnChange();
    }



    private IEnumerator InGame()
    {
        yield return null;
        nowTurn = ChessColor.White;
        TargetPlayer(nowTurn).Player_TurnStart();
    }

    public void EndInGame(ChessColor chessColor)
    {
        turnResult[nowTurnCount] = chessColor;

        StartCoroutine(SwitchStage(GameStage.TurnEnd));
    }

    #endregion

    #region TurnEnd 

    private IEnumerator TurnEnd()
    {
        yield return null;

        if (nowTurnCount >= maxTurnCount) 
        {
            Debug.Log("Real GameSet");
            StartCoroutine(SwitchStage(GameStage.GameEnd));
            yield break;

        }

        Debug.Log("GameSet!!");
        nowTurnCount++;
        player01.AllBuffLevelUp();
        player02.AllBuffLevelUp();

        StartCoroutine(SwitchStage(GameStage.TurnStart));

    }

    #endregion

    #region GameEnd
    private IEnumerator GameEnd()
    {
        yield return null;
        yield return PlayerCameraOpen(false);
        yield return StartCoroutine(MainCameraTurn(GameStage.Release));
        chessBoard.CleanTheBoard();
        StartCoroutine(SwitchStage(GameStage.Release));
    }




    #endregion

    #region Release

    public GameObject gameReleasePanel;
    private Dictionary<int, ChessColor> turnResult = new();


    private IEnumerator Release()
    {
        yield return null;
        gameReleasePanel.SetActive(true);

    }

    private void s()
    {

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
        StartCoroutine(SwitchStage(GameStage.Loading));


    }


}
