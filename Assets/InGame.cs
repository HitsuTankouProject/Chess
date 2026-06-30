using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static ChessBlock;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;



public enum CameraStage { Normal, Pick }

public struct PlayerViewData
{
    private CameraView normalView;
    private CameraView pickView;

    public PlayerViewData(CameraView view01, CameraView view02)
    {
        normalView = view01;
        pickView = view02;
    }

    public CameraView TargetView(CameraStage cameraStage)
    {
        if(cameraStage== CameraStage.Normal) return normalView;
        else return pickView;
    }

}



public enum InGameStage
{
    Init,

    ChooseSkill,

    TurnStart,
    TurnChanging,

    GameSet

}


public class InGame : MonoBehaviour
{
    private PoolManager _poolManager => PoolManager.Instance;
    private Camera _camera => Camera.main;
    private ChessBoard _chessBoard => ChessBoard.Instance;

    public Player whiteChessPlayer;
    public Player blackChessPlayer;

    public static InGame Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public InGameStage inGameStage { get; private set; } = InGameStage.Init;
    public ChessColor nowTurn { get; private set; } = ChessColor.White;

    #region InGameInit
    private void InGameInit()
    {
        inGameStage = InGameStage.Init;

        _chessBoard.ChessBoard_Init();
        whiteChessPlayer.Player_Init(ChessColor.White);
        blackChessPlayer.Player_Init(ChessColor.Black);
        PlayerViewDataInit();
        whiteChessPlayer.playerInPut.StartInput();
        blackChessPlayer.playerInPut.StartInput();


        StartChooseSkill();

    }


    #endregion

    #region ChooseSkills
    [Header("ChooseSkills")]
    public Camera mainPanelCamera;
    public ChooseSkillPanel chooseSkillPanel;
    private void StartChooseSkill()
    {
        inGameStage = InGameStage.ChooseSkill;
        mainPanelCamera.gameObject.SetActive(true);
        chooseSkillPanel.gameObject.SetActive(true);
        chooseSkillPanel.Init();
    }

    public void EndOfChooseSkill()
    {
        mainPanelCamera.gameObject.SetActive(false);
        chooseSkillPanel.gameObject.SetActive(false);

        GameStart();
    }

    #endregion

    #region Turn
    public void EndTurn()
    {
        _chessBoard.UpdatePlayerChose(new Vector2Int(-1, -1));
        inGameStage = InGameStage.TurnChanging;
        whiteChessPlayer.nowPlayerStage = PlayerStage.NoMyTurn;
        if (whiteChessPlayer.turnStart != null)
        {
            StopCoroutine(whiteChessPlayer.turnStart);
            whiteChessPlayer.turnStart = null;
        }
        blackChessPlayer.nowPlayerStage = PlayerStage.NoMyTurn;
        if (blackChessPlayer.turnStart != null)
        {
            StopCoroutine(blackChessPlayer.turnStart);
            blackChessPlayer.turnStart = null;
        }

        TurnChange();
    }

    private void TurnChange()
    {
        nowTurn = nowTurn == ChessColor.White ? ChessColor.Black : ChessColor.White;

        if (nowTurn == ChessColor.White) whiteChessPlayer.Player_TurnStart();
        else blackChessPlayer.Player_TurnStart();

        inGameStage = InGameStage.TurnStart;

    }

    private void TurnInit()
    {
        _chessBoard.ChessBoard_TurnInit();
        Dictionary<Vector2Int, ChessBasic> whiteChess = new Dictionary<Vector2Int, ChessBasic>();
        Dictionary<Vector2Int, ChessBasic> blackChess = new Dictionary<Vector2Int, ChessBasic>();

        foreach (Vector2Int chessPos in _chessBoard.board.Keys)
        {
            if (_chessBoard.board[chessPos].color == ChessColor.White)
                whiteChess[chessPos] = _chessBoard.board[chessPos];
            else blackChess[chessPos] = _chessBoard.board[chessPos];
        }

        whiteChessPlayer.Player_ChessInit(whiteChess);
        blackChessPlayer.Player_ChessInit(blackChess);
    }

    #endregion

    #region Camera Turn

    private readonly CameraView whiteView = new CameraView(new Vector3(0, 70, -44), new Vector3(55, 0, 0));
    private readonly CameraView pickView_White = new CameraView(new Vector3(0, 90, 0), new Vector3(90, 0, 0));

    private readonly CameraView blackView = new CameraView(new Vector3(0, 70, 44), new Vector3(55, 180, 0));
    private readonly CameraView pickView_Black = new CameraView(new Vector3(0, 90, 0), new Vector3(90, 180, 0));

    public Dictionary<ChessColor, PlayerViewData> playerCameraView {  get; private set; } = new();
    private readonly Vector3 center = new Vector3(0, 0, 0);
    private const float cameraTurnTime = 0.75f;

    private void PlayerViewDataInit()
    {
        playerCameraView.Clear();
        playerCameraView[ChessColor.White] = new PlayerViewData(whiteView, pickView_White);
        playerCameraView[ChessColor.Black] = new PlayerViewData(blackView, pickView_Black);

    }
    public IEnumerator TurnCamera(Camera camera, ChessColor color , CameraStage cameraStage)
    {
        CameraView targetView = playerCameraView[color].TargetView(cameraStage);

        if (Vector3.Distance(camera.transform.position, targetView.position) < 0.01f)
        {
            camera.transform.position = targetView.position;
            camera.transform.rotation = Quaternion.Euler(targetView.angle);
            yield break;
        }

        CameraStage nowStage = cameraStage == CameraStage.Normal
            ? CameraStage.Pick : CameraStage.Normal;

        CameraView nowView = playerCameraView[color].TargetView(nowStage);

        float timer = 0f;

        Vector3 startOffset = nowView.position - center;
        Vector3 endOffset = targetView.position - center;

        float startRadius = startOffset.magnitude;
        float endRadius = endOffset.magnitude;

        float startAngle = nowView.angle.x;
        float endAngle = targetView.angle.x;

        float side = color == ChessColor.White ? -1f : 1f;

        while (timer < cameraTurnTime)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / cameraTurnTime);

            float angle = Mathf.Lerp(startAngle, endAngle, t);
            float radius = Mathf.Lerp(startRadius, endRadius, t);

            float rad = angle * Mathf.Deg2Rad;

            float y = center.y + Mathf.Sin(rad) * radius;
            float z = center.z + side * Mathf.Cos(rad) * radius;

            camera.transform.position = new Vector3(center.x, y, z);

            float rotateY = Mathf.Lerp(nowView.angle.y, targetView.angle.y, t);
            camera.transform.rotation = Quaternion.Euler(angle, rotateY, 0);

            yield return null;
        }

        camera.transform.position = targetView.position;
        camera.transform.rotation = Quaternion.Euler(targetView.angle);
    }


    #endregion

    #region GameSet

    public int nowRound = 1;
    private const int maxRound = 3;

    public void GameSet()
    {
        inGameStage = InGameStage.GameSet;
        Debug.Log("GameSet!!");
        nowRound++;
        if(nowRound> maxRound)
        {
            Debug.Log("Real GameSet");
            return;
        }
        whiteChessPlayer.AllBuffLevelUp();
        blackChessPlayer.AllBuffLevelUp();

        StartChooseSkill();
    }



    #endregion
    public void GameStart()
    {
        TurnInit();
        nowTurn = ChessColor.White;
        whiteChessPlayer.Player_TurnStart();
        inGameStage = InGameStage.TurnStart;
    }

    private void Start()
    {
        InGameInit();


    }

    public bool test = false;

    private void Update()
    {
        if (test)
        {
            test = false;
            GameSet();
        }


    }



}
