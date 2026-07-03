using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessEffect : MonoBehaviour
{
    private ResourcesData _resourcesData => GameManager.Instance.resourcesData;
    public PoolManager _poolManager => PoolManager.Instance;
    
    public PoolObject poolObject => this.gameObject.GetComponent<PoolObject>();

    [Header("Effect Basic")]
    public float speed = 5f;
    public float rotateSpeed = 360f;
    public float lifeTime = 0.8f;

    private List<Vector3> allPiecesLocalPosition = new();
    private List<FlyingPiece> allPieces = new();
    private void Awake()
    {
        foreach (Transform piece in transform)
        {
            allPiecesLocalPosition.Add(piece.localPosition);
            allPieces.Add(piece.gameObject.AddComponent<FlyingPiece>());
        }
        gameObject.SetActive(false);

    }

    private IEnumerator TurnOffEffect()
    {
        yield return new WaitForSeconds(lifeTime * 1.1f);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).localPosition = allPiecesLocalPosition[i];
        }

        if (poolObject != null && _poolManager != null) poolObject.pool.Return(this.gameObject);
        else gameObject.SetActive(false);
    }
    public void PlayEffect(ChessColor color)
    {
        Material material = _resourcesData.TargetColor(color);
        for (int i = 0; i < transform.childCount; i++)
        {
            Vector3 dir = Random.onUnitSphere;
            allPieces[i].Init(lifeTime,dir * speed, rotateSpeed, material);
        }
        StartCoroutine(TurnOffEffect());
    }

}
