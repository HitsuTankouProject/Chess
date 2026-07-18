using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class PlayerCanvas : MonoBehaviour
{

    private ResourcesData _resourcesData => GameManager.Instance.resourcesData;
    public GameObject pausePanel;
    private Player _player;
    private bool isPlayerUseGamePad => _player.playerInPut.nowUsingDevice == CanUseDevice.Gamepad;
    public Camera playerCamera;

    public bool isPause { get; private set; }

    public void Init(Player player, List<AllBuffCard> choseBuffs)
    {
        _player = player;
        PauseInit(choseBuffs);
    }

    #region Button
    public void Button_Pause()
    {
        if (GameManager.Instance.nowGameStage != GameStage.InGame || isConfirming) return;
        isPause = !isPause;
        pausePanel.SetActive(isPause);
        if (isPlayerUseGamePad)
        {
            pickCard.gameObject.SetActive(isPause);
            pickCardIndex = 0;
            pickCard.transform.localPosition = cardsPos[pickCardIndex];

        }
        else pickCard.enabled = false;

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
        if (GameManager.Instance.nowGameStage != GameStage.InGame || isConfirming) return;
        OpenConfirmPanel(ConfirmStage.Surrender);
    }
    public void Button_BackToGameTitle()
    {
        if (GameManager.Instance.nowGameStage != GameStage.InGame || isConfirming) return;
        OpenConfirmPanel(ConfirmStage.BackToGameTitle);
    }

    public void Button_Return()
    {
        confirmStage = ConfirmStage.None;
        confirmPanel.SetActive(false);
    }
    public void Button_Confirm()
    {
        switch (confirmStage)
        {
            case ConfirmStage.Surrender: GameManager.Instance.Surrender(_player.usingChess); break;
            case ConfirmStage.BackToGameTitle: GameManager.Instance.Button_BackToGameTitle(); break;
            default: return;
        }
        Button_Return();
    }

    #endregion

    #region Pause
    [Header("Pause")]
    public Image pickCard;
    private int pickCardIndex = 0;
    private int maxCanPick => _player.choseBuffs.Count - 1;


    public Button[] cards;
    private List<Vector3> cardsPos = new();
    public List<Action> cardActions = new();

    private void PauseInit(List<AllBuffCard> choseBuffs)
    {

        if (choseBuffs.Count > 3)
        {
            Debug.LogError("Pick over Then 3 Buff");
            return;
        }
        cardActions.Clear();
        cardsPos.Clear();

        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].onClick.RemoveAllListeners();

            if (i < choseBuffs.Count)
            {
                AllBuffCard targetBuff = choseBuffs[i];
                cards[i].image.sprite = GameManager.Instance.resourcesData.cardDataDict[targetBuff].sp_CardCover;
                cards[i].onClick.AddListener(() => Button_OpenSkillDescriptionPanel(targetBuff));
                cardsPos.Add(cards[i].transform.localPosition);
                cardActions.Add(() => Button_OpenSkillDescriptionPanel(targetBuff));
            }
            else
                cards[i].image.sprite = GameManager.Instance.resourcesData.cradDataList.sp_CardBack;
        }
    }

    public void WatchBuffSkillDescription()
    {
        if (!isPlayerUseGamePad) return;
        cardActions[pickCardIndex]();
    }

    public void NextCard()
    {
        if (!isPlayerUseGamePad) return;
        pickCardIndex = Mathf.Min(pickCardIndex + 1, maxCanPick);
        pickCard.transform.localPosition = cardsPos[pickCardIndex];
    }
    public void BackCard()
    {
        if (!isPlayerUseGamePad) return;
        pickCardIndex = Mathf.Max(pickCardIndex - 1, 0);
        pickCard.transform.localPosition = cardsPos[pickCardIndex];
    }


    #endregion

    #region Confirm
    [Header("Confirm Panel")]
    public GameObject confirmPanel;
    public Image confirmImage;
    private bool isConfirming => confirmPanel.activeSelf;
    private enum ConfirmStage {None, Surrender, BackToGameTitle}
    private ConfirmStage confirmStage;
    private Sprite Sp_Confirm()
    {
        switch (confirmStage)
        {
            case ConfirmStage.None: return null;
            case ConfirmStage.Surrender: return _resourcesData.allSprite.sp_Confirm_Surrender;
            case ConfirmStage.BackToGameTitle: return _resourcesData.allSprite.sp_Confirm_Surrender;
            default: return null;
        }
    }

    private void OpenConfirmPanel(ConfirmStage stage)
    {
        confirmStage = stage;
        confirmImage.sprite = Sp_Confirm();
        confirmPanel.SetActive(true);
    }

    


    #endregion

    #region SkillDescriptionPanel
    [Header("Skill Description Panel ")]
    public SkillDescriptionPanel skillDescriptionPanel;
    #endregion
}
