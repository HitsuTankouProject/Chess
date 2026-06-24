using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; } 
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

    public void AllPoolInit()
    {
        foreach (var pair in GameManager.Instance.resourcesData.chessModelDict)
        {
            if (pair.Value == null)
            {
                Debug.LogError($"{pair.Key} Model Missing");
                continue;
            }

            Pool pool = new Pool(pair.Value.prefab);
            Transform parent = new GameObject($"Pool : {pair.Key}").transform;

            parent.SetParent(transform);
            pool.Initialize(parent);

            dictionary[pair.Value.prefab] = pool;
        }
    }

    //public void AllPoolInit()
    //{



    //    foreach (GameObject chess in GameManager.Instance.resourcesData.chessModelDict.Values)
    //    {
    //        if (chess == null)
    //        {
    //            Debug.LogError("Chess Model is null");
    //            continue;
    //        }
    //        Pool pool = new Pool();
    //        dictionary.Add(chess, pool);
    //        Transform poolParent = new GameObject("Pool: " + chess.name).transform;
    //        poolParent.parent = transform;
    //        pool.Initialize(poolParent);

    //    }

    //    foreach (var pool in chessPoolList)
    //    {

    //        dictionary.Add(pool.Prefab, pool);          

    //        Transform poolParent = new GameObject("Pool: " + pool.Prefab.name).transform;
    //        poolParent.parent = transform;              

    //        pool.Initialize(poolParent);                
    //    }
    //}

    public  GameObject Release(GameObject prefab)
    {

        return dictionary[prefab].PreparedObject(); 
    }
    public  GameObject Release(GameObject prefab, Vector3 position)
    {

        return dictionary[prefab].PreparedObject(position);    
    }
  
    public GameObject Release(GameObject prefab, Vector3 position, Quaternion rotation)
    {

        
        return dictionary[prefab].PreparedObject(position, rotation);
    }
        public  GameObject Release(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 localScale)
    {

        return dictionary[prefab].PreparedObject(position, rotation, localScale);
    }

    public void ReturnToPool(GameObject prefab) => dictionary[prefab].Return(prefab);
}

