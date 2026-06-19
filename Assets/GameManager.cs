using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Scene { Loading,GameTitle,InGame,Release,Error}


public class GameManager : MonoBehaviour
{
    public static GameManager Instance {  get; private set; }
    public ResourcesData resourcesData;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    #region Resources

    private void ResourcesInit()
    {
        resourcesData.CardMaterialDictInit();
    }

    public Pair<Material, Material> CardMaterials(AllBuffCard buffCard) => resourcesData.cardMaterialDict[buffCard];



    #endregion

    private void Start()
    {
        ResourcesInit();
        SceneManager.LoadScene("InGame", LoadSceneMode.Single);
    }
}
