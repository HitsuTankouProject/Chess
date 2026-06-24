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
public class BoardInitData
{
    public string[] boardStartData;

    public Dictionary<Vector2Int, Pair<ChessColor, ChessType>> boardStartMap = new();
}

[System.Serializable]
public class ChessData
{
    public GameObject prefab;
    public Mesh model;
    public AllBuffCard buff01 { get; private set; }
    public AllBuffCard buff02 { get; private set; }
    public void SetBuff(AllBuffCard first, AllBuffCard secondy)
    {
        buff01 = first;
        buff02 = secondy;
    }

}
[System.Serializable]
public class CardData
{
    public Material m_CardCover;
    public string name { get; set; }
    public string buffLevel01Description { get; set; }
    public string buffLevel02Description { get; set; }
    public string buffLevel03Description { get; set; }
}



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
    #region Card Data
    [Header("Buff Card Data")]
    public CardData sageKing;
    public CardData madKing;
    public CardData witcher;
    public CardData beauty;
    public CardData sorcerer;
    public CardData monk;
    public CardData rusher;
    public CardData guardian;
    public CardData charger;
    public CardData skirmisher;
    public CardData scout;
    public CardData substitute;

    [Header("Card's Back Materials")]
    public Material m_CardBack;

    public Dictionary<AllBuffCard, CardData> cardDataDict { get; private set; } = new();
    public void CardDataDictInit()
    {
        cardDataDict = new Dictionary<AllBuffCard, CardData>()
        {
            // King
            { AllBuffCard.SageKing, sageKing },
            { AllBuffCard.MadKing, madKing },

            // Queen
            { AllBuffCard.Witcher, witcher },
            { AllBuffCard.Beauty, beauty },

            // Bishop
            { AllBuffCard.Sorcerer, sorcerer },
            { AllBuffCard.Monk, monk },

            // Rook
            { AllBuffCard.Rusher, rusher },
            { AllBuffCard.Guardian, guardian },

            // Knight
            { AllBuffCard.Charger, charger },
            { AllBuffCard.Skirmisher, skirmisher },

            // Pawn
            { AllBuffCard.Scout, scout },
            { AllBuffCard.Substitute, substitute },
    };
    }

    public enum Language { Japanese, English }
    private const string englishDescriptionPath = "Csvs/Card_Description/Card_Description_English";
    private string[] englishDescriptionCsvLines;

    private const string japaneseDescriptionPath = "Csvs/Card_Description/Card_Description_Japanese";
    private string[] japaneseDescriptionCsvLines;

    private const int dataIndexMax = 4;

    private string[] LanguageCsv(Language language) =>
        language == Language.Japanese ? japaneseDescriptionCsvLines : englishDescriptionCsvLines;

    private void CardDataUpdate(Language language)
    {
        string[] languageFile = LanguageCsv(language);

        if (languageFile == null || languageFile.Length < 1)
        {
            Debug.LogError("CSV is Null");
            return;
        }
        //Debug.Log(languageFile.Length);
        foreach (AllBuffCard buffCardName in Enum.GetValues(typeof(AllBuffCard)))
        {
            if (buffCardName == AllBuffCard.None) continue;
            int index = (int)buffCardName + 1;
            if (index >= languageFile.Length)
            {
                Debug.LogWarning($"CSV missing data for {buffCardName}");
                continue;
            }

            //Debug.Log($"RAW = [{languageFile[index]}]");

            string[] values = languageFile[index].Split(',');

            //Debug.Log($"Count = {values.Length}");
            //Debug.Log(string.Join(" | ", values));
            //Debug.Log(values[0].Trim());
            //Debug.Log(values[1].Trim());
            //Debug.Log(values[2].Trim());
            //Debug.Log(values[3].Trim());

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

    private bool LanguageDataInit()
    {
        bool haveEnglishLanguage = LoadCSV(englishDescriptionPath, out englishDescriptionCsvLines);
        bool haveJapaneseLanguage = LoadCSV(japaneseDescriptionPath, out japaneseDescriptionCsvLines);

        return haveEnglishLanguage && haveJapaneseLanguage;
    }

    private void CardDataInit()
    {
        CardDataDictInit();
        bool isAllLanguageDataInit = LanguageDataInit();
        if (!isAllLanguageDataInit)
        {
            Debug.LogError("AllLanguageDataInit Failed");
            return;
        }

        CardDataUpdate(Language.Japanese);
        

    }

    #endregion

    #region PlayerSprite
    [Header("PlayerSprite")]
    public Sprite sp_WhiteColor;
    public Sprite sp_BlackColor;
    public Sprite PlayerSprite(ChessColor color)
    {
        if (color == ChessColor.White) return sp_WhiteColor;
        else if (color == ChessColor.Black) return sp_BlackColor;

        return null;
    }

    #endregion

    #region BoardData

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

    private void BoardDataInit()
    {
        GetAllBoardDataFile();
    }


    #endregion

    #region ChessData
    [Header("Chess Model")]

    public ChessData king;
    public ChessData queen;
    public ChessData bishop;
    public ChessData rook;
    public ChessData knight;
    public ChessData pawn;

    public Dictionary<ChessType, ChessData> chessModelDict { get; private set; } = new();
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


    private void ChessDataInit()
    {
        king.SetBuff(AllBuffCard.SageKing, AllBuffCard.MadKing);
        queen.SetBuff(AllBuffCard.Witcher, AllBuffCard.Beauty);
        bishop.SetBuff(AllBuffCard.Sorcerer, AllBuffCard.Monk);
        rook.SetBuff(AllBuffCard.Rusher, AllBuffCard.Guardian);
        knight.SetBuff(AllBuffCard.Charger, AllBuffCard.Skirmisher);
        pawn.SetBuff(AllBuffCard.Scout, AllBuffCard.Substitute);

        chessModelDict = new()
        {
            [ChessType.King] = king,
            [ChessType.Queen] = queen,
            [ChessType.Bishop] = bishop,
            [ChessType.Rook] = rook,
            [ChessType.Knight] = knight,
            [ChessType.Pawn] = pawn,
        };
    }

    [Header("Chess Material")]
    public Material m_White;
    public Material m_Black;
    public Material TargetColor(ChessColor color)
    {
        if(color== ChessColor.White) return m_White;
        else if (color == ChessColor.Black) return m_Black;

        Debug.LogError("Why in here");
        return null;

    }
    public Material m_GotCurse;
    #endregion

    public void ResourcesInit()
    {
        CardDataInit();
        BoardDataInit();
        ChessDataInit();
    }
}
