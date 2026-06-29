using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PausePanel : MonoBehaviour
{

    public SkillDescriptionPanel skillDescriptionPanel;
    private Player _player;
    public Button[] cards;

    public void Init(Player player, List<AllBuffCard> choseBuffs)
    {
        if (choseBuffs.Count > 3)
        {
            Debug.LogError("Pick over Then 3 Buff");
            return;
        }

        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].onClick.RemoveAllListeners();
            cards[i].gameObject.SetActive(false);
            if (i < choseBuffs.Count)
            {
                cards[i].gameObject.SetActive(true);
                cards[i].onClick.AddListener(() => Button_OpenSkillDescriptionPanel(choseBuffs[i]));
            }
        }
        gameObject.SetActive(false);
    }

    private void Button_OpenSkillDescriptionPanel(AllBuffCard targetBuff)
    {
        uint nowLevel = _player.cardBuffMap[targetBuff].nowBuffLevel;
        skillDescriptionPanel.ChangeDescription(targetBuff, nowLevel);
        skillDescriptionPanel.gameObject.SetActive(true);
    }

    public void Button_BackToTitle()
    {

    }

    public void Button_GoToRelease()
    {

    }
}
