using UnityEngine;

[System.Serializable]
public struct Adddd
{
    public int sss;
}

public class aaa : MonoBehaviour
{
    public GameObject sss;
    public Material black;
    public Material white;

    public Adddd asss;

    private void Start()
    {
        float scaleX = sss.transform.localScale.x;
        float scaleZ = sss.transform.localScale.z;


        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                GameObject chessBlock = Instantiate(sss, new Vector3(0 + i*10, 0, 0 + j*10), Quaternion.identity);
                chessBlock.name = $"Pos_{i}_{j}";
                int sum = (i - j) % 2;
                chessBlock.transform.GetChild(0).GetComponent<MeshRenderer>().material = sum==0? black: white;

            }
        }
    }

}
