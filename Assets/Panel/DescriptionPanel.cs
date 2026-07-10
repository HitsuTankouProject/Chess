using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DescriptionPanel : MonoBehaviour
{
    private bool isInit = false;

    public void Init()
    {
        Button_OpenRulesDescription();
        if (isInit)return;
        AllBuffInit();
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
        rules_Description.SetActive(true);
    }



    [Header("Input Description")]
    public GameObject input_Description;
    public void Button_OpenInputDescription()
    {
        CloseAllTheObjectDescription();
        input_Description.SetActive(true);
    }



    [Header("ChessAndBoard Description")]
    public GameObject chessAndBoard_Description;
    public void Button_OpenChessAndBoardDescription()
    {
        CloseAllTheObjectDescription();
        chessAndBoard_Description.SetActive(true);
    }


    [Header("Buff Description")]
    public GameObject buff_Description;
    public SkillDescriptionPanel skillDescriptionPanel;
    public Button[] buffs;

    private void AllBuffInit()
    {


        foreach (AllBuffCard buffCardName in Enum.GetValues(typeof(AllBuffCard)))
        {
            if (buffCardName == AllBuffCard.None) continue;
            int index = (int)buffCardName;
            buffs[index].onClick.RemoveAllListeners();
            buffs[index].onClick.AddListener(() => Button_OpenSkillDescriptionPanel(buffCardName));

        }

    }


    public void Button_OpenBuffDescription()
    {
        CloseAllTheObjectDescription();
        Button_Return();
    }

    public void Button_OpenSkillDescriptionPanel(AllBuffCard targetBuff)
    {
        buff_Description.gameObject.SetActive(false);
        skillDescriptionPanel.ChangeDescription(targetBuff, 0);
        skillDescriptionPanel.gameObject.SetActive(true);
    }
    public void Button_Return()
    {
        skillDescriptionPanel.gameObject.SetActive(false);
        buff_Description.gameObject.SetActive(true);

    }

}
