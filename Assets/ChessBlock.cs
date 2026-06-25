using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public enum ChessBlockStage {Normal, CanGo, CanEat };
public enum BlockStage { None, KingSpawn, GotChose };

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
    public BlockStage blockStage { get; private set; } = BlockStage.None;
    public bool isGotCurse { get; private set; } = false;
    private ChessBasic curseChess;
    public Vector2Int position;

    private GameObject choseEffect;

    private readonly Pair<Vector3, Vector3> activeSide = new Pair<Vector3, Vector3>()
    {
        first = new Vector3(0, -2, 0),
        second = new Vector3(0, 3.5f, 0)
    };
    private readonly Pair<Vector3, Vector3> normalSide = new Pair<Vector3, Vector3>()
    {
        first = new Vector3(0, -5, 0),
        second = new Vector3(0, 0.03f, 0)
    };

    private MeshRenderer meshRenderer => transform.GetChild(0).GetComponent<MeshRenderer>();

    public void Active(ChessBlockStage chessBlockStage)
    {
        if (curseChess != null) return;
        meshRenderer.material = GetStageMaterial(chessBlockStage);
        Pair<Vector3, Vector3> targetSide = chessBlockStage == ChessBlockStage.Normal ? normalSide : activeSide;
        transform.GetChild(0).localPosition = targetSide.first;
        choseEffect.transform.localPosition = targetSide.second;
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

    public MeshRenderer chessEffect;
    public GameObject pickMark;
    public MeshRenderer blockEffect;

    private ResourcesData _resourcesData => GameManager.Instance.resourcesData;
    private Material m_GetChessEffect(ChessBlockStage chessBlockStage)
    {
        switch (chessBlockStage)
        {
            case ChessBlockStage.CanGo:return _resourcesData.m_BoardBlockCanGo;
            case ChessBlockStage.CanEat: return _resourcesData.m_BoardBlockCanEat;
            default: return null;
        }
    }


    public void ShowChoseEffect(bool isShow)
    {
        if (pickMark == null) return;
        pickMark.SetActive(isShow);
    }

    public void ChessEffectActive(ChessBlockStage chessBlockStage)
    {
        if(chessBlockStage== ChessBlockStage.Normal)
        {
            choseEffect.SetActive(false);
            return;
        }
        if (!choseEffect.activeSelf) choseEffect.SetActive(true);
        chessEffect.material = m_GetChessEffect(chessBlockStage);
    }

    public void CurseTheBlock(ChessBasic chess)
    {
        if (chess == null || blockStage == BlockStage.KingSpawn) return;
        curseChess = chess;

        blockStage = BlockStage.GotChose;
        blockEffect.enabled = true;
        blockEffect.material = _resourcesData.m_BoardBlockGotCurse;
        StartCoroutine(GotCurse());
    }
    public void TurnToKingSpawn()
    {
        blockStage = BlockStage.KingSpawn;
        blockEffect.enabled = true;
        blockEffect.material = _resourcesData.m_BoardBlockGotCurse;
    }

    public void Init(Vector2Int targetPos)
    {
        position = targetPos;
        choseEffect = transform.GetChild(1).gameObject;
        choseEffect.SetActive(false);
    }



}   
