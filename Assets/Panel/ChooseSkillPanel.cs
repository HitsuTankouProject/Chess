using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ChooseSkillPanel : MonoBehaviour
{
    private InGame _inGame => InGame.Instance;
    private ResourcesData _resourcesData => GameManager.Instance.resourcesData;
    private InPutManager _inPutManager => GameManager.Instance.inPutManager;
    private ChessColor chooseSkillPlayerColor = ChessColor.White;

    private Player chooseSkillPlayer => chooseSkillPlayerColor == ChessColor.White ?
        _inGame.whiteChessPlayer : _inGame.blackChessPlayer;
    private bool isWhiteChessPlayerPick = false;
    private bool isBlackChessPlayerPick = false;


    public Card[] canPickCard;
    public Button[] pickCardButton;
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

    public SkillDescriptionPanel skillDescriptionPanel;
    public GameObject showCanPickPanel;

    public Image playerTag;
    private AllBuffCard pickedCard;

    [Header("DrawAgain")]
    private bool canDrawAgain = true;
    public Button drawAgain;
    private Image image_drawAgain=> drawAgain.image;

    private readonly Color c_Draw = Color.white;
    private readonly Color c_Drawed = new Color(0.5f, 0.5f, 0.5f);

    public Sprite sp_canDraw;
    public Sprite sp_cantDraw;



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

            int buffIndex = Random.Range(0, buffs.Length);
            canPickCard[i].SetCard(buffs[buffIndex]);

            playerCanPick.RemoveAt(randomTypeIndex);
            pickCardButton[i].onClick.AddListener(() => Button_OpenSkillDescriptionPanel(buffs[buffIndex]));

        }
    }

    private IEnumerator CardReadyProcess()
    {
        yield return null;
        pickCardButton[0].enabled = false;
        pickCardButton[1].enabled = false;
        pickCardButton[2].enabled = false;


        for (int i = 0; i < canPickCard.Length; i++)
        {
            yield return canPickCard[i].TurnTheCard(CardFace.Back);
        }
        PickThreeCard();
        for (int i = 0; i < canPickCard.Length; i++)
        {
            yield return canPickCard[i].TurnTheCard(CardFace.Front);
        }
        pickCardButton[0].enabled = true;
        pickCardButton[1].enabled = true;
        pickCardButton[2].enabled = true;

    }

    private void Button_OpenSkillDescriptionPanel(AllBuffCard targetBuff)
    {
        showCanPickPanel.gameObject.SetActive(false);
        skillDescriptionPanel.ChangeDescription(targetBuff, 0);
        skillDescriptionPanel.gameObject.SetActive(true);
    }

    private void StartPlayerTurn(ChessColor color)
    {
        chooseSkillPlayerColor = color;
        _inPutManager.PlayerInputStage(chooseSkillPlayerColor, InputStage.ChooseSkill);
        playerTag.sprite = _resourcesData.PlayerSprite(color);
        canDrawAgain = true;
        image_drawAgain.sprite = sp_canDraw;
        image_drawAgain.color = c_Draw;
        StartCoroutine(CardReadyProcess());

    }

    public void Button_Return()
    {
        showCanPickPanel.gameObject.SetActive(true);
        skillDescriptionPanel.gameObject.SetActive(false);

    }
    public void Button_ConFirm()
    {
        chooseSkillPlayer.ChooseBuff(pickedCard);
        if (chooseSkillPlayerColor == ChessColor.White) isWhiteChessPlayerPick = true;
        else isBlackChessPlayerPick = true;
        Button_Return();
        TurnSwitch();
    }
    public void Button_DrawAgain()
    {
        if (!canDrawAgain) return;
        canDrawAgain = false;
        image_drawAgain.sprite = sp_cantDraw;
        image_drawAgain.color = c_Drawed;
        StartCoroutine(CardReadyProcess());
    }
    private void TurnSwitch()
    {
        if (isWhiteChessPlayerPick && !isBlackChessPlayerPick)
        {
            StartPlayerTurn(ChessColor.Black);
            return;

        }
        else if (isWhiteChessPlayerPick && isBlackChessPlayerPick)
        {
            EndOfChooseSkill();
            return;
        }
        Debug.LogError("isWhiteChose == flase");

    }
    private void EndOfChooseSkill()
    {
        isWhiteChessPlayerPick = false;
        isBlackChessPlayerPick = false;
        skillDescriptionPanel.gameObject.SetActive(false);
        showCanPickPanel.gameObject.SetActive(false);
        _inGame.EndOfChooseSkill();
    }




    public void Init()
    {
        isWhiteChessPlayerPick = false;
        isBlackChessPlayerPick = false;
        Button_Return();
        StartPlayerTurn(ChessColor.White);
    }




}
