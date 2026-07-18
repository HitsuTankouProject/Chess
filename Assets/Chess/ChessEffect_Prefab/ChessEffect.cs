using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static UnityEngine.Audio.ProcessorInstance;


public enum EffectType { Swapn, Dead }

public class ChessEffect : MonoBehaviour
{
    private ResourcesData _resourcesData => GameManager.Instance.resourcesData;
    public PoolManager _poolManager => PoolManager.Instance;
    public PoolObject poolObject => this.gameObject.GetComponent<PoolObject>();
    private List<MeshRenderer> allPieces = new();


    [Header("Effect Basic")]
    //public Material m_material;
    public Animator animator;
    public bool isEffectFinish { get; private set; } = false;

    private void TrigegerOn(EffectType effectType)
        => animator.SetTrigger(effectType.ToString());


    private void Awake()
    {
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

    public void EffectFinish()
    {
        isEffectFinish = true;
        poolObject.pool.Return(this.gameObject);
    }

    public void PlayEffect(EffectType effectType, ChessColor color)
    {
        isEffectFinish = false;
        Material targetMaterial = _resourcesData.TargetColor(color);
        for (int i = 0; i < transform.childCount; i++)
            allPieces[i].material = targetMaterial;

        TrigegerOn(effectType);
    }

    public void PlayEffect(EffectType effectType, Material addMaterial)
    {
        isEffectFinish = false;

        for (int i = 0; i < transform.childCount; i++)
            allPieces[i].material = addMaterial;

        TrigegerOn(effectType);
    }

}
