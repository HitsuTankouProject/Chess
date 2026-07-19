using Data;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public enum Language { Japanese, English }

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;
    public Language nowUsingLanguage = Language.Japanese;
    [Header("Language Data")]
    public LanguageData japanese;
    public LanguageData english;
    public LanguageData NowUsingLanguageData()
    {
        if (nowUsingLanguage == Language.English) return english;
        else if (nowUsingLanguage == Language.Japanese) return japanese;
        Debug.LogError("How???");
        return null;
    }

    #region Crad Data
    [Header("Crad Data")]
    public AllCradData cradDataList;
    public Dictionary<AllBuffCard, CardData> cardDataDict => cradDataList.cardDataDict;
    public void CardDataDictInit() => cradDataList.CardDataDictInit();
    private const int dataIndexMax = 4;

    private void CardDataUpdate()
    {
        string[] languageFile = NowUsingLanguageData().csvFile.text.Split('\n');

        if (languageFile == null || languageFile.Length < 1)
        {
            Debug.LogError("CSV is Null");
            return;
        }

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
            if (values[0].Trim() != null) cardDataDict[buffCardName].name = values[0].Trim();
            if (values[1].Trim() != null) cardDataDict[buffCardName].buffLevel01Description = values[1].Trim();
            if (values[2].Trim() != null) cardDataDict[buffCardName].buffLevel02Description = values[2].Trim();
            if (values[3].Trim() != null) cardDataDict[buffCardName].buffLevel03Description = values[3].Trim();
        }
    }

    #endregion

    #region GameImage
    public Sprite sp_GameTitle => NowUsingLanguageData().sp_GameTitle;
    public Sprite sp_GameStart => NowUsingLanguageData().sp_GameStart;
    public Sprite sp_Description => NowUsingLanguageData().sp_Description;

    public Sprite sp_Button_Return => NowUsingLanguageData().sp_Button_Return;
    public Sprite sp_Button_Confirm => NowUsingLanguageData().sp_Button_Confirm;

    public Sprite sp_Rules => NowUsingLanguageData().sp_Rules;
    public Sprite sp_Rules_Intro => NowUsingLanguageData().sp_Rules_Intro;

    public Sprite sp_Control => NowUsingLanguageData().sp_Control;
    public Sprite sp_Control_Intro => NowUsingLanguageData().sp_Control_Intro;


    public Sprite sp_ChessAndBoard => NowUsingLanguageData().sp_ChessAndBoard;
    public Sprite sp_ChessAndBoard_Intro => NowUsingLanguageData().sp_ChessAndBoard_Intro;

    public Sprite sp_Buffs => NowUsingLanguageData().sp_Buffs;



    public Sprite sp_ChooseSkills_Logo => NowUsingLanguageData().sp_ChooseSkills_Logo;
    public Sprite sp_Button_CanDrawAgain => NowUsingLanguageData().sp_Button_CanDrawAgain;
    public Sprite sp_Button_CannotDrawAgain => NowUsingLanguageData().sp_Button_CannotDrawAgain;

    public Sprite sp_ActionMark => NowUsingLanguageData().sp_ActionMark;

    public Sprite sp_Button_Pause => NowUsingLanguageData().sp_Button_Pause;
    public Sprite sp_Button_Surrender => NowUsingLanguageData().sp_Button_Surrender;
    public Sprite sp_Confirm_Surrender => NowUsingLanguageData().sp_Confirm_Surrender;
    public Sprite sp_Confirm_BackToGameTitle => NowUsingLanguageData().sp_Confirm_BackToGameTitle;

    public Sprite sp_Release_Winner => NowUsingLanguageData().sp_Release_Winner;
    public Sprite sp_Button_Resume => NowUsingLanguageData().sp_Button_Resume;
    public Sprite sp_Button_Quit => NowUsingLanguageData().sp_Button_Quit;







    #endregion




    public void ChangeLanguage(Language language)
    {
        nowUsingLanguage = language;
        CardDataUpdate();
    }

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
