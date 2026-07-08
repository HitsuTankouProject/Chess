using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SkillDescriptionPanel : MonoBehaviour
{
    private ResourcesData _resourcesData => GameManager.Instance.resourcesData;
    public Image cardImage;
    public TMP_Text cardName;
    public TMP_Text[] cardBuffLevelDescriptions;
    public Image[] nowBuffMark;

    public void ChangeDescription(AllBuffCard buffCard, uint nowLevel)
    {
        Data.CardData cardData = _resourcesData.cardDataDict[buffCard];
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

    public void Button_Return() => gameObject.SetActive(false);
}
