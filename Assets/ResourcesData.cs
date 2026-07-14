using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Data;
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
public enum Language { Japanese, English }

public class ResourcesData : MonoBehaviour
{
    private bool LoadCSV(string pathKey, out string[] csvLine)
    {
        if (string.IsNullOrEmpty(pathKey))
        {
            Debug.LogError("CSV key null");
            csvLine = null;
            return false;
        }

        string path = Path.Combine(Application.streamingAssetsPath, pathKey + ".csv");

        // CSV 存在チェック
        if (!File.Exists(path))
        {
            Debug.LogWarning("文件不存在: " + path);
            csvLine = null;
            return false;
        }

        // CSV 読み込み
        string[] lines = File.ReadAllLines(path, new UTF8Encoding(true));

        // 前後空白削除
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Trim();
        }

        csvLine = lines;
        return true;

    }

    #region ChessData
    [Header("Chess Model")]

    public AllChessModel allChessModel;
    public Dictionary<ChessType, ChessData> chessModelDict => allChessModel.chessModelDict;
    public ChessData FindChessDataByBuff(AllBuffCard targetBuff)
    {
        foreach (ChessData chessData in chessModelDict.Values)
        {
            if (chessData.buff01 == targetBuff ||
                chessData.buff02 == targetBuff)
            {
                return chessData;
            }
        }

        return null;
    }
    private void ChessDataInit() => allChessModel.ChessDataInit();

    public Material TargetColor(ChessColor color)
    {
        if (color == ChessColor.White) return allMaterial.m_White;
        else return allMaterial.m_Black;
    }

    #endregion

    #region Card Data
    [Header("Buff Card Data")]
    public AllCradData cradDataList;
    public Dictionary<AllBuffCard, CardData> cardDataDict => cradDataList.cardDataDict;
    public void CardDataDictInit()=> cradDataList.CardDataDictInit();
    private AllBoardInitData allBoardInitData = new();
    private Dictionary<Language, string[]> allDescriptionCsvLines => allBoardInitData.allDescriptionCsvLines;

    private const int dataIndexMax = 4;

    private string[] LanguageCsv(Language language) => allDescriptionCsvLines[language];
    private string DescriptionPath(Language language)
        => $"Csvs/Card_Description/Card_Description_{language.ToString()}";

    public void CardDataUpdate(Language language)
    {
        string[] languageFile = LanguageCsv(language);

        if (languageFile == null || languageFile.Length < 1)
        {
            Debug.LogError("CSV is Null");
            return;
        }
        foreach (AllBuffCard buffCardName in Enum.GetValues(typeof(AllBuffCard)))
        {
            if (buffCardName == AllBuffCard.None) continue;
            int index = (int)buffCardName + 1;
            if (index >= languageFile.Length)
            {
                Debug.LogWarning($"CSV missing data for {buffCardName}");
                continue;
            }

            string[] values = languageFile[index].Split(',');

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
        foreach (Language language in Enum.GetValues(typeof(Language)))
        {
            bool haveLanguage = LoadCSV(DescriptionPath(language), out string[] descriptionCsvLines);
            if (!haveLanguage)
            {
                Debug.LogError($"{language.ToString()} Failed");
                return;
            }
            else allBoardInitData.AddToDescriptionCsvLines(language, descriptionCsvLines);


        }

    }

    private void CardDataInit()
    {
        CardDataDictInit();
        LanguageDataInit();
        CardDataUpdate(Language.Japanese);
    }

    #endregion

    #region PlayerSprite
    [Header("All Sprite")]
    public AllSprite allSprite;
    public Sprite PlayerSprite(ChessColor color)
    {
        if (color == ChessColor.White) return allSprite.sp_WhiteColor;
        else if (color == ChessColor.Black) return allSprite.sp_BlackColor;

        return null;
    }

    #endregion

    #region BoardData

    #region Chess Start Map
    private Dictionary<string, BoardInitData> boardData = new();
    private const int board_Width = 8;
    private const int board_Height = 8;
    public Dictionary<Vector2Int, Pair<ChessColor, ChessType>> GetBcoardInitData(string fileName)
    {
        if (!boardData.ContainsKey(fileName))
        {
            Debug.LogError("No File Excite");
            return default;
        }
        else return boardData[fileName].boardStartMap;
    }
    private void GetAllBoardDataFile()
    {
        string folderPath = Path.Combine(Application.streamingAssetsPath, "Csvs", "TurnStage");

        string[] csvFiles = Directory.GetFiles(folderPath, "*.csv");

        foreach (string filePath in csvFiles)
        {
            LoadBoardData(Path.GetFileName(filePath), filePath);
        }
    }
    private void LoadBoardData(string fileName, string filePath)
    {
        BoardInitData data = new BoardInitData();
        data.boardStartData = File.ReadAllLines(filePath);
        data.boardStartMap = new();

        for (int y = 0; y < board_Height; y++)
        {
            string[] row = data.boardStartData[y].Split(',');

            for (int x = 0; x < board_Width; x++)
            {
                string cell = row[x];
                if (string.IsNullOrWhiteSpace(cell)) continue;
                string[] chessData = cell.Split(':');

                ChessColor color = Enum.Parse<ChessColor>(chessData[0]);

                ChessType type = Enum.Parse<ChessType>(chessData[1]);

                data.boardStartMap.Add(new Vector2Int(x, y),
                    new Pair<ChessColor, ChessType>(color, type));
            }
        }

        boardData[fileName] = data;
    }

    #endregion

    #region Material
    [Header("All Material")]
    public AllMaterial allMaterial;
    #endregion

    private void BoardDataInit()
    {
        GetAllBoardDataFile();
    }


    #endregion

    #region Adudio Data
    [Header("Bgm Data")]
    public BgmData GameTitleBgm;
    public BgmData inGameBgm;
    public BgmData releaseBgm;

    [Header("Bgm Data")]

    public SfxData pressButtonSfx;

    public SfxData gameStartSfx;
    public SfxData turnChangeSfx;

    public SfxData pickChessSfx;
    public SfxData putChessSfx;

    #endregion

    public void ResourcesInit()
    {
        CardDataInit();
        BoardDataInit();
        ChessDataInit();
    }
}
