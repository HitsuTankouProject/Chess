using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBlock : MonoBehaviour
{
    public enum ChessBoardColor
    {
        White,
        Black
    }
    public ChessBoardColor color;

    public bool isKingChessSpawn;
    public Vector2Int position;
    private Vector3 canGoPos => new Vector3(0, -2, 0);
    private Vector3 normalPos => new Vector3(0, -5, 0);

    public Material m_Black;
    public Material m_White;
    public Material m_CanGo;
    private MeshRenderer meshRenderer => transform.GetChild(0).GetComponent<MeshRenderer>();

    public bool isCanGo;
    private IEnumerator CanGo()
    {
        meshRenderer.material = m_CanGo;
        transform.GetChild(0).localPosition = canGoPos;

        while (isCanGo)
        {
            yield return null;
        }
        meshRenderer.material = color == ChessBoardColor.Black ? m_Black : m_White;

        transform.GetChild(0).localPosition = normalPos;
    }

    public void ShowCanGo()
    {
        isCanGo = true;
        StartCoroutine(CanGo());
    }
    public void ResetNormal()
    {
        isCanGo = false;
    }


    public bool test;
    private void Update()
    {
        if(test)
        {
            test = false;
            StartCoroutine(CanGo());
        }
    }



}
