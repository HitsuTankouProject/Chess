using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static UnityEngine.Audio.ProcessorInstance;

/// <summary>チェス駒へ適用する演出の種類を表します。</summary>
public enum EffectType
{
    /// <summary>駒を盤面へ生成する際の演出です。</summary>
    Swapn,
    /// <summary>駒が捕獲されて盤面から消える際の演出です。</summary>
    Dead
}

/// <summary>
/// チェス駒の生成・消滅エフェクトを管理します。
/// 子オブジェクトのすべてのメッシュへ指定色またはマテリアルを適用し、
/// エフェクト種別に対応する Animator のトリガーを実行します。
/// アニメーション完了後は、このオブジェクトをプールへ返却します。
/// </summary>
public class ChessEffect : MonoBehaviour
{
    /// <summary>ゲーム内で共有するマテリアル情報を取得します。</summary>
    private ResourcesData _resourcesData => GameManager.Instance.resourcesData;
    /// <summary>エフェクト用オブジェクトプールの共有インスタンスを取得します。</summary>
    public PoolManager _poolManager => PoolManager.Instance;
    /// <summary>このエフェクトに設定されているプール管理コンポーネントを取得します。</summary>
    public PoolObject poolObject => this.gameObject.GetComponent<PoolObject>();
    /// <summary>エフェクトを構成するすべての子メッシュを保持します。</summary>
    private List<MeshRenderer> allPieces = new();


    [Header("Effect Basic")]

    /// <summary>生成・消滅アニメーションを制御する Animator です。</summary>
    public Animator animator;
    /// <summary>現在のエフェクト再生が完了したかどうかを取得します。</summary>
    public bool isEffectFinish { get; private set; } = false;
    /// <summary>指定されたエフェクト種別に対応する Animator トリガーを実行します。</summary>
    /// <param name="effectType">再生するエフェクトの種類です。</param>
    private void TrigegerOn(EffectType effectType) => animator.SetTrigger(effectType.ToString());

    /// <summary>
    /// 子オブジェクトからエフェクトを構成するメッシュを収集し、
    /// Animator の再生速度を設定します。
    /// </summary>
    private void Awake()
    {
        // 各子オブジェクトの MeshRenderer を後続の素材変更用に保存します。
        for (int i = 0; i < transform.childCount; i++)
        {
            if (!transform.GetChild(i).TryGetComponent<MeshRenderer>(out MeshRenderer childMeshRenderer))
            {
                Debug.LogError(gameObject.name + $"Child : {i} no have MeshRenderer");
                continue;
            }

            allPieces.Add(childMeshRenderer);
        }
        animator.speed = 2.0f;
    }

    /// <summary>エフェクトを完了状態にし、このオブジェクトをプールへ返却します。</summary>
    public void EffectFinish()
    {
        isEffectFinish = true;
        poolObject.pool.Return(this.gameObject);
    }
    /// <summary>
    /// 指定した駒色のマテリアルをすべての子メッシュへ適用し、エフェクトを再生します。
    /// </summary>
    /// <param name="effectType">再生するエフェクトの種類です。</param>
    /// <param name="color">エフェクトへ反映する駒色です。</param>
    public void PlayEffect(EffectType effectType, ChessColor color)
    {
        isEffectFinish = false;
        Material targetMaterial = _resourcesData.TargetColor(color);

        // エフェクトを構成するすべてのパーツへ同じ駒色を設定します。
        for (int i = 0; i < transform.childCount; i++)
            allPieces[i].material = targetMaterial;

        TrigegerOn(effectType);
    }

    /// <summary>
    /// 指定したマテリアルをすべての子メッシュへ適用し、エフェクトを再生します。
    /// </summary>
    /// <param name="effectType">再生するエフェクトの種類です。</param>
    /// <param name="addMaterial">エフェクトへ適用するマテリアルです。</param>
    public void PlayEffect(EffectType effectType, Material addMaterial)
    {
        isEffectFinish = false;

        // エフェクトを構成するすべてのパーツへ指定マテリアルを設定します。
        for (int i = 0; i < transform.childCount; i++)
            allPieces[i].material = addMaterial;

        TrigegerOn(effectType);
    }

}
