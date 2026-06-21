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
    public PickCardManual pickCardManual;

    public MyButton confirmButton;

    [Header("PlayerTag")]
    public SpriteRenderer spr_playerTag;
    public Sprite sp_white_Tag;
    public Sprite sp_black_Tag;
    private Sprite sp_PlayerTag
    {
        get
        {
            switch (chooseSkillPlayerColor)
            {
                case ChessColor.White: return sp_white_Tag;
                case ChessColor.Black: return sp_black_Tag;
                default: return null;
            }
        }
    }

    [Header("DrawAgain")]
    private bool canDrawAgain = true;
    public MyButton button_DrawAgain;
    public SpriteRenderer spr_DrawAgain;
    private readonly Color c_Draw = Color.white;
    private readonly Color c_Drawed = new Color(0.5f, 0.5f, 0.5f);
    public Sprite sp_canDraw;
    public Sprite sp_cantDraw;

    public override void Confirm()
    {
        if (pickCardManual == null) return;
        chooseSkillPlayer.ChooseBuff(pickCardManual.pickedCard.buffCard);
        pickCardManual.Return();
        TurnSwitch(true);

        Debug.Log("ChooseSkillManual: Conform");
    }

    public override void DrawAgain()
    {
        if (spr_DrawAgain == null || button_DrawAgain == null)
        {
            Debug.LogError("spr_DrawAgain == null");
            return;
        }
        if (!canDrawAgain) return;
        canDrawAgain = false;
        button_DrawAgain.StopAllCoroutines();
        button_DrawAgain.enabled = false;

        spr_DrawAgain.color = c_Drawed;
        spr_DrawAgain.sprite = sp_cantDraw;

        StartCoroutine(CardReadyProcess());

    }

    private void EndOfChooseSkill()
    {
        gameObject.SetActive(false);
        _inGame.GameStart();
    }


    private void TurnSwitch(bool isSwitch)
    {
        if (spr_playerTag == null)
        {
            Debug.LogError("spr_playerTag == null");
            return;
        }
        if (isSwitch)
        {
            chooseSkillPlayerColor =
            chooseSkillPlayerColor == ChessColor.White ? 
            ChessColor.Black : ChessColor.White;

            if (chooseSkillPlayerColor == ChessColor.White)
            {
                EndOfChooseSkill();
                return;
            }

        }
        InPutManager.Instance.PlayerInputStage(chooseSkillPlayerColor, InputStage.ChooseSkill);
        spr_playerTag.sprite = sp_PlayerTag;

        canDrawAgain = true;
        button_DrawAgain.enabled = true;
        spr_DrawAgain.color = c_Draw;
        spr_DrawAgain.sprite = sp_canDraw;


        StartCoroutine(CardReadyProcess());
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
        chooseSkillPlayerColor = ChessColor.White;
        for (int i = 0; i < canPickCard.Length; i++)
        {
            StartCoroutine(canPickCard[i].TurnTheCard(CardFace.Back));
        }
        TurnSwitch(false);
    }

    private void Start()
    {
        
    }


    private void Update()
    {
        if (!pickCardManual.gameObject.activeSelf) 
        {
            if (confirmButton.gameObject.activeSelf|| button_DrawAgain.gameObject.activeSelf)
            {
                confirmButton.gameObject.SetActive(false);
                button_DrawAgain.gameObject.SetActive(true);
            }
        }
        else
        {
            if (!confirmButton.gameObject.activeSelf|| !button_DrawAgain.gameObject.activeSelf)
            {
                confirmButton.gameObject.SetActive(true);
                button_DrawAgain.gameObject.SetActive(false);

            }
        }

    }


}
