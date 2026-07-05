using UnityEngine;

[CreateAssetMenu(fileName = "LanguageData", menuName = "GameImage/LanguageData")]
public class LanguageData : ScriptableObject
{
    public Language language;

    [Header("Button Sprites")]
    public Sprite sp_Button_GameTitle;
    public Sprite sp_Button_GameStart;
    public Sprite sp_Button_Description;
    public Sprite sp_Button_CanDrawAgain;
    public Sprite sp_Button_CannotDrawAgain;
    public Sprite sp_Button_Return;
    public Sprite sp_Button_Confirm;
    public Sprite sp_Button_Pause;
    public Sprite sp_Button_PauseReturn;
    public Sprite sp_Button_Yes;
    public Sprite sp_Button_No;
    public Sprite sp_Button_Surrender;
    public Sprite sp_Button_Restart;
    public Sprite sp_Button_Quit;


}
