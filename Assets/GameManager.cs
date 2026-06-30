using System.Collections.Generic;
using System;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Scene { Loading, GameTitle, InGame, Release, Error}

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

[System.Serializable]
public struct CameraView
{
    public Vector3 position { get; private set; }
    public Vector3 angle { get; private set; }

    public CameraView(Vector3 targetPos, Vector3 targetAngle)
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
    public Player player01;
    public Player player02;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool isTest = false;
    
    private void Start()
    {
        resourcesData.ResourcesInit();
        inPutManager.Init();

        if (isTest) return;
        SceneManager.LoadScene("InGame", LoadSceneMode.Single);
    }
}
