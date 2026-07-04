using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCanvas : MonoBehaviour
{
    public GameObject pausePanel;

    private Player _player;
    public Camera playerCamera;

    public bool isPause {  get; private set; }

    public void Init(Player player, List<AllBuffCard> choseBuffs)
    {
        _player = player;
        PauseInit(choseBuffs);
    }
    public void Button_Pause()
    {
        isPause = !isPause;
        pausePanel.SetActive(isPause); 
    }



    #region SkillDescriptionPanel
    public SkillDescriptionPanel skillDescriptionPanel;
    #endregion

    #region Button
    private void Button_OpenSkillDescriptionPanel(AllBuffCard targetBuff)
    {
        uint nowLevel = _player.cardBuffMap[targetBuff].nowBuffLevel;
        skillDescriptionPanel.ChangeDescription(targetBuff, nowLevel);
        skillDescriptionPanel.gameObject.SetActive(true);
    }

    public void Button_CloseSkillDescriptionPanel()
        => skillDescriptionPanel.gameObject.SetActive(false);
    #endregion

    #region Pause
    [Header("Pause ")]
    public Button[] cards;
    private void PauseInit(List<AllBuffCard> choseBuffs)
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

    #endregion
}
