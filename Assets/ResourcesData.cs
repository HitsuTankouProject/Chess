using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public enum AllBuffCard
{
    SageKing, MadKing,
    Witcher, Beauty,
    Sorcerer, Monk,
    Rusher, Guardian,
    Charger, Skirmisher,
    Scout, Substitute,


    None = -1
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

    private const int dataIndexMax = 4;

    private string[] LanguageCsv(Language language) =>
        language == Language.Japanese ? japaneseDescriptionCsvLines : englishDescriptionCsvLines;

    public Dictionary<AllBuffCard, CardData> cardDataDict { get; private set; } = new()
        {
            [AllBuffCard.SageKing] = new(),
            [AllBuffCard.MadKing] = new(),

            [AllBuffCard.Witcher] = new(),
            [AllBuffCard.Beauty] = new(),

            [AllBuffCard.Sorcerer] = new(),
            [AllBuffCard.Monk] = new(),

            [AllBuffCard.Rusher] = new(),
            [AllBuffCard.Guardian] = new(),

            [AllBuffCard.Charger] = new(),
            [AllBuffCard.Skirmisher] = new(),

            [AllBuffCard.Scout] = new(),
            [AllBuffCard.Substitute] = new(),
        };


    private void CardDataUpdate(Language language)
    {
        string[] languageFile = LanguageCsv(language);

        if (languageFile == null || languageFile.Length < 1)
        {
            Debug.LogError("CSV is Null");
            return;
        }
        Debug.Log(languageFile.Length);
        foreach (AllBuffCard buffCardName in Enum.GetValues(typeof(AllBuffCard)))
        {
            if (buffCardName == AllBuffCard.None) continue;
            int index = (int)buffCardName + 1;
            if (index >= languageFile.Length)
            {
                Debug.LogWarning($"CSV missing data for {buffCardName}");
                continue;
            }

            Debug.Log($"RAW = [{languageFile[index]}]");

            string[] values = languageFile[index].Split(',');

            Debug.Log($"Count = {values.Length}");
            Debug.Log(string.Join(" | ", values));
            Debug.Log(values[0].Trim());
            Debug.Log(values[1].Trim());
            Debug.Log(values[2].Trim());
            Debug.Log(values[3].Trim());

            if (values.Length < dataIndexMax)
            {
                Debug.LogWarning($"{buffCardName} CSV format error");
                continue;
            }



            if (values[0].Trim() != null) cardDataDict[buffCardName].name = values[0].Trim();
            if (values[1].Trim() != null) cardDataDict[buffCardName].buffLevel01Description = values[1].Trim();
            if (values[2].Trim() != null) cardDataDict[buffCardName].buffLevel02Description = values[2].Trim();
            if (values[3].Trim() != null) cardDataDict[buffCardName].buffLevel03Description = values[3].Trim();

        }
    }



    private void LanguageDataInit()
    {
        LoadCSV(englishDescriptionPath, out englishDescriptionCsvLines);
        LoadCSV(japaneseDescriptionPath, out japaneseDescriptionCsvLines);

    }

    public void CardDataDictInit()
    {
        LanguageDataInit();

        Dictionary<AllBuffCard, Material> cardMaterialDict = CardMaterialDict();
        foreach (AllBuffCard buffCardName in Enum.GetValues(typeof(AllBuffCard)))
        {
            if (buffCardName == AllBuffCard.None) continue;
            if( cardMaterialDict[buffCardName] == null)
            {
                Debug.LogWarning($"{buffCardName} : Cover Material loss");
                continue;
            }
            cardDataDict[buffCardName].m_CardCover = cardMaterialDict[buffCardName];
        }

        CardDataUpdate(Language.Japanese);
    }

    #endregion


}
