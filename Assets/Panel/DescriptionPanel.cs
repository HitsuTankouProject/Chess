using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class DescriptionPanel : MonoBehaviour
{
    private GameManager _gameManager => GameManager.Instance;
    private bool isInit = false;

    private int minPage = 0;
    private int nowPage = 0;
    private int maxPage = 3;
    private Action[] buttonActions = new Action[4];
    private void ButtonActionsInit()
    {
        buttonActions[0] = Button_OpenRulesDescription;
        buttonActions[1] = Button_OpenInputDescription;
        buttonActions[2] = Button_OpenChessAndBoardDescription;
        buttonActions[3] = Button_OpenBuffDescription;
    }
    public void Init()
    {
        Button_OpenRulesDescription();
        nowPage = 1;
        WaitGamePadInput_GameDescription().Forget();
        if (isInit)return;
        isInit = true;
        AllBuffInit();
        ButtonActionsInit();
    }

    private async UniTask WaitGamePadInput_GameDescription()
    {
        while (_gameManager.nowGameStage == GameStage.GameDescription)
        {
            ButtonControl button = await _gameManager.inPutManager.WaitForGamePadButtonInput();
            await UniTask.Yield();
            if (button == null) continue;

            switch (button.name)
            {
                case "buttonWest":      _gameManager.Button_BackToGameTitle();  return;
                case "buttonNorth":     _gameManager.Button_BackToGameStart();  return;

                case "rightShoulder":   NextPage();                             break;
                case "leftShoulder":    BackPage();                             break;

                case "up":              SwitchPick(-1);                         break;
                case "down":            SwitchPick(1);                          break;
                case "left":            SwitchPick(-2);                         break;
                case "right":           SwitchPick(2);                          break;

                case "buttonEast":      Button_Return();                        break;
                case "buttonSouth":     cardButton[pickIndex]();                break;

                default:                await UniTask.Yield();                  break;
            }
        }
    }

    private void NextPage()
    {
        nowPage = Mathf.Min(nowPage + 1, maxPage);
        Debug.Log(nowPage);
        buttonActions[nowPage]();
    }

    private void BackPage()
    {
        nowPage = Mathf.Max(nowPage - 1, minPage);
        Debug.Log(nowPage);
        buttonActions[nowPage]();
    }


    private void CloseAllTheObjectDescription()
    {
        rules_Description.SetActive(false);
        input_Description.SetActive(false);
        chessAndBoard_Description.SetActive(false);
        buff_Description.SetActive(false );
    }

    [Header("Rules Description")]
    public GameObject rules_Description;
    public void Button_OpenRulesDescription()
    {
        CloseAllTheObjectDescription();
        nowPage = 0;
        rules_Description.SetActive(true);
    }



    [Header("Input Description")]
    public GameObject input_Description;
    public void Button_OpenInputDescription()
    {
        CloseAllTheObjectDescription();
        nowPage = 1;
        input_Description.SetActive(true);
    }



    [Header("ChessAndBoard Description")]
    public GameObject chessAndBoard_Description;
    public void Button_OpenChessAndBoardDescription()
    {
        CloseAllTheObjectDescription();
        nowPage = 2;
        chessAndBoard_Description.SetActive(true);
    }


    [Header("Buff Description")]
    public GameObject buff_Description;
    public SkillDescriptionPanel skillDescriptionPanel;
    public Button[] buffs;

    public Image pickImage;
    private int pickIndex = 0;
    private Vector3[] cardPositions;
    Dictionary<int, Action> cardButton = new();

    private void AllBuffInit()
    {
        cardPositions = new Vector3[buffs.Length];

        foreach (AllBuffCard buffCardName in Enum.GetValues(typeof(AllBuffCard)))
        {
            if (buffCardName == AllBuffCard.None) continue;
            int index = (int)buffCardName;
            buffs[index].onClick.RemoveAllListeners();
            buffs[index].onClick.AddListener(() => Button_OpenSkillDescriptionPanel(buffCardName));
            cardButton[index] = () => Button_OpenSkillDescriptionPanel(buffCardName);
            cardPositions[index] = buffs[index].gameObject.transform.localPosition;
        }
    }

    private void SwitchPick(int value)
    {
        if (!buff_Description.activeSelf) return;
        pickIndex += value;
        if (pickIndex < 0) pickIndex = 0;
        if (pickIndex > buffs.Length) pickIndex = buffs.Length;

        pickImage.gameObject.transform.localPosition = cardPositions[pickIndex];

    }



    public void Button_OpenBuffDescription()
    {
        CloseAllTheObjectDescription();
        nowPage = 3;
        pickIndex = 0;
        pickImage.transform.localPosition = cardPositions[pickIndex];
        buff_Description.gameObject.SetActive(true);

    }

    public void Button_OpenSkillDescriptionPanel(AllBuffCard targetBuff)
    {
        if (!buff_Description.activeSelf) return;
        buff_Description.gameObject.SetActive(false);
        skillDescriptionPanel.ChangeDescription(targetBuff, 0);
        skillDescriptionPanel.gameObject.SetActive(true);
    }
    public void Button_Return()
    {
        if (!skillDescriptionPanel.gameObject.activeSelf) return ;

        skillDescriptionPanel.gameObject.SetActive(false);
        buff_Description.gameObject.SetActive(true);

    }

}
