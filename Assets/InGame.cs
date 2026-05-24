using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChessBlock;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;


[System.Serializable]
public class Pair<F, S>
{
    public Pair()
    { }
    public Pair(F f, S s)
    {
        this.first = f;
        this.second = s;
    }
    public F first { get; set; }
    public S second { get; set; }

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

    public static bool operator == (Pair<F, S> a, Pair<F, S> b)
    {
        return a.first.Equals(b.first) && a.second.Equals(b.second);
    }
    public static bool operator !=(Pair<F, S> a, Pair<F, S> b)
    {
        return !a.first.Equals(b.first) || !a.second.Equals(b.second);
    }

}

public class CameraView
{
    public Vector3 position {  get; private set; }
    public Vector3 angle { get; private set; }

    public CameraView(Vector3 targetPos, Vector3 targetAngle)
    {
        position = targetPos;
        angle = targetAngle;
    }

}

public enum InGameStage
{
    Init,

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

    #region Camera Turn
    private readonly CameraView whiteView = new CameraView(new Vector3(0, 70, -54), new Vector3(55, 0, 0));
    private readonly CameraView blackView = new CameraView(new Vector3(0, 70, 54), new Vector3(55, 180, 0));
    private float radius => Math.Abs(whiteView.position.z - blackView.position.z) / 2;
    private readonly Vector3 center = new Vector3(0, 70, 0);

    private readonly Dictionary<ChessColor, CameraView> turnView = new Dictionary<ChessColor, CameraView>();
    private CameraView nowCameraView => turnView[nowTurn];

    private const float cameraTurnTime = 2.0f;

    private IEnumerator CameraTurn()
    {
        if (turnView.Count == 0)
        {
            turnView[ChessColor.White] = whiteView;
            turnView[ChessColor.Black] = blackView;
        }

        float timer = 0;
        CameraView basicView = nowTurn == ChessColor.White ? blackView : whiteView;


        while (timer < cameraTurnTime)
        {
            timer += Time.deltaTime;

            float turnAngle = 180 * (timer / cameraTurnTime);
            turnAngle += basicView.angle.y;
            _camera.transform.rotation = Quaternion.Euler(55, turnAngle, 0);

            float radian = turnAngle * Mathf.Deg2Rad;
            float targetX = center.x + 54 * Mathf.Sin(radian);
            float targetZ = center.z - 54 * Mathf.Cos(radian);

            _camera.transform.position = new Vector3(targetX, basicView.position.y, targetZ);

            yield return null;
        }

        _camera.transform.position = nowCameraView.position;
        _camera.transform.rotation = Quaternion.Euler(nowCameraView.angle);
    }
    #endregion


    private Coroutine turnChange;

    public void StartTurnChange()
    {
        if (turnChange != null) return;
        turnChange = StartCoroutine(TurnChange());
    }

    private IEnumerator TurnChange()
    {
        if (inGameStage == InGameStage.TurnChanging)
        {
            yield break;
        } 
        inGameStage = InGameStage.TurnChanging;
        whiteChessPlayer.nowPlayerStage = PlayerStage.NoMyTurn;
        blackChessPlayer.nowPlayerStage = PlayerStage.NoMyTurn;

        if (whiteChessPlayer.turnStart != null)
        {
            StopCoroutine(whiteChessPlayer.turnStart);
            whiteChessPlayer.turnStart = null;
        }

        if (blackChessPlayer.turnStart != null)
        {
            StopCoroutine(blackChessPlayer.turnStart);
            blackChessPlayer.turnStart = null;
        }

        nowTurn = nowTurn == ChessColor.White ? ChessColor.Black : ChessColor.White;

        yield return StartCoroutine(CameraTurn());

        switch (nowTurn)
        {
            case ChessColor.White:
                whiteChessPlayer.Player_TurnStart();
                break;

            case ChessColor.Black:
                blackChessPlayer.Player_TurnStart();
                break;
        }

        inGameStage = InGameStage.TurnStart;

        turnChange = null;


    }
    private void InGameInit()
    {
        inGameStage = InGameStage.Init;

        _chessBoard.ChessBoard_Init();
        whiteChessPlayer.Player_Init(ChessColor.White);
        blackChessPlayer.Player_Init(ChessColor.Black);

        inGameStage = InGameStage.TurnStart;

        StartCoroutine(TurnInit());
    }

    private IEnumerator TurnInit()
    {
        yield return StartCoroutine(_chessBoard.ChessBoard_TurnInit());

        HashSet<ChessBasic> whiteChess = new HashSet<ChessBasic>();
        HashSet<ChessBasic> blackChess = new HashSet<ChessBasic>();

        foreach (ChessBasic chess in _chessBoard.board.Values)
        {
            if(chess.color == ChessColor.White)
            {
                whiteChess.Add(chess);
            }
            else
            {
                blackChess.Add(chess);
            }

        }
        yield return null;

        whiteChessPlayer.Player_ChessInit(whiteChess);
        blackChessPlayer.Player_ChessInit(blackChess);

        yield return null;
        whiteChessPlayer.Player_TurnStart();

    }

    #region Turn



    #endregion



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

            StartCoroutine(CameraTurn());
        }


    }



}
