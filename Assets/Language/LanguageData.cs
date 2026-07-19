using UnityEngine;

[CreateAssetMenu(fileName = "LanguageData", menuName = "GameLanguage/LanguageData")]
public class LanguageData : ScriptableObject
{
    public TextAsset csvFile;
    [Header("GameStage")]
    public Sprite sp_GameTitle;
    public Sprite sp_GameStart;
    public Sprite sp_Description;

    [Header("Common")]
    public Sprite sp_Button_Return;
    public Sprite sp_Button_Confirm;

    [Header("Description")]
    public Sprite sp_Rules;
    public Sprite sp_Rules_Intro;

    public Sprite sp_Control;
    public Sprite sp_Control_Intro;

    public Sprite sp_ChessAndBoard;
    public Sprite sp_ChessAndBoard_Intro;

    public Sprite sp_Buffs;

    [Header("Choose Skills")]
    public Sprite sp_ChooseSkills_Logo;
    public Sprite sp_Button_CanDrawAgain;
    public Sprite sp_Button_CannotDrawAgain;

    [Header("InGame")]
    public Sprite sp_ActionMark;

    public Sprite sp_Button_Pause;
    public Sprite sp_Button_Surrender;
    public Sprite sp_Confirm_Surrender;
    public Sprite sp_Confirm_BackToGameTitle;

    [Header("Release")]
    public Sprite sp_Release_Winner;

    public Sprite sp_Button_Resume;
    public Sprite sp_Button_Quit;




}
