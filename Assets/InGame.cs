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

    


    public ChessColor nowTurn {  get; private set; } = ChessColor.White;
    private readonly CameraView whiteView = new CameraView(new Vector3(0, 70, -54), new Vector3(55, 0, 0));
    private readonly CameraView blackView = new CameraView(new Vector3(0, 70, 54), new Vector3(55, 180, 0));
    private readonly Dictionary<ChessColor, CameraView> turnView = new Dictionary<ChessColor, CameraView>();
    private CameraView nowCameraView => turnView[nowTurn];

    private IEnumerator CameraChange()
    {
        if (turnView.Count == 0)
        {
            turnView[ChessColor.White] = whiteView;
            turnView[ChessColor.Black] = blackView;
        }

        _camera.transform.position = nowCameraView.position;
        _camera.transform.rotation = Quaternion.Euler(nowCameraView.angle);


        yield return null;

    }

    public void TurnChange()
    {
        inGameStage = InGameStage.TurnChanging;
        nowTurn = nowTurn == ChessColor.White ? ChessColor.Black : ChessColor.White;
        CameraChange();
    }



    private void Start()
    {
    }


}
