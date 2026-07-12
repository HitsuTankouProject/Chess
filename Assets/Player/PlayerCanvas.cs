using System;
using System.Collections.Generic;
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

    #region Button
    public void Button_Pause()
    {
        if (GameManager.Instance.nowGameStage != GameStage.InGame) return;
        isPause = !isPause;
        pausePanel.SetActive(isPause);
    }

    public void Button_OpenSkillDescriptionPanel(AllBuffCard targetBuff)
    {
        uint nowLevel = _player.cardBuffMap[targetBuff].nowBuffLevel;
        Debug.Log(nowLevel);
        skillDescriptionPanel.ChangeDescription(targetBuff, nowLevel);
        skillDescriptionPanel.gameObject.SetActive(true);
    }

    public void Button_CloseSkillDescriptionPanel()
        => skillDescriptionPanel.gameObject.SetActive(false);
    public void Button_Surrender()
    {
        GameManager.Instance.Surrender(_player.usingChess);
    }
    public void Button_BackToGameTitle()
    {
        GameManager.Instance.Button_BackToGameTitle();
    }


    #endregion

    #region Pause
    [Header("Pause")]
    public Button[] cards;
    public List<Action> cardActions;
    private void PauseInit(List<AllBuffCard> choseBuffs)
    {

        if (choseBuffs.Count > 3)
        {
            Debug.LogError("Pick over Then 3 Buff");
            return;
        }
        cardActions.Clear();

        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].onClick.RemoveAllListeners();

            if (i < choseBuffs.Count)
            {
                AllBuffCard targetBuff = choseBuffs[i];
                cards[i].image.sprite = GameManager.Instance.resourcesData.cardDataDict[targetBuff].sp_CardCover;
                cards[i].onClick.AddListener(() => Button_OpenSkillDescriptionPanel(targetBuff));
                cardActions.Add(() => Button_OpenSkillDescriptionPanel(targetBuff));
            }
            else
                cards[i].image.sprite = GameManager.Instance.resourcesData.cradDataList.sp_CardBack;
        }
    }

    #endregion

    #region SkillDescriptionPanel
    [Header("Skill Description Panel ")]
    public SkillDescriptionPanel skillDescriptionPanel;
    #endregion
}
