using Data;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

/// <summary>ゲームで表示できる言語を表します。</summary>
public enum Language 
{
    /// <summary>日本語表示です。</summary>
    Japanese,
    /// <summary>英語表示です。</summary>
    English
}

/// <summary>
/// ゲーム内の表示言語と、言語別のテキスト・UI画像を管理します。
/// 現在選択中の言語に対応する <see cref="LanguageData" /> を提供し、
/// CSVからバフカード名とレベル別説明文を読み込んでカードデータへ反映します。
/// 各画面はこのクラスのプロパティを通して現在言語のスプライトを取得できます。
/// </summary>
public class LanguageManager : MonoBehaviour
{
    /// <summary>言語管理オブジェクトの共有インスタンスです。</summary>
    public static LanguageManager Instance;
    /// <summary>現在ゲームで使用している表示言語です。</summary>
    public Language nowUsingLanguage = Language.Japanese;
    [Header("Language Data")]
    /// <summary>日本語のテキストとUI画像を保持するデータです。</summary>
    public LanguageData japanese;
    /// <summary>英語のテキストとUI画像を保持するデータです。</summary>
    public LanguageData english;
    /// <summary>現在選択中の言語に対応する言語データを取得します。</summary>
    /// <returns>日本語または英語の言語データです。不明な言語の場合は <see langword="null" /> です。</returns>
    public LanguageData NowUsingLanguageData()
    {
        if (nowUsingLanguage == Language.English) return english;
        else if (nowUsingLanguage == Language.Japanese) return japanese;
        Debug.LogError("How???");
        return null;
    }

    #region Crad Data
    [Header("Crad Data")]
    /// <summary>全バフカードの画像、素材、表示テキストを保持するデータです。</summary>
    public AllCradData cradDataList;
    /// <summary>バフカード種別とカード表示データの対応表を取得します。</summary>
    public Dictionary<AllBuffCard, CardData> cardDataDict => cradDataList.cardDataDict;
    /// <summary>バフカード種別とカード表示データの対応表を初期化します。</summary>
    public void CardDataDictInit() => cradDataList.CardDataDictInit();
    /// <summary>カードCSVの1行に必要なデータ列数です。</summary>
    private const int dataIndexMax = 4;
    /// <summary>現在言語のCSVからカード名と3段階のバフ説明文を読み込みます。</summary>
    private void CardDataUpdate()
    {
        string[] languageFile = NowUsingLanguageData().csvFile.text.Split('\n');

        if (languageFile == null || languageFile.Length < 1)
        {
            Debug.LogError("CSV is Null");
            return;
        }

        // 定義されている各バフカードについて、対応するCSV行を解析します。
        foreach (AllBuffCard buffCardName in Enum.GetValues(typeof(AllBuffCard)))
        {
            
            if (buffCardName == AllBuffCard.None
                || buffCardName == AllBuffCard.AllBuffCount) continue;

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
            // CSVの各列を、カード名とレベル別説明文へ反映します。
            if (values[0].Trim() != null) cardDataDict[buffCardName].name = values[0].Trim();
            if (values[1].Trim() != null) cardDataDict[buffCardName].buffLevel01Description = values[1].Trim();
            if (values[2].Trim() != null) cardDataDict[buffCardName].buffLevel02Description = values[2].Trim();
            if (values[3].Trim() != null) cardDataDict[buffCardName].buffLevel03Description = values[3].Trim();
        }
    }

    #endregion

    #region GameImage
    /// <summary>現在言語のゲームタイトル画像を取得します。</summary>
    public Sprite sp_GameTitle => NowUsingLanguageData().sp_GameTitle;
    /// <summary>現在言語のゲーム開始ボタン画像を取得します。</summary>
    public Sprite sp_GameStart => NowUsingLanguageData().sp_GameStart;
    /// <summary>現在言語のゲーム説明ボタン画像を取得します。</summary>
    public Sprite sp_Description => NowUsingLanguageData().sp_Description;
    /// <summary>現在言語の戻るボタン画像を取得します。</summary>
    public Sprite sp_Button_Return => NowUsingLanguageData().sp_Button_Return;
    /// <summary>現在言語の決定ボタン画像を取得します。</summary>
    public Sprite sp_Button_Confirm => NowUsingLanguageData().sp_Button_Confirm;
    /// <summary>現在言語のルール見出し画像を取得します。</summary>
    public Sprite sp_Rules => NowUsingLanguageData().sp_Rules;
    /// <summary>現在言語のルール説明画像を取得します。</summary>
    public Sprite sp_Rules_Intro => NowUsingLanguageData().sp_Rules_Intro;
    /// <summary>現在言語の操作方法見出し画像を取得します。</summary>
    public Sprite sp_Control => NowUsingLanguageData().sp_Control;
    /// <summary>現在言語の操作方法説明画像を取得します。</summary>
    public Sprite sp_Control_Intro => NowUsingLanguageData().sp_Control_Intro;
    /// <summary>現在言語の駒と盤面の見出し画像を取得します。</summary>
    public Sprite sp_ChessAndBoard => NowUsingLanguageData().sp_ChessAndBoard;
    /// <summary>現在言語の駒と盤面の説明画像を取得します。</summary>
    public Sprite sp_ChessAndBoard_Intro => NowUsingLanguageData().sp_ChessAndBoard_Intro;
    /// <summary>現在言語のバフ説明画像を取得します。</summary>
    public Sprite sp_Buffs => NowUsingLanguageData().sp_Buffs;
    /// <summary>現在言語のスキル選択ロゴ画像を取得します。</summary>
    public Sprite sp_ChooseSkills_Logo => NowUsingLanguageData().sp_ChooseSkills_Logo;
    /// <summary>現在言語の再抽選可能ボタン画像を取得します。</summary>
    public Sprite sp_Button_CanDrawAgain => NowUsingLanguageData().sp_Button_CanDrawAgain;
    /// <summary>現在言語の再抽選不可ボタン画像を取得します。</summary>
    public Sprite sp_Button_CannotDrawAgain => NowUsingLanguageData().sp_Button_CannotDrawAgain;
    /// <summary>現在言語の再抽選不可ボタン画像を取得します。</summary>
    public Sprite sp_ActionMark => NowUsingLanguageData().sp_ActionMark;
    /// <summary>現在言語のポーズボタン画像を取得します。</summary>
    public Sprite sp_Button_Pause => NowUsingLanguageData().sp_Button_Pause;
    /// <summary>現在言語の投了ボタン画像を取得します。</summary>
    public Sprite sp_Button_Surrender => NowUsingLanguageData().sp_Button_Surrender;
    /// <summary>現在言語の投了確認画像を取得します。</summary>
    public Sprite sp_Confirm_Surrender => NowUsingLanguageData().sp_Confirm_Surrender;
    /// <summary>現在言語のタイトル復帰確認画像を取得します。</summary>
    public Sprite sp_Confirm_BackToGameTitle => NowUsingLanguageData().sp_Confirm_BackToGameTitle;
    /// <summary>現在言語の勝者表示画像を取得します。</summary>
    public Sprite sp_Release_Winner => NowUsingLanguageData().sp_Release_Winner;
    /// <summary>現在言語の再開ボタン画像を取得します。</summary>
    public Sprite sp_Button_Resume => NowUsingLanguageData().sp_Button_Resume;
    /// <summary>現在言語の終了ボタン画像を取得します。</summary>
    public Sprite sp_Button_Quit => NowUsingLanguageData().sp_Button_Quit;

    #endregion

    /// <summary>表示言語を変更し、カード名と説明文を選択言語へ更新します。</summary>
    /// <param name="language">新しく使用する表示言語です。</param>
    public void ChangeLanguage(Language language)
    {
        nowUsingLanguage = language;
        CardDataUpdate();
    }
    /// <summary>
    /// カードデータ辞書を構築し、現在設定されている言語を適用します。
    /// </summary>
    public void Init()
    {
        CardDataDictInit();
        ChangeLanguage(nowUsingLanguage);
    }



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
    }
}
