using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲーム内で使用する駒とエフェクトのオブジェクトプールを一元管理します。
/// Prefabごとに対応する <see cref="Pool" /> を生成して辞書へ登録し、
/// 必要な位置、回転、スケールを指定して再利用可能なオブジェクトを提供します。
/// 使用を終えたオブジェクトは、対応するプールへ返却できます。
/// </summary>
public class PoolManager : MonoBehaviour
{
    /// <summary>オブジェクトプール管理の共有インスタンスを取得します。</summary>
    public static PoolManager Instance { get; private set; }
    /// <summary>Prefabと、そのPrefab専用プールの対応表です。</summary>
    private Dictionary<GameObject, Pool> dictionary;

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

        dictionary = new Dictionary<GameObject, Pool>();
    }

    /// <summary>
    /// 登録されている全駒種について、駒本体とエフェクトの専用プールを作成します。
    /// 各プールにはHierarchy上で識別できる親オブジェクトを割り当て、
    /// Prefabをキーとして後から取得できるよう辞書へ登録します。
    /// </summary>
    public void AllPoolInit()
    {
        foreach (var pair in GameManager.Instance.resourcesData.chessModelDict)
        {
            if (pair.Value == null)
            {
                Debug.LogError($"{pair.Key} Model Missing");
                continue;
            }

            // 駒本体と、その駒種で使用する演出用Prefabのプールを個別に作成します。
            Pool modelPool = new Pool(pair.Value.prefab);
            Pool effectPool = new Pool(pair.Value.chessEffect);

            // プール内オブジェクトを駒種ごとに整理する親Transformを作成します。
            Transform modelPoolParent = new GameObject($"Pool : {pair.Key}").transform;
            Transform effectPoolParent = new GameObject($"Pool : {pair.Key}'s Effect").transform;

            modelPoolParent.SetParent(transform);
            effectPoolParent.SetParent(transform);

            modelPool.Initialize(modelPoolParent);
            effectPool.Initialize(effectPoolParent);

            dictionary[pair.Value.prefab] = modelPool;
            dictionary[pair.Value.chessEffect] = effectPool;

        }
    }

    /// <summary>指定Prefabのオブジェクトを既定Transformでプールから取得します。</summary>
    /// <param name="prefab">取得するオブジェクトのPrefabです。</param>
    /// <returns>使用可能な状態へ準備されたオブジェクトです。</returns>
    public GameObject Release(GameObject prefab)
    {

        return dictionary[prefab].PreparedObject(); 
    }
    /// <summary>指定Prefabのオブジェクトを、指定位置へ配置してプールから取得します。</summary>
    /// <param name="prefab">取得するオブジェクトのPrefabです。</param>
    /// <param name="position">設定するワールド座標です。</param>
    /// <returns>指定位置へ配置されたオブジェクトです。</returns>
    public GameObject Release(GameObject prefab, Vector3 position)
    {

        return dictionary[prefab].PreparedObject(position);    
    }
    /// <summary>指定Prefabのオブジェクトへ位置と回転を設定してプールから取得します。</summary>
    /// <param name="prefab">取得するオブジェクトのPrefabです。</param>
    /// <param name="position">設定するワールド座標です。</param>
    /// <param name="rotation">設定する回転です。</param>
    /// <returns>指定された位置と回転へ設定されたオブジェクトです。</returns>
    public GameObject Release(GameObject prefab, Vector3 position, Quaternion rotation)
    {

        
        return dictionary[prefab].PreparedObject(position, rotation);
    }
    /// <summary>指定Prefabのオブジェクトへ位置、回転、スケールを設定してプールから取得します。</summary>
    /// <param name="prefab">取得するオブジェクトのPrefabです。</param>
    /// <param name="position">設定するワールド座標です。</param>
    /// <param name="rotation">設定する回転です。</param>
    /// <param name="localScale">設定するローカルスケールです。</param>
    /// <returns>指定されたTransform情報へ設定されたオブジェクトです。</returns>
    public GameObject Release(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 localScale)
    {

        return dictionary[prefab].PreparedObject(position, rotation, localScale);
    }
    /// <summary>指定オブジェクトを、Prefabをキーとする対応プールへ返却します。</summary>
    /// <param name="prefab">プールへ返却するオブジェクトです。</param>
    public void ReturnToPool(GameObject prefab) => dictionary[prefab].Return(prefab);
}

