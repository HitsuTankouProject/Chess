using System.IO;
using System.Text;
using UnityEngine;

public class CxvReader : MonoBehaviour
{
    //English
    private const string csvPath = "Csvs/Card_Description/Card_Description_Japanese";
    private string filename;

    private string[] csvLines;

    private const int dataIndexMax = 5;

    void LoadCSV()
    {
        string key = csvPath;

        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("CSV key null");
        }

        string path = Path.Combine(
            Application.streamingAssetsPath,
            key + ".csv"
        );

        // CSV 存在チェック
        if (!File.Exists(path))
        {
            Debug.LogWarning("文件不存在: " + path);
            return;
        }

        // CSV 読み込み
        string[] lines = File.ReadAllLines(path, new UTF8Encoding(true));

        // 前後空白削除
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Trim();
        }

        csvLines = lines;
    }


}
