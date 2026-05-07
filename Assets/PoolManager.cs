using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プールを管理するシングルトンマネージャー。
/// プレハブごとに Pool を保持し、オブジェクトの生成・取得・返却を効率的に行う。
/// </summary>
public class PoolManager : MonoBehaviour
{
    static public PoolManager Instance { get; private set; }    // シングルトンインスタンス
    [SerializeField] Pool[] whiteChessPoolList;                 // Inspector で設定するプールの配列（例：プレイヤーの弾丸など）
    [SerializeField] Pool[] blockChessPoolList;                 // Inspector で設定するプールの配列（例：プレイヤーの弾丸など）

    Dictionary<GameObject, Pool> dictionary; // プレハブをキー、対応する Pool を値として管理する辞書

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

        dictionary = new Dictionary<GameObject, Pool>();    // 辞書を初期化
    }

    /// <summary>
    /// 指定したプール配列を初期化するメソッド。
    /// 各プールを辞書に登録し、プール用の親オブジェクトを作成して管理する。
    /// </summary>
    /// <param name="pools">初期化するプール配列</param>
    public void AllPoolInit()
    {
        foreach (var pool in whiteChessPoolList)
        {
#if UNITY_EDITOR
            // デバッグ用：同じ Prefab が既に辞書に登録されている場合はエラー表示
            if (dictionary.ContainsKey(pool.Prefab))
            {
                Debug.LogError("Same Prefab have been found!:" + pool.Prefab.name);

                continue;   // 同じキーがあればスキップ
            }
#endif
            dictionary.Add(pool.Prefab, pool);          // プレハブをキーとしてプールを辞書に追加

            // プール用の親オブジェクトを作成
            Transform poolParent = new GameObject("Pool: " + pool.Prefab.name).transform;
            poolParent.parent = transform;              // このオブジェクトの子として設定

            pool.Initialize(poolParent);                // プールを初期化
        }
        foreach (var pool in blockChessPoolList)
        {
#if UNITY_EDITOR
            // デバッグ用：同じ Prefab が既に辞書に登録されている場合はエラー表示
            if (dictionary.ContainsKey(pool.Prefab))
            {
                Debug.LogError("Same Prefab have been found!:" + pool.Prefab.name);

                continue;   // 同じキーがあればスキップ
            }
#endif
            dictionary.Add(pool.Prefab, pool);          // プレハブをキーとしてプールを辞書に追加

            // プール用の親オブジェクトを作成
            Transform poolParent = new GameObject("Pool: " + pool.Prefab.name).transform;
            poolParent.parent = transform;              // このオブジェクトの子として設定

            pool.Initialize(poolParent);                // プールを初期化
        }
    }

    /// <summary>
    /// 指定したプレハブに対応するプールから使用可能なオブジェクトを取得するメソッド（引数はプレハブのみ）。
    /// </summary>
    /// <param name="prefab">取得したいオブジェクトのプレハブ</param>
    /// <returns>準備された GameObject。プレハブがプールに存在しない場合は null を返す。</returns>
    public  GameObject Release(GameObject prefab)
    {
#if UNITY_EDITOR 

        // デバッグ用：辞書にプレハブが存在しない場合エラーメッセージを表示
        if (!dictionary.ContainsKey(prefab))
        {
            Debug.LogError("Pool Manager が指定プレハブを見つけられません:" + prefab.name);

            return null;  // プレハブが見つからない場合はnullを返す
        }
#endif
        return dictionary[prefab].PreparedObject();   // プレハブに対応するプールからオブジェクトを取得
    }
    /// <summary>
    /// 指定したプレハブに対応するプールから使用可能なオブジェクトを取得し、
    /// 指定した位置に配置して返すメソッド。
    /// </summary>
    /// <param name="prefab">取得したいオブジェクトのプレハブ</param>
    /// <param name="position">オブジェクトを配置するワールド座標</param>
    /// <returns>準備された GameObject。プレハブがプールに存在しない場合は null を返す。</returns>
    public  GameObject Release(GameObject prefab, Vector3 position)
    {
#if UNITY_EDITOR
        // デバッグ用：辞書にプレハブが存在しない場合エラーメッセージを表示
        if (!dictionary.ContainsKey(prefab))
        {
            Debug.LogError("Pool Manager が指定プレハブを見つけられません:" + prefab.name);

            return null;   // プレハブが見つからない場合はnullを返す

        }

#endif
        return dictionary[prefab].PreparedObject(position);    // 指定位置でオブジェクトを準備
    }
    /// <summary>
    /// 指定したプレハブに対応するプールから使用可能なオブジェクトを取得し、
    /// 指定した位置と回転で配置して返すメソッド。
    /// </summary>
    /// <param name="prefab">取得したいオブジェクトのプレハブ</param>
    /// <param name="position">オブジェクトを配置するワールド座標</param>
    /// <param name="rotation">オブジェクトの回転</param>
    /// <returns>準備された GameObject。プレハブがプールに存在しない場合は null を返す。</returns>
    public GameObject Release(GameObject prefab, Vector3 position, Quaternion rotation)
    {
#if UNITY_EDITOR
        // デバッグ用：辞書にプレハブが存在しない場合エラーメッセージを表示
        if (!dictionary.ContainsKey(prefab))
        {
            Debug.LogError("Pool Manager が指定プレハブを見つけられません:" + prefab.name);

            return null;   // プレハブが見つからない場合はnullを返す

        }

#endif
        // プレハブに対応するプールから指定位置・回転でオブジェクトを取得
        return dictionary[prefab].PreparedObject(position, rotation);
    }
    /// <summary>
    /// 指定したプレハブに対応するプールから使用可能なオブジェクトを取得し、
    /// 指定した位置・回転・スケールで配置して返すメソッド。
    /// </summary>
    /// <param name="prefab">取得したいオブジェクトのプレハブ</param>
    /// <param name="position">オブジェクトを配置するワールド座標</param>
    /// <param name="rotation">オブジェクトの回転</param>
    /// <param name="localScale">オブジェクトのスケール・大きさ</param>
    /// <returns>準備された GameObject。プレハブがプールに存在しない場合は null を返す。</returns>
    public  GameObject Release(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 localScale)
    {
#if UNITY_EDITOR
        // デバッグ用：辞書にプレハブが存在しない場合エラーメッセージを表示
        if (!dictionary.ContainsKey(prefab))
        {
            Debug.LogError("Pool Manager が指定プレハブを見つけられません:" + prefab.name);

            return null;   // プレハブが見つからない場合はnullを返す

        }

#endif
        // プレハブに対応するプールから指定位置・回転・スケールでオブジェクトを取得
        return dictionary[prefab].PreparedObject(position, rotation, localScale);
    }

    public void ReturnToPool(GameObject prefab) => dictionary[prefab].Return(prefab);
}

