using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public enum ChessBlockStage {Normal, CanGo, CanEat };
public enum BlockStage { None, KingSpawn, GotCurse };

public class ChessBlock : MonoBehaviour
{
    public ChessColor color;
    public BlockStage blockStage { get; private set; } = BlockStage.None;

    public bool isKingChessSpawn;
    private ChessBasic curseChess;
    public Vector2Int position;

    public GameObject choseMark;
    public MeshRenderer chessEffect;
    private MeshFilter chessEffect_filter;

    public MeshRenderer blockEffect;
    public ParticleSystemRenderer blockParticle;


    private ResourcesData _resourcesData => GameManager.Instance.resourcesData;
    private Material m_GetChessEffect(ChessBlockStage chessBlockStage)
    {
        switch (chessBlockStage)
        {
            case ChessBlockStage.CanGo:return _resourcesData.allMaterial.m_BoardBlockCanGo;
            case ChessBlockStage.CanEat: return _resourcesData.allMaterial.m_BoardBlockCanEat;
            default: return null;
        }
    }
    private Mesh model_GetChessModel(ChessType chessType)
        => _resourcesData.chessModelDict[chessType].model;
    private Dictionary<ChessType, Vector3> choseEffectPosition = new()
    {
        {ChessType.King, new Vector3(-3.55f,0,-3.55f)  },
        {ChessType.Queen, new Vector3(-3.555f,0,-3.55f) },
        {ChessType.Bishop, new Vector3(-3.33f,0,-3.47f) },
        {ChessType.Rook,new Vector3(-3.31f,0,-3.31f) },
        {ChessType.Knight,new Vector3(-3.31f,0,-3.31f) },
        {ChessType.Pawn,new Vector3(-2.95f,0,-2.95f) }
    };



    public void ShowChoseEffect(bool isShow)
    {
        if (choseMark == null) return;
        choseMark.SetActive(isShow);
    }
    public void ChangeChessBlockEffect(ChessBlockStage chessBlockStage, ChessType chessType = default)
    {
        choseMark.SetActive(false);
        if (chessBlockStage == ChessBlockStage.Normal)
        {
            chessEffect.enabled = false;
            return;
        }

        chessEffect.enabled = true;
        chessEffect.gameObject.transform.localPosition = choseEffectPosition[chessType];
        chessEffect_filter.mesh = model_GetChessModel(chessType);

        chessEffect.material = m_GetChessEffect(chessBlockStage);
    }
    private void ChangeBlockEffect(BlockStage stage)
    {
        if (stage == BlockStage.None)
        {
            blockEffect.enabled = false;
            blockParticle.gameObject.SetActive(false);
        }
        else if (stage == BlockStage.KingSpawn)
        {
            blockEffect.enabled = true;
            blockParticle.gameObject.SetActive(true);

            blockEffect.material = _resourcesData.allMaterial.m_BoardBlockKingSpawn;
            blockParticle.material = _resourcesData.allMaterial.e_BoardBlockKingSpawn;

        }
        else if (stage == BlockStage.GotCurse)
        {

            blockEffect.enabled = true;
            blockParticle.gameObject.SetActive(true);

            blockEffect.material = _resourcesData.allMaterial.m_GotCurse;
            blockParticle.material = _resourcesData.allMaterial.e_BoardBlockGotCurse;
        }
    }

    private IEnumerator GotCurse()
    {
        while (curseChess != null && curseChess.gameObject.activeSelf)
        {
            yield return null;
        }
        curseChess = null;

        ChangeBlockStage(BlockStage.None);

    }

    public void ChangeBlockStage(BlockStage stage, ChessBasic chess = default)
    {
        if (blockStage == BlockStage.KingSpawn) return;
        blockStage = stage;
        ChangeBlockEffect(stage);

        if (stage == BlockStage.GotCurse)
        {
            if (chess == default)
            {
                Debug.LogError("Block Cant Got Curse With No Chess");
                return;
            }
            curseChess = chess;
            StartCoroutine(GotCurse());
        }

    }

    public void Init(Vector2Int targetPos)
    {
        position = targetPos;

        chessEffect.enabled = false;
        choseMark.SetActive(false);
        chessEffect_filter = chessEffect.gameObject.GetComponent<MeshFilter>();
        ChangeBlockStage(BlockStage.None);
        ChangeChessBlockEffect(ChessBlockStage.Normal);

    }

    private void Start()
    {
        Init(Vector2Int.zero);
    }

    public bool normal = false;
    public bool gotCurse = false;
    public bool kingChessSpawn = false;
    public bool canGo = false;
    public bool caneat = false;

    private void Update()
    {
        if (normal)
        {
            normal = false;
            ShowChoseEffect(false);
            ChangeBlockStage(BlockStage.None);
            ChangeChessBlockEffect(ChessBlockStage.Normal);
        }
        if (gotCurse)
        {
            gotCurse = false;
            ChangeBlockStage(BlockStage.GotCurse);
        }
        if (kingChessSpawn)
        {
            kingChessSpawn = false;
            ChangeBlockStage(BlockStage.KingSpawn);
        }
        if (canGo)
        {
            canGo = false;
            ChangeChessBlockEffect(ChessBlockStage.CanGo, ChessType.Knight);
            ShowChoseEffect(true);
        }
        if (caneat)
        {
            caneat = false;
            ChangeChessBlockEffect(ChessBlockStage.CanEat, ChessType.Pawn);
            ShowChoseEffect(true);

        }




    }

}   
