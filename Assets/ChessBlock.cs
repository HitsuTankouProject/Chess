using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public enum ChessBlockStage {Normal, CanGo, CanEat };
public class ChessBlock : MonoBehaviour
{
    public enum ChessBoardColor
    {
        White,
        Black
    }
    public ChessBoardColor color;

    private Material normalMaterial => color == ChessBoardColor.Black ? ChessBoard.Instance.m_Black : ChessBoard.Instance.m_White;
    private Material m_GotCurse =>ChessBoard.Instance.m_GotCurse;
    private Material GetStageMaterial(ChessBlockStage chessBlockStage)
    {
        switch (chessBlockStage)
        {
            case ChessBlockStage.Normal:    return normalMaterial;
            case ChessBlockStage.CanGo:     return ChessBoard.Instance.m_CanGo;
            case ChessBlockStage.CanEat:    return ChessBoard.Instance.m_CanEat;
            default:
                Debug.LogError("Wrong ChessBlockStage");
                return null;
        }


    }

    public bool isKingChessSpawn;
    public bool isGotCurse { get; private set; } = false;
    private ChessBasic curseChess;
    public Vector2Int position;
    private Vector3 activePos => new Vector3(0, -2, 0);
    private Vector3 normalPos => new Vector3(0, -5, 0);

    private MeshRenderer meshRenderer => transform.GetChild(0).GetComponent<MeshRenderer>();

    public void Active(ChessBlockStage chessBlockStage)
    {
        if (curseChess != null) return;
        meshRenderer.material = GetStageMaterial(chessBlockStage);
        Vector3 targetPos = chessBlockStage == ChessBlockStage.Normal ? normalPos : activePos;
        transform.GetChild(0).localPosition = targetPos;
    }

    private IEnumerator GotCurse()
    {
        meshRenderer.material = ChessBoard.Instance.m_GotCurse;

        while (curseChess != null && curseChess.gameObject.activeSelf)
        {
            yield return null;
        }
        curseChess = null;

        Active(ChessBlockStage.Normal);

    }
    public void CurseTheBlock(ChessBasic chess)
    {
        if (curseChess != null) return;
        curseChess = chess;
        StartCoroutine(GotCurse());

    }

}
