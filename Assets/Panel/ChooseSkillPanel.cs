using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ChooseSkillPanel : MonoBehaviour
{
    private GameManager _gameManager => GameManager.Instance;

    private ResourcesData _resourcesData => _gameManager.resourcesData;
    private InPutManager _inPutManager => _gameManager.inPutManager;

    private ChessColor chooseSkillPlayerColor = ChessColor.White;

    private Player chooseSkillPlayer => chooseSkillPlayerColor == ChessColor.White ?
        _gameManager.player01 : _gameManager.player02;

    private bool isWhiteChessPlayerPick = false;
    private bool isBlackChessPlayerPick = false;

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

    [Header("DrawAgain")]
    private bool canDrawAgain = true;
    public Button drawAgain;
    private Image image_drawAgain=> drawAgain.image;

    private readonly Color c_Draw = Color.white;
    private readonly Color c_Drawed = new Color(0.5f, 0.5f, 0.5f);

    private Sprite sp_canDraw => _resourcesData.allSprite.sp_canDraw;
    private Sprite sp_cantDraw => _resourcesData.allSprite.sp_canDraw;

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
        pickedThreeCard.Clear();
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

            pickedThreeCard.Add(buffs[buffIndex]);
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

    public void Button_OpenSkillDescriptionPanel(AllBuffCard targetBuff)
    {
        picking = targetBuff;
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
        picking = AllBuffCard.None;
        showCanPickPanel.gameObject.SetActive(true);
        skillDescriptionPanel.gameObject.SetActive(false);

    }
    public void Button_ConFirm()
    {
        pickedCards.Add(picking);
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


        _gameManager.EndSkillChoose(pickedCards[0], pickedCards[1]);
    }

    public void Init()
    {
        isWhiteChessPlayerPick = false;
        isBlackChessPlayerPick = false;
        Button_Return();
        StartPlayerTurn(ChessColor.White);
    }




}
