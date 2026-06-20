using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
public class ChooseSkillManual : ButtonManual
{
    private InGame _inGame => InGame.Instance;
    private ResourcesData _resourcesData => GameManager.Instance.resourcesData;

    private ChessColor chooseSkillPlayerColor = ChessColor.White;
    private Player chooseSkillPlayer =>
        chooseSkillPlayerColor == ChessColor.White ?
        _inGame.whiteChessPlayer : _inGame.blackChessPlayer;

    public Card[] canPickCard;
    private Card pickedCard;

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

    public override void PickTheCard(Card card)
    {
        pickedCard = card;
        PickTheCard();
        Debug.Log("ChooseSkillManual: PickTheCard");
    }

    public override void Confirm()
    {
        chooseSkillPlayer.ChooseBuff(pickedCard.buffCard);
        chooseSkillPlayerColor =
        chooseSkillPlayerColor == ChessColor.White ? ChessColor.Black : ChessColor.White;
        if (chooseSkillPlayerColor == ChessColor.White) ChooseTurnOff();

        Debug.Log("ChooseSkillManual: Conform");
    }

    public override void Return()
    {
        pickedCard = null;
        ReturnTheCard();
        Debug.Log("ChooseSkillManual: Return");
    }

    public override void DrawAgain()=> StartCoroutine(CardReadyProcess());

    [Header("Card's Description ")]
    public GameObject descriptionPad;
    public TMP_Text descriptionName;
    public MeshRenderer pickCardCover_MeshRenderer;
    public TMP_Text descriptionLevel01;
    public TMP_Text descriptionLevel02;
    public TMP_Text descriptionLevel03;


    private void DescriptionUpdate()
    {
        if (pickedCard == null) return;
        CardData cardData = _resourcesData.cardDataDict[pickedCard.buffCard];
        descriptionName.text = cardData.name;
        pickCardCover_MeshRenderer.material = cardData.m_CardCover;
        descriptionLevel01.text = cardData.buffLevel01Description;
        descriptionLevel02.text = cardData.buffLevel02Description;
        descriptionLevel03.text = cardData.buffLevel03Description;

    }

    private void PickTheCard()
    {
        descriptionPad.SetActive(true);
        DescriptionUpdate();
    }

    private void ReturnTheCard()
    {
        descriptionPad.SetActive(false);

    }

    private void ChooseTurnOff()
    {



    }

    private List<ChessType> PlayerCanPick()
    {
        List<ChessType> playerCanPick = new();

        if (chooseSkillPlayer.kingBuffType == Player.KingBuff.None)
            playerCanPick.Add(ChessType.King);

        if (chooseSkillPlayer.queenBuffType == Player.QueenBuff.None)
            playerCanPick.Add(ChessType.Queen);

        if (chooseSkillPlayer.bishopBuffType == Player.BishopBuff.None)
            playerCanPick.Add(ChessType.Bishop);

        if (chooseSkillPlayer.knightBuffType == Player.KnightBuff.None)
            playerCanPick.Add(ChessType.Knight);

        if (chooseSkillPlayer.rookBuffType == Player.RookBuff.None)
            playerCanPick.Add(ChessType.Rook);

        if (chooseSkillPlayer.pawnBuffType == Player.PawnBuff.None)
            playerCanPick.Add(ChessType.Pawn);

        return playerCanPick;
    }

    private void PickThreeCard()
    {
        List<ChessType> playerCanPick = PlayerCanPick();

        int count = Mathf.Min(3, playerCanPick.Count);

        for (int i = 0; i < count; i++)
        {
            int randomTypeIndex = Random.Range(0, playerCanPick.Count);

            ChessType chessType = playerCanPick[randomTypeIndex];

            AllBuffCard[] buffs = buffChessDict[chessType];

            canPickCard[i].SetCard(
                buffs[Random.Range(0, buffs.Length)]);

            playerCanPick.RemoveAt(randomTypeIndex);
        }
    }

    private IEnumerator CardReadyProcess()
    {
        yield return null;
        for (int i = 0; i < canPickCard.Length; i++)
        {
            yield return canPickCard[i].TurnTheCard(CardFace.Back);
        }
        PickThreeCard();
        for (int i = 0; i < canPickCard.Length; i++)
        {
            yield return canPickCard[i].TurnTheCard(CardFace.Front);
        }

    }

    public void Init()
    {
        StartCoroutine(CardReadyProcess());
    }


    private void Start()
    {
        Init();
    }

    public bool test;

    private void Update()
    {
        if (test)
        {
            test = false;
            DrawAgain();
        }
    }


}
