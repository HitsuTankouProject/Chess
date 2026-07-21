using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>盤面マスに表示する移動候補エフェクトの状態を表します。</summary>
public enum ChessBlockStage 
{
    /// <summary>移動候補を表示していない通常状態です。</summary>
    Normal,
    /// <summary>選択中の駒が移動できるマスです。</summary>
    CanGo,
    /// <summary>選択中の駒が敵駒を捕獲できるマスです。</summary>
    CanEat
};
/// <summary>盤面マス自体に設定される特殊状態を表します。</summary>
public enum BlockStage 
{
    /// <summary>特殊効果が設定されていない通常状態です。</summary>
    None,
    /// <summary>キングの生成地点として使用される状態です。</summary>
    KingSpawn,
    /// <summary>マスへ呪いが付与されている状態です。</summary>
    GotCurse
};

/// <summary>
/// チェス盤の1マスに対する状態と表示エフェクトを管理します。
/// 駒の移動・捕獲候補、キング生成地点、呪い状態に応じて、
/// メッシュ、マテリアル、パーティクル、選択マークを切り替えます。
/// 呪いを付与した駒が盤面から消えた際は、呪いを解除してマス上の駒を浄化します。
/// </summary>
public class ChessBlock : MonoBehaviour
{
    /// <summary>このマスに設定されている駒色です。</summary>
    public ChessColor color;
    /// <summary>現在の特殊なマス状態を取得します。</summary>
    public BlockStage blockStage { get; private set; } = BlockStage.None;
    /// <summary>このマスがキングの生成地点かどうかを示します。</summary>
    public bool isKingChessSpawn;
    /// <summary>このマスへ呪いを付与した駒です。</summary>
    private ChessBasic curseChess;
    /// <summary>このマスの盤面座標です。</summary>
    public Vector2Int position;
    /// <summary>マスが選択されていることを示すマークです。</summary>
    public GameObject choseMark;
    /// <summary>移動・捕獲候補となる駒モデルを描画するレンダラーです。</summary>
    public MeshRenderer chessEffect;
    /// <summary>移動候補として表示する駒モデルを設定する MeshFilter です。</summary>
    private MeshFilter chessEffect_filter;
    /// <summary>特殊なマス状態を表示するレンダラーです。</summary>
    public MeshRenderer blockEffect;
    /// <summary>特殊なマス状態を表示するパーティクルレンダラーです。</summary>
    public ParticleSystemRenderer blockParticle;
    /// <summary>ゲーム内で共有するマテリアルと駒モデルを取得します。</summary>
    private ResourcesData _resourcesData => GameManager.Instance.resourcesData;
    /// <summary>移動候補の状態に対応する盤面マテリアルを取得します。</summary>
    /// <param name="chessBlockStage">表示する移動候補の状態です。</param>
    /// <returns>移動または捕獲用マテリアルです。通常状態の場合は <see langword="null" /> です。</returns>
    private Material m_GetChessEffect(ChessBlockStage chessBlockStage)
    {
        switch (chessBlockStage)
        {
            case ChessBlockStage.CanGo:return _resourcesData.allMaterial.m_BoardBlockCanGo;
            case ChessBlockStage.CanEat: return _resourcesData.allMaterial.m_BoardBlockCanEat;
            default: return null;
        }
    }
    /// <summary>指定した駒種に対応する表示モデルを取得します。</summary>
    /// <param name="chessType">表示する駒の種類です。</param>
    /// <returns>駒種に対応するメッシュを返します。</returns>
    private Mesh model_GetChessModel(ChessType chessType) => _resourcesData.chessModelDict[chessType].model;
    /// <summary>駒種ごとの候補表示モデルのローカル座標です。</summary>
    private Dictionary<ChessType, Vector3> choseEffectPosition = new()
    {
        {ChessType.King, new Vector3(-3.55f,0,-3.55f)  },
        {ChessType.Queen, new Vector3(-3.555f,0,-3.55f) },
        {ChessType.Bishop, new Vector3(-3.33f,0,-3.47f) },
        {ChessType.Rook,new Vector3(-3.31f,0,-3.31f) },
        {ChessType.Knight,new Vector3(-3.31f,0,-3.31f) },
        {ChessType.Pawn,new Vector3(-2.95f,0,-2.95f) }
    };
    /// <summary>マスの選択マークを表示または非表示にします。</summary>
    /// <param name="isShow">選択マークを表示する場合は <see langword="true" /> です。</param>
    public void ShowChoseEffect(bool isShow)
    {
        if (choseMark == null) return;
        choseMark.SetActive(isShow);
    }
    /// <summary>移動候補の状態と駒種に合わせて候補表示を更新します。</summary>
    /// <param name="chessBlockStage">表示する移動候補の状態です。</param>
    /// <param name="chessType">候補位置へ表示する駒の種類です。</param>
    public void ChangeChessBlockEffect(ChessBlockStage chessBlockStage, ChessType chessType = default)
    {
        choseMark.SetActive(false);
        if (chessBlockStage == ChessBlockStage.Normal)
        {
            chessEffect.enabled = false;
            return;
        }

        // 駒種に合わせた位置とモデルを設定し、候補状態のマテリアルを適用します。
        chessEffect.enabled = true;
        chessEffect.gameObject.transform.localPosition = choseEffectPosition[chessType];
        chessEffect_filter.mesh = model_GetChessModel(chessType);

        chessEffect.material = m_GetChessEffect(chessBlockStage);
    }
    /// <summary>特殊なマス状態に合わせてメッシュとパーティクル表示を更新します。</summary>
    /// <param name="stage">新しく表示するマス状態です。</param>
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
    /// <summary>
    /// 呪いを付与した駒が非アクティブになるまで待機し、マスの呪いを解除します。
    /// 解除時にマス上へ駒が存在する場合は、その駒の呪いも浄化します。
    /// </summary>
    private async UniTask GotCurse()
    {
        await UniTask.WaitWhile(() => curseChess != null && curseChess.gameObject.activeSelf);
        curseChess = null;

        ChangeBlockStage(BlockStage.None);
        if(GameManager.Instance.chessBoard.board.TryGetValue(position,out ChessBasic chess))
            chess.PurifyThisChess();
    }
    /// <summary>マスの特殊状態を変更し、呪い状態の場合は解除監視を開始します。</summary>
    /// <param name="stage">新しく設定するマス状態です。</param>
    /// <param name="chess">このマスへ呪いを付与した駒です。</param>
    public void ChangeBlockStage(BlockStage stage, ChessBasic chess = default)
    {
        blockStage = stage;
        ChangeBlockEffect(stage);

        if (blockStage != BlockStage.KingSpawn && stage == BlockStage.GotCurse)
        {
            // 呪いの解除時期を判断するため、付与元の駒が必要です。
            if (chess == default)
            {
                Debug.LogError("Block Cant Got Curse With No Chess");
                return;
            }
            curseChess = chess;
            GotCurse().Forget();
        }

    }
    /// <summary>盤面座標と表示コンポーネントを初期状態へ設定します。</summary>
    /// <param name="targetPos">このマスへ割り当てる盤面座標です。</param>
    public void Init(Vector2Int targetPos)
    {
        position = targetPos;

        chessEffect.enabled = false;
        choseMark.SetActive(false);
        chessEffect_filter = chessEffect.gameObject.GetComponent<MeshFilter>();
        ChangeBlockStage(BlockStage.None);
        ChangeChessBlockEffect(ChessBlockStage.Normal);

    }
    /// <summary>初期座標を原点としてマスを初期化します。</summary>
    private void Start()
    {
        Init(Vector2Int.zero);
    }


}   
