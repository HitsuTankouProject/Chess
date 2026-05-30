using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プー?を管?するシ?グ?ト?マネージ?ー。
/// プ?ハブごとに Pool を保?し、オブジェクトの生成・取得・返却を効率的に行う。
/// </summary>
public class PoolManager : MonoBehaviour
{
    static public PoolManager Instance { get; private set; }    // シ?グ?ト?イ?スタ?ス
    [SerializeField] Pool[] whiteChessPoolList;                 // Inspector で設定するプー?の配列（例：プ?イ?ーの弾丸など）
    [SerializeField] Pool[] blockChessPoolList;                 // Inspector で設定するプー?の配列（例：プ?イ?ーの弾丸など）

    Dictionary<GameObject, Pool> dictionary; // プ?ハブをキー、対?する Pool を値として管?する辞?

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        dictionary = new Dictionary<GameObject, Pool>();    // 辞?を?期化
    }

    /// <summary>
    /// 指定したプー?配列を?期化する?ソッド。
    /// 各プー?を辞?に登録し、プー?用の親オブジェクトを作成して管?する。
    /// </summary>
    /// <param name="pools">?期化するプー?配列</param>
    public void AllPoolInit()
    {
        foreach (var pool in whiteChessPoolList)
        {
#if UNITY_EDITOR
            // デバッグ用：同じ Prefab が既に辞?に登録されている場?はエ?ー表示
            if (dictionary.ContainsKey(pool.Prefab))
            {
                Debug.LogError("Same Prefab have been found!:" + pool.Prefab.name);

                continue;   // 同じキーが?ればスキップ
            }
#endif
            dictionary.Add(pool.Prefab, pool);          // プ?ハブをキーとしてプー?を辞?に追加

            // プー?用の親オブジェクトを作成
            Transform poolParent = new GameObject("Pool: " + pool.Prefab.name).transform;
            poolParent.parent = transform;              // このオブジェクトの子として設定

            pool.Initialize(poolParent);                // プー?を?期化
        }
        foreach (var pool in blockChessPoolList)
        {
#if UNITY_EDITOR
            // デバッグ用：同じ Prefab が既に辞?に登録されている場?はエ?ー表示
            if (dictionary.ContainsKey(pool.Prefab))
            {
                Debug.LogError("Same Prefab have been found!:" + pool.Prefab.name);

                continue;   // 同じキーが?ればスキップ
            }
#endif
            dictionary.Add(pool.Prefab, pool);          // プ?ハブをキーとしてプー?を辞?に追加

            // プー?用の親オブジェクトを作成
            Transform poolParent = new GameObject("Pool: " + pool.Prefab.name).transform;
            poolParent.parent = transform;              // このオブジェクトの子として設定

            pool.Initialize(poolParent);                // プー?を?期化
        }
    }

    /// <summary>
    /// 指定したプ?ハブに対?するプー?から使用可能なオブジェクトを取得する?ソッド（引?はプ?ハブのみ）。
    /// </summary>
    /// <param name="prefab">取得したいオブジェクトのプ?ハブ</param>
    /// <returns>?備された GameObject。プ?ハブがプー?に存在しない場?は null を返す。</returns>
    public  GameObject Release(GameObject prefab)
    {
#if UNITY_EDITOR 

        // デバッグ用：辞?にプ?ハブが存在しない場?エ?ー?ッセージを表示
        if (!dictionary.ContainsKey(prefab))
        {
            Debug.LogError("Pool Manager が指定プ?ハブを見つけられません:" + prefab.name);

            return null;  // プ?ハブが見つからない場?はnullを返す
        }
#endif
        return dictionary[prefab].PreparedObject();   // プ?ハブに対?するプー?からオブジェクトを取得
    }
    /// <summary>
    /// 指定したプ?ハブに対?するプー?から使用可能なオブジェクトを取得し、
    /// 指定した位置に配置して返す?ソッド。
    /// </summary>
    /// <param name="prefab">取得したいオブジェクトのプ?ハブ</param>
    /// <param name="position">オブジェクトを配置する?ー?ド座標</param>
    /// <returns>?備された GameObject。プ?ハブがプー?に存在しない場?は null を返す。</returns>
    public  GameObject Release(GameObject prefab, Vector3 position)
    {
#if UNITY_EDITOR
        // デバッグ用：辞?にプ?ハブが存在しない場?エ?ー?ッセージを表示
        if (!dictionary.ContainsKey(prefab))
        {
            Debug.LogError("Pool Manager が指定プ?ハブを見つけられません:" + prefab.name);

            return null;   // プ?ハブが見つからない場?はnullを返す

        }

#endif
        return dictionary[prefab].PreparedObject(position);    // 指定位置でオブジェクトを?備
    }
    /// <summary>
    /// 指定したプ?ハブに対?するプー?から使用可能なオブジェクトを取得し、
    /// 指定した位置と回転で配置して返す?ソッド。
    /// </summary>
    /// <param name="prefab">取得したいオブジェクトのプ?ハブ</param>
    /// <param name="position">オブジェクトを配置する?ー?ド座標</param>
    /// <param name="rotation">オブジェクトの回転</param>
    /// <returns>?備された GameObject。プ?ハブがプー?に存在しない場?は null を返す。</returns>
    public GameObject Release(GameObject prefab, Vector3 position, Quaternion rotation)
    {
#if UNITY_EDITOR
        // デバッグ用：辞?にプ?ハブが存在しない場?エ?ー?ッセージを表示
        if (!dictionary.ContainsKey(prefab))
        {
            Debug.LogError("Pool Manager が指定プ?ハブを見つけられません:" + prefab.name);

            return null;   // プ?ハブが見つからない場?はnullを返す

        }

#endif
        // プ?ハブに対?するプー?から指定位置・回転でオブジェクトを取得
        return dictionary[prefab].PreparedObject(position, rotation);
    }
    /// <summary>
    /// 指定したプ?ハブに対?するプー?から使用可能なオブジェクトを取得し、
    /// 指定した位置・回転・スケー?で配置して返す?ソッド。
    /// </summary>
    /// <param name="prefab">取得したいオブジェクトのプ?ハブ</param>
    /// <param name="position">オブジェクトを配置する?ー?ド座標</param>
    /// <param name="rotation">オブジェクトの回転</param>
    /// <param name="localScale">オブジェクトのスケー?・大きさ</param>
    /// <returns>?備された GameObject。プ?ハブがプー?に存在しない場?は null を返す。</returns>
    public  GameObject Release(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 localScale)
    {
#if UNITY_EDITOR
        // デバッグ用：辞?にプ?ハブが存在しない場?エ?ー?ッセージを表示
        if (!dictionary.ContainsKey(prefab))
        {
            Debug.LogError("Pool Manager が指定プ?ハブを見つけられません:" + prefab.name);

            return null;   // プ?ハブが見つからない場?はnullを返す

        }

#endif
        // プ?ハブに対?するプー?から指定位置・回転・スケー?でオブジェクトを取得
        return dictionary[prefab].PreparedObject(position, rotation, localScale);
    }

    public void ReturnToPool(GameObject prefab) => dictionary[prefab].Return(prefab);
}

