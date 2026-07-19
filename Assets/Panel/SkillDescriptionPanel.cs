using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SkillDescriptionPanel : MonoBehaviour
{
    private LanguageManager _languageManager => GameManager.Instance.languageManager;
    public Image cardImage;
    public TMP_Text cardName;
    public TMP_Text[] cardBuffLevelDescriptions;
    public Image[] nowBuffMark;

    [Header("Button Image")]
    public Image image_confirm;
    public Image image_return;

    private void ChangeButtonImage()
    {
        if (image_confirm != null&& image_confirm.sprite != _languageManager.sp_Button_Confirm)
            image_confirm.sprite = _languageManager.sp_Button_Confirm;

        if (image_return != null && image_return.sprite != _languageManager.sp_Button_Return)
            image_return.sprite = _languageManager.sp_Button_Return;
            
    }

    public void ChangeDescription(AllBuffCard buffCard, uint nowLevel)
    {
        ChangeButtonImage();

        Data.CardData cardData = _languageManager.cardDataDict[buffCard];

        cardImage.sprite = cardData.sp_CardCover;
        cardName.text = cardData.name;

        cardBuffLevelDescriptions[0].text = cardData.buffLevel01Description;
        cardBuffLevelDescriptions[1].text = cardData.buffLevel02Description;
        cardBuffLevelDescriptions[2].text = cardData.buffLevel03Description;

        for (int i = 0; i < nowBuffMark.Length; i++)
        {
            nowBuffMark[i].enabled = nowLevel >= i + 1;
        }

    }

    public void Button_Return()
    {
        GameManager.Instance.PlayButtonSfx();
        gameObject.SetActive(false);
    }
}
