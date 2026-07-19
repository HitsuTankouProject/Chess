using Cysharp.Threading.Tasks;
using Data;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChooseSkillPanel : MonoBehaviour
{
    private GameManager _gameManager => GameManager.Instance;
    private ResourcesData _resourcesData => _gameManager.resourcesData;
    private LanguageManager _languageManager => _gameManager.languageManager;
    private InPutManager _inPutManager => _gameManager.inPutManager;

    public ChessColor chooseSkillPlayerColor { get; private set; } = ChessColor.White;
    private Player chooseSkillPlayer => chooseSkillPlayerColor == ChessColor.White ?
        _gameManager.player01 : _gameManager.player02;

    private bool isWhiteChessPlayerPick = false;
    private bool isBlackChessPlayerPick = false;
    private void OffPlayerPick(ChessColor color)
    {
        if (color == ChessColor.White) isWhiteChessPlayerPick = true;
        else if (color == ChessColor.Black) isBlackChessPlayerPick = true;
        _inPutManager.PlayerInputStage(chooseSkillPlayerColor, InputStage.None);

    }

    [Header("Pick Cards")]
    public Card[] canPickCard;
    public Button[] pickCardButton;
    public List<AllBuffCard> pickedThreeCard { get; private set; } = new();
    private readonly Dictionary<ChessType, AllBuffCard[]> buffChessDict = new()
    {
        [ChessType.King] = new[] { AllBuffCard.MadKing, AllBuffCard.SageKing },
        [ChessType.Queen] = new[] { AllBuffCard.Witcher, AllBuffCard.Beauty },
        [ChessType.Queen] = new[] { AllBuffCard.Witcher, AllBuffCard.Beauty },
        [ChessType.Bishop] = new[] { AllBuffCard.Sorcerer, AllBuffCard.Monk },
        [ChessType.Knight] = new[] { AllBuffCard.Charger, AllBuffCard.Skirmisher },
        [ChessType.Rook] = new[] { AllBuffCard.Rusher, AllBuffCard.Guardian },
        [ChessType.Pawn] = new[] { AllBuffCard.Scout, AllBuffCard.Substitute }
    };

    [Header("Picking Card")]
    public SkillDescriptionPanel skillDescriptionPanel;
    public GameObject showCanPickPanel;

    [Header("Picking Tag")]
    public Image playerTag;
    private AllBuffCard picking;
    public bool isPicking => picking != AllBuffCard.None;
    private List<AllBuffCard> pickedCards = new();

    [Header("Picking Mark")]
    public Image pickingMark;
    private int pickingIndex = 0;
    private List<Action> cardPikButton = new();
    private const float posZ = -240.0f;
    private List<Vector3> cardPosition = new();

    private void PickMarkInit()
    {
        cardPosition.Clear();
        for (int i = 0; i < canPickCard.Length; i++)
        {
            Vector3 target = canPickCard[i].transform.localPosition;
            cardPosition.Add(new Vector3(target.x, target.y, posZ));
        }

    }
    public void PickNextCard()
    {
        pickingIndex = Mathf.Min(pickingIndex + 1, canPickCard.Length - 1);
        pickingMark.transform.localPosition = cardPosition[pickingIndex];
    }

    public void PickBackCard()
    {
        pickingIndex = Mathf.Max(pickingIndex - 1, 0);
        pickingMark.transform.localPosition = cardPosition[pickingIndex];
    }

    public void PickThatCard() => cardPikButton[pickingIndex]();

    [Header("DrawAgain")]
    private bool canDrawAgain = true;
    public Button drawAgain;

    private Image image_drawAgain=> drawAgain.image;

    private Pair<Sprite, Color> pair_CanDraw;
    private Pair<Sprite, Color> pair_CantDraw;

    #region Button
    public void Button_Return()
    {
        _gameManager.PlayButtonSfx();
        picking = AllBuffCard.None;
        showCanPickPanel.gameObject.SetActive(true);
        skillDescriptionPanel.gameObject.SetActive(false);

    }
    public void Button_ConFirm()
    {
        _gameManager.PlayButtonSfx();
        pickedCards.Add(picking);
        Button_Return();
        EndPlayerChooseSkill(chooseSkillPlayerColor);
    }

    private void SetDrawAgain(bool isUsed)
    {
        canDrawAgain = isUsed;
        Pair<Sprite, Color> target = isUsed ? pair_CanDraw : pair_CantDraw;
        image_drawAgain.sprite = target.first;
        image_drawAgain.color = target.second;
    }


    public void Button_DrawAgain()
    {
        if (!canDrawAgain) return;
        _gameManager.PlayButtonSfx();
        SetDrawAgain(false);
        CardReadyProcess().Forget();
    }
    public void Button_OpenSkillDescriptionPanel(AllBuffCard targetBuff)
    {
        _gameManager.PlayButtonSfx();
        picking = targetBuff;
        showCanPickPanel.gameObject.SetActive(false);
        skillDescriptionPanel.ChangeDescription(targetBuff, 0);
        skillDescriptionPanel.gameObject.SetActive(true);
    }

    #endregion

    private List<ChessType> PlayerCanPick()
    {
        List<ChessType> playerCanPick = new();

        if (chooseSkillPlayer.kingBuffType == KingBuff.None)
            playerCanPick.Add(ChessType.King);

        if (chooseSkillPlayer.queenBuffType == QueenBuff.None)
            playerCanPick.Add(ChessType.Queen);

        if (chooseSkillPlayer.bishopBuffType == BishopBuff.None)
            playerCanPick.Add(ChessType.Bishop);

        if (chooseSkillPlayer.knightBuffType == KnightBuff.None)
            playerCanPick.Add(ChessType.Knight);

        if (chooseSkillPlayer.rookBuffType == RookBuff.None)
            playerCanPick.Add(ChessType.Rook);

        if (chooseSkillPlayer.pawnBuffType == PawnBuff.None)
            playerCanPick.Add(ChessType.Pawn);

        return playerCanPick;
    }
    private void PickThreeCard()
    {
        List<ChessType> playerCanPick = PlayerCanPick();
        cardPikButton.Clear();
        pickedThreeCard.Clear();
        int count = Mathf.Min(3, playerCanPick.Count);

        for (int i = 0; i < count; i++)
        {
            int randomTypeIndex = UnityEngine.Random.Range(0, playerCanPick.Count);

            ChessType chessType = playerCanPick[randomTypeIndex];

            AllBuffCard[] buffs = buffChessDict[chessType];

            int buffIndex = UnityEngine.Random.Range(0, buffs.Length);
            AllBuffCard pickedBuff = buffs[buffIndex];



            canPickCard[i].SetCard(pickedBuff);

            playerCanPick.RemoveAt(randomTypeIndex);
            pickCardButton[i].onClick.AddListener(() => Button_OpenSkillDescriptionPanel(pickedBuff));
            cardPikButton.Add(() => Button_OpenSkillDescriptionPanel(pickedBuff));
            pickedThreeCard.Add(pickedBuff);
        }
    }
    private async UniTask CardReadyProcess()
    {
        await UniTask.Yield();
        pickCardButton[0].enabled = false;
        pickCardButton[1].enabled = false;
        pickCardButton[2].enabled = false;

        for (int i = 0; i < canPickCard.Length; i++)
        {
            await canPickCard[i].TurnTheCard(CardFace.Back);
        }
        PickThreeCard();
        for (int i = 0; i < canPickCard.Length; i++)
        {
            await canPickCard[i].TurnTheCard(CardFace.Front);
        }
        pickCardButton[0].enabled = true;
        pickCardButton[1].enabled = true;
        pickCardButton[2].enabled = true;

        if (chooseSkillPlayer.playerInPut.nowUsingDevice == CanUseDevice.Gamepad)
        {
            pickingIndex = 0;
            pickingMark.transform.localPosition = cardPosition[pickingIndex];
            pickingMark.gameObject.SetActive(true);
        }
    }
    private void EndOfChooseSkill()
    {
        skillDescriptionPanel.gameObject.SetActive(false);
        showCanPickPanel.gameObject.SetActive(false);

        _gameManager.EndSkillChoose(pickedCards[0], pickedCards[1]);
    }
    private void StartChooseSkill(ChessColor color)
    {
        chooseSkillPlayerColor = color;
        _inPutManager.PlayerInputStage(chooseSkillPlayerColor, InputStage.ChooseSkill);

        playerTag.sprite = _resourcesData.PlayerSprite(color);
        pickingMark.gameObject.SetActive(false);

        SetDrawAgain(true);
        CardReadyProcess().Forget();
        chooseSkillPlayer.playerInPut.StartGamepadInput();

    }
    public void EndPlayerChooseSkill(ChessColor color)
    {
        OffPlayerPick(color);
        if (isWhiteChessPlayerPick && isBlackChessPlayerPick)
        {
            EndOfChooseSkill();
            return;
        }

        if (!isWhiteChessPlayerPick) StartChooseSkill(ChessColor.White);
        else if(!isBlackChessPlayerPick) StartChooseSkill(ChessColor.Black);

    }




    #region Language Change

    [Header("Language Change")]
    public Image logo;
    private void LanguageChange()
    {
        logo.sprite = _languageManager.sp_ChooseSkills_Logo;
    }

    #endregion



    public void Init()
    {
        pickedCards.Clear();
        isWhiteChessPlayerPick = false;
        isBlackChessPlayerPick = false;
        pair_CanDraw = new(_languageManager.sp_Button_CanDrawAgain, Color.white);
        pair_CantDraw = new(_languageManager.sp_Button_CannotDrawAgain, new Color(0.5f, 0.5f, 0.5f));
        LanguageChange();

        PickMarkInit();
        Button_Return();

        StartChooseSkill(ChessColor.White);
    }




}
