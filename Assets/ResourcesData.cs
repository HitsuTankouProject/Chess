using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public enum AllBuffCard
{
    None = 0,
    SageKing, MadKing,
    Witcher, Beauty,
    Sorcerer, Monk,
    Rusher, Guardian,
    Charger, Skirmisher,
    Scout, Substitute
}

public class ResourcesData : MonoBehaviour
{
    private void LoadCSV(string pathKey , out string[] csvLine)
    {
        if (string.IsNullOrEmpty(pathKey))
        {
            Debug.LogError("CSV key null");
        }

        string path = Path.Combine(
            Application.streamingAssetsPath,
            pathKey + ".csv"
        );

        // CSV 存在チェック
        if (!File.Exists(path))
        {
            Debug.LogWarning("文件不存在: " + path);
            csvLine = null;
            return;
        }

        // CSV 読み込み
        string[] lines = File.ReadAllLines(path, new UTF8Encoding(true));

        // 前後空白削除
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Trim();
        }

        csvLine = lines;
    }




    #region Card Data
    #region Card Materials

    [Header("King's Card Materials")]
    public Material m_Card_King_SageKing;
    public Material m_Card_King_MadKing;
    [Header("Queen's Card Materials")]
    public Material m_Card_Queen_Witcher;
    public Material m_Card_Queen_Beauty;
    [Header("Bishop's Card Materials")]
    public Material m_Card_Bishop_Sorcerer;
    public Material m_Card_Bishop_Monk;
    [Header("Rook's Card Materials")]
    public Material m_Card_Rook_Rusher;
    public Material m_Card_Rook_Guardian;
    [Header("Knight's Card Materials")]
    public Material m_Card_Knight_Charger;
    public Material m_Card_Knight_Skirmisher;
    [Header("Pawn's Card Materials")]
    public Material m_Card_Pawn_Scout;
    public Material m_Card_Pawn_Substitute;
    [Header("Card's Back Materials")]
    public Material m_CardBack;

    public Dictionary<AllBuffCard, Material> cardMaterialDict {  get; private set; }
    public void CardMaterialDictInit()
    {
        cardMaterialDict = new Dictionary<AllBuffCard, Material>()
            {
                {AllBuffCard.None, null },

                {AllBuffCard.SageKing, m_Card_King_SageKing },
                {AllBuffCard.MadKing, m_Card_King_MadKing },

                {AllBuffCard.Witcher, m_Card_Queen_Witcher },
                {AllBuffCard.Beauty, m_Card_Queen_Beauty },

                {AllBuffCard.Sorcerer, m_Card_Bishop_Sorcerer },
                {AllBuffCard.Monk, m_Card_Bishop_Monk },

                {AllBuffCard.Rusher, m_Card_Rook_Rusher },
                {AllBuffCard.Guardian, m_Card_Rook_Guardian },

                {AllBuffCard.Charger, m_Card_Knight_Charger },
                {AllBuffCard.Skirmisher, m_Card_Knight_Skirmisher },

                {AllBuffCard.Scout, m_Card_Pawn_Scout },
                {AllBuffCard.Substitute, m_Card_Pawn_Substitute }
            };
    }

    public Dictionary<AllBuffCard, Material> CardMaterialDict()
    {
        Dictionary<AllBuffCard, Material> result 
            = new Dictionary<AllBuffCard, Material>()
            {
                {AllBuffCard.None, null },

                {AllBuffCard.SageKing, m_Card_King_SageKing },
                {AllBuffCard.MadKing, m_Card_King_MadKing },

                {AllBuffCard.Witcher, m_Card_Queen_Witcher },
                {AllBuffCard.Beauty, m_Card_Queen_Beauty },

                {AllBuffCard.Sorcerer, m_Card_Bishop_Sorcerer },
                {AllBuffCard.Monk, m_Card_Bishop_Monk },

                {AllBuffCard.Rusher, m_Card_Rook_Rusher },
                {AllBuffCard.Guardian, m_Card_Rook_Guardian },

                {AllBuffCard.Charger, m_Card_Knight_Charger },
                {AllBuffCard.Skirmisher, m_Card_Knight_Skirmisher },

                {AllBuffCard.Scout, m_Card_Pawn_Scout },
                {AllBuffCard.Substitute, m_Card_Pawn_Substitute }
            };

        return result;
    }


    #endregion
    public enum Language { Japanese, English }
    private const string englishDescriptionPath = "Csvs/Card_Description/Card_Description_English";
    private string[] englishDescriptionCsvLines;

    private const string japaneseDescriptionPath = "Csvs/Card_Description/Card_Description_Japanese";
    private string[] japaneseDescriptionCsvLines;

    private string[] LanguageCsv(Language language) =>
        language == Language.Japanese ? japaneseDescriptionCsvLines : englishDescriptionCsvLines;

    public Dictionary<AllBuffCard, CardData> cardDataDict { get; private set; }=
        new Dictionary<AllBuffCard, CardData>();




    public void CardDataInit()
    {
        cardDataDict.Clear();
        LoadCSV(englishDescriptionPath, out englishDescriptionCsvLines);
        LoadCSV(japaneseDescriptionPath, out japaneseDescriptionCsvLines);

        Debug.Log(englishDescriptionCsvLines[0]);
        Debug.Log(japaneseDescriptionCsvLines[0]);



    }

    #endregion


}
