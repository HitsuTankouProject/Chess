using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Data;

/// <summary>ゲーム内で選択できるすべてのバフカード種別を表します。</summary>
public enum AllBuffCard
{
    /// <summary>キング用のSageKingバフです。</summary>
    SageKing,
    /// <summary>キング用のMadKingバフです。</summary>
    MadKing,
    /// <summary>クイーン用のWitcherバフです。</summary>
    Witcher,
    /// <summary>クイーン用のBeautyバフです。</summary>
    Beauty,
    /// <summary>ビショップ用のSorcererバフです。</summary>
    Sorcerer,
    /// <summary>ビショップ用のMonkバフです。</summary>
    Monk,
    /// <summary>ルーク用のRusherバフです。</summary>
    Rusher,
    /// <summary>ルーク用のGuardianバフです。</summary>
    Guardian,
    /// <summary>ナイト用のChargerバフです。</summary>
    Charger,
    /// <summary>ナイト用のSkirmisherバフです。</summary>
    Skirmisher,
    /// <summary>ポーン用のScoutバフです。</summary>
    Scout,
    /// <summary>ポーン用のSubstituteバフです。</summary>
    Substitute,
    /// <summary>実際のバフカード数を示す終端値です。</summary>
    AllBuffCount,
    /// <summary>バフカードが選択されていない状態です。</summary>
    None = -1
}

/// <summary>
/// ゲームで共有する駒、盤面、素材、画像、音声リソースを管理します。
/// StreamingAssets内のCSVからターン別の開始配置を読み込み、
/// 駒種とモデル・バフの対応表を初期化します。
/// また、駒色、プレイヤー画像、BGM、効果音を他の管理クラスへ提供します。
/// </summary>
public class ResourcesData : MonoBehaviour
{
    /// <summary>StreamingAssetsから指定キーのCSVをUTF-8で読み込みます。</summary>
    /// <param name="pathKey">拡張子を除いたCSVの相対パスです。</param>
    /// <param name="csvLine">読み込んで前後の空白を除去した各行です。</param>
    /// <returns>CSVを正常に読み込めた場合は <see langword="true" /> です。</returns>
    private bool LoadCSV(string pathKey, out string[] csvLine)
    {
        if (string.IsNullOrEmpty(pathKey))
        {
            Debug.LogError("CSV key null");
            csvLine = null;
            return false;
        }

        string path = Path.Combine(Application.streamingAssetsPath, pathKey + ".csv");

        // 指定されたCSVファイルが存在するか確認します。
        if (!File.Exists(path))
        {
            Debug.LogWarning("文件不存在: " + path);
            csvLine = null;
            return false;
        }

        // CSVをUTF-8としてすべて読み込みます。
        string[] lines = File.ReadAllLines(path, new UTF8Encoding(true));

        // 各行の前後に含まれる不要な空白を取り除きます。
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Trim();
        }

        csvLine = lines;
        return true;

    }

    #region ChessData
    [Header("Chess Model")]
    /// <summary>全駒種のPrefab、モデル、エフェクト、バフ情報です。</summary>
    public AllChessModel allChessModel;
    /// <summary>駒種と駒モデルデータの対応表を取得します。</summary>
    public Dictionary<ChessType, ChessData> chessModelDict => allChessModel.chessModelDict;
    /// <summary>指定バフカードを使用できる駒種のモデルデータを検索します。</summary>
    /// <param name="targetBuff">検索するバフカードです。</param>
    /// <returns>対応する駒モデルデータです。見つからない場合は <see langword="null" /> です。</returns>
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
    /// <summary>各駒種のバフ情報とモデル検索辞書を初期化します。</summary>
    private void ChessDataInit() => allChessModel.ChessDataInit();
    /// <summary>指定した駒色に対応するマテリアルを取得します。</summary>
    /// <param name="color">取得するマテリアルの駒色です。</param>
    /// <returns>白の場合は白素材、それ以外の場合は黒素材です。</returns>
    public Material TargetColor(ChessColor color)
    {
        if (color == ChessColor.White) return allMaterial.m_White;
        else return allMaterial.m_Black;
    }

    #endregion

    #region PlayerSprite
    [Header("All Sprite")]
    /// <summary>プレイヤー色と数値表示に使用する共通画像です。</summary>
    public AllSprite allSprite;
    /// <summary>指定した駒色に対応するプレイヤー画像を取得します。</summary>
    /// <param name="color">取得する画像のプレイヤー色です。</param>
    /// <returns>白または黒プレイヤーの画像です。対象外の場合は <see langword="null" /> です。</returns>
    public Sprite PlayerSprite(ChessColor color)
    {
        if (color == ChessColor.White) return allSprite.sp_WhiteColor;
        else if (color == ChessColor.Black) return allSprite.sp_BlackColor;

        return null;
    }

    #endregion

    #region BoardData

    #region Chess Start Map
    /// <summary>CSVファイル名と解析済み盤面開始データの対応表です。</summary>
    private Dictionary<string, BoardInitData> boardData = new();
    /// <summary>開始配置CSVの横方向のセル数です。</summary>
    private const int board_Width = 8;
    /// <summary>開始配置CSVの縦方向の行数です。</summary>
    private const int board_Height = 8;
    /// <summary>指定ファイル名の解析済み盤面開始配置を取得します。</summary>
    /// <param name="fileName">拡張子を含む開始配置CSVのファイル名です。</param>
    /// <returns>盤面座標と駒情報の対応表です。未登録の場合は既定値です。</returns>
    public Dictionary<Vector2Int, Pair<ChessColor, ChessType>> GetBcoardInitData(string fileName)
    {
        if (!boardData.ContainsKey(fileName))
        {
            Debug.LogError("No File Excite");
            return default;
        }
        else return boardData[fileName].boardStartMap;
    }
    /// <summary>ターン開始配置フォルダー内の全CSVファイルを検索して読み込みます。</summary>
    private void GetAllBoardDataFile()
    {
        string folderPath = Path.Combine(Application.streamingAssetsPath, "Csvs", "TurnStage");

        string[] csvFiles = Directory.GetFiles(folderPath, "*.csv");

        foreach (string filePath in csvFiles)
        {
            LoadBoardData(Path.GetFileName(filePath), filePath);
        }
    }
    /// <summary>8×8の開始配置CSVを解析し、座標と駒情報の対応表へ変換します。</summary>
    /// <param name="fileName">辞書へ登録するCSVのファイル名です。</param>
    /// <param name="filePath">読み込むCSVの完全パスです。</param>
    private void LoadBoardData(string fileName, string filePath)
    {
        BoardInitData data = new BoardInitData();
        data.boardStartData = File.ReadAllLines(filePath);
        data.boardStartMap = new();

        // 各セルの「駒色:駒種」を解析し、空白セルを除いて配置辞書へ登録します。
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
    /// <summary>盤面、駒、状態エフェクトで共有するすべてのマテリアルです。</summary>
    public AllMaterial allMaterial;
    #endregion
    /// <summary>
    /// StreamingAssetsからすべてのターン開始配置CSVを読み込みます。
    /// </summary>
    private void BoardDataInit()
    {
        GetAllBoardDataFile();
    }


    #endregion

    #region Adudio Data
    [Header("Bgm Data")]
    /// <summary>対局中に再生するBGMデータです。</summary>
    public BgmData bgm_game;

    [Header("Sfx Data")]
    /// <summary>ゲーム内で使用するすべての効果音データです。</summary>
    public AllSfxData allSfxData;
    /// <summary>ボタン押下時の効果音を取得します。</summary>
    public SfxData sfx_PressButton => allSfxData.sfx_PressButton;
    /// <summary>ゲーム開始時の効果音を取得します。</summary>
    public SfxData sfx_GameStart => allSfxData.sfx_GameStart;
    /// <summary>駒選択時の効果音を取得します。</summary>
    public SfxData sfx_PickChess => allSfxData.sfx_PickChess;
    /// <summary>駒配置時の効果音を取得します。</summary>
    public SfxData sfx_PutChess => allSfxData.sfx_PutChess;
    /// <summary>手番交代時の効果音を取得します。</summary>
    public SfxData sfx_TurnChange => allSfxData.sfx_TurnChange;
    /// <summary>リザルト表示時の効果音を取得します。</summary>
    public SfxData sfx_Release => allSfxData.sfx_Release;
    /// <summary>言語変更時の効果音を取得します。</summary>
    public SfxData sfx_ChangeLanguage => allSfxData.sfx_ChangeLanguage;
    /// <summary>駒生成時の効果音を取得します。</summary>
    public SfxData sfx_ChessSwapn => allSfxData.sfx_ChessSwapn;

    #endregion
    
    [Header("Controller Mark Data")]
    public ControllerMarkData ps_ControllerMark;
    public ControllerMarkData xbox_ControllerMark;
    public ControllerMarkData switch_ControllerMark;
    
    
    
    
    /// <summary>
    /// 盤面開始配置と駒モデル・バフ対応データを初期化します。
    /// </summary>

    public void ResourcesInit()
    {
        BoardDataInit();
        ChessDataInit();
    }
}
