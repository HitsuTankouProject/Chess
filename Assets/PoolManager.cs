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

            Pool modelPool = new Pool(pair.Value.prefab);
            Pool effectPool = new Pool(pair.Value.chessEffect);

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

    public  GameObject Release(GameObject prefab)
    {

        return dictionary[prefab].PreparedObject(); 
    }
    public GameObject Release(GameObject prefab, Vector3 position)
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

