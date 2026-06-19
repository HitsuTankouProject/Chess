using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class ChooseSkillManual : ButtonManual
{
    private InGame _inGame => InGame.Instance;
    private ChessColor chooseSkillPlayerColor = ChessColor.White;
    private Player chooseSkillPlayer =>
        chooseSkillPlayerColor == ChessColor.White ?
        _inGame.whiteChessPlayer : _inGame.blackChessPlayer;

    public Card[] canPickCard;
    private Card pickedCard;

    public override void PickTheCard(Card card)
    {
        pickedCard = card;
        PickTheCard();
        Debug.Log("ChooseSkillManual: PickTheCard");
    }

    public override void Conform()
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
    public override void DrawAgain()
    {
        StartCoroutine(CardReadyProcess());
    }


    private void PickTheCard()
    {


    }

    private void ReturnTheCard()
    {


    }

    private void ChooseTurnOff()
    {



    }

    private List<AllBuffCard> PlayerCanPick()
    {
        List<AllBuffCard> playerCanPick = new();

        if (chooseSkillPlayer.kingBuffType == Player.KingBuff.None)
            playerCanPick.AddRange(new[] { AllBuffCard.SageKing, AllBuffCard.MadKing });

        if (chooseSkillPlayer.queenBuffType == Player.QueenBuff.None)
            playerCanPick.AddRange(new[] { AllBuffCard.Witcher, AllBuffCard.Beauty });

        if (chooseSkillPlayer.knightBuffType == Player.KnightBuff.None)
            playerCanPick.AddRange(new[] { AllBuffCard.Charger, AllBuffCard.Skirmisher });


        if (chooseSkillPlayer.bishopBuffType == Player.BishopBuff.None)
            playerCanPick.AddRange(new[] { AllBuffCard.Sorcerer, AllBuffCard.Monk });

        if (chooseSkillPlayer.rookBuffType == Player.RookBuff.None)
            playerCanPick.AddRange(new[] { AllBuffCard.Rusher, AllBuffCard.Guardian });

        if (chooseSkillPlayer.pawnBuffType == Player.PawnBuff.None)
            playerCanPick.AddRange(new[] { AllBuffCard.Scout, AllBuffCard.Substitute });

        return playerCanPick;
    }
    private void PickThreeCard()
    {
        List<AllBuffCard> playerCanPick = PlayerCanPick();

        int count = Mathf.Min(3, playerCanPick.Count);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, playerCanPick.Count);

            canPickCard[i].SetCard(playerCanPick[randomIndex]);

            playerCanPick.RemoveAt(randomIndex);
        }
    }





    public IEnumerator CardReadyProcess()
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






    private void Start()
    {
        StartCoroutine(CardReadyProcess());
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
