using Cysharp.Threading.Tasks;
using Data;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 両プレイヤーのバフカード選択画面を管理します。
/// 未選択の駒種から3枚の候補を抽選し、カードの裏返し演出、詳細表示、
/// 再抽選、マウス・ゲームパッド操作、プレイヤー交代を制御します。
/// 両プレイヤーの選択完了後は、選ばれたバフをゲーム進行へ渡します。
/// </summary>
public class ChooseSkillPanel : MonoBehaviour
{
    /// <summary>ゲーム全体を管理する共有インスタンスを取得します。</summary>
    private GameManager _gameManager => GameManager.Instance;
    /// <summary>ゲームで共有する画像とカードリソースを取得します。</summary>
    private ResourcesData _resourcesData => _gameManager.resourcesData;
    /// <summary>現在言語のUI画像を管理するオブジェクトを取得します。</summary>
    private LanguageManager _languageManager => _gameManager.languageManager;
    /// <summary>両プレイヤーの入力ステージを管理するオブジェクトを取得します。</summary>
    private InPutManager _inPutManager => _gameManager.inPutManager;
    /// <summary>現在スキルを選択しているプレイヤーの駒色を取得します。</summary>
    public ChessColor chooseSkillPlayerColor { get; private set; } = ChessColor.White;
    /// <summary>現在スキルを選択しているプレイヤーを取得します。</summary>
    private Player chooseSkillPlayer => chooseSkillPlayerColor == ChessColor.White ?
        _gameManager.player01 : _gameManager.player02;

    /// <summary>白プレイヤーがカード選択を完了したかどうかを示します。</summary>
    private bool isWhiteChessPlayerPick = false;
    /// <summary>黒プレイヤーがカード選択を完了したかどうかを示します。</summary>
    private bool isBlackChessPlayerPick = false;
    /// <summary>指定プレイヤーを選択完了状態にし、カード選択入力を無効化します。</summary>
    /// <param name="color">選択を完了したプレイヤーの駒色です。</param>
    private void OffPlayerPick(ChessColor color)
    {
        if (color == ChessColor.White) isWhiteChessPlayerPick = true;
        else if (color == ChessColor.Black) isBlackChessPlayerPick = true;
        _inPutManager.PlayerInputStage(chooseSkillPlayerColor, InputStage.None);

    }

    [Header("Pick Cards")]
    /// <summary>候補として表示する3Dカードの配列です。</summary>
    public Card[] canPickCard;
    /// <summary>候補として表示する3Dカードの配列です。</summary>
    public Button[] pickCardButton;
    /// <summary>現在画面へ提示されている最大3枚のバフカードを取得します。</summary>
    public List<AllBuffCard> pickedThreeCard { get; private set; } = new();
    /// <summary>駒種と、その駒種で選択できる2種類のバフカードの対応表です。</summary>
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
    /// <summary>選択中のバフカード詳細を表示するパネルです。</summary>
    public SkillDescriptionPanel skillDescriptionPanel;
    /// <summary>選択可能なカード一覧を表示するパネルです。</summary>
    public GameObject showCanPickPanel;

    [Header("Picking Tag")]
    /// <summary>現在選択中のプレイヤー色を表示する画像です。</summary>
    public Image playerTag;
    /// <summary>詳細画面で現在確認しているバフカードです。</summary>
    private AllBuffCard picking;
    /// <summary>現在いずれかのバフカード詳細を確認中かどうかを取得します。</summary>
    public bool isPicking => picking != AllBuffCard.None;
    /// <summary>白・黒プレイヤーが確定したバフカードを順番に保持します。</summary>
    private List<AllBuffCard> pickedCards = new();

    [Header("Picking Mark")]
    /// <summary>ゲームパッド操作時に現在の候補を示す選択マークです。</summary>
    public Image pickingMark;
    /// <summary>ゲームパッドで現在選択している候補カードの配列番号です。</summary>
    private int pickingIndex = 0;
    /// <summary>候補カードごとの決定処理です。</summary>
    private List<Action> cardPikButton = new();
    /// <summary>選択マークをカードの前面へ表示するためのZ座標です。</summary>
    private const float posZ = -240.0f;
    /// <summary>各候補カードに対応する選択マークのローカル座標です。</summary>
    private List<Vector3> cardPosition = new();
    /// <summary>
    /// 各候補カードの位置からゲームパッド用選択マーク座標を作成します。
    /// </summary>
    private void PickMarkInit()
    {
        cardPosition.Clear();
        for (int i = 0; i < canPickCard.Length; i++)
        {
            Vector3 target = canPickCard[i].transform.localPosition;
            cardPosition.Add(new Vector3(target.x, target.y, posZ));
        }

    }
    /// <summary>
    /// ゲームパッドの選択対象を右隣の候補カードへ移動します。
    /// </summary>
    public void PickNextCard()
    {
        pickingIndex = Mathf.Min(pickingIndex + 1, canPickCard.Length - 1);
        pickingMark.transform.localPosition = cardPosition[pickingIndex];
    }
    /// <summary>
    /// ゲームパッドの選択対象を左隣の候補カードへ移動します。
    /// </summary>
    public void PickBackCard()
    {
        pickingIndex = Mathf.Max(pickingIndex - 1, 0);
        pickingMark.transform.localPosition = cardPosition[pickingIndex];
    }
    /// <summary>
    /// ゲームパッドで現在選択している候補カードの詳細画面を開きます。
    /// </summary>
    public void PickThatCard() => cardPikButton[pickingIndex]();

    [Header("DrawAgain")]
    /// <summary>現在のプレイヤーが候補カードを再抽選できるかどうかを示します。</summary>
    private bool canDrawAgain = true;
    /// <summary>候補カードを再抽選するボタンです。</summary>
    public Button drawAgain;
    /// <summary>再抽選ボタンの画像コンポーネントを取得します。</summary>
    private Image image_drawAgain=> drawAgain.image;
    /// <summary>再抽選可能時に使用するボタン画像と色です。</summary>
    private Pair<Sprite, Color> pair_CanDraw;
    /// <summary>再抽選不可時に使用するボタン画像と色です。</summary>
    private Pair<Sprite, Color> pair_CantDraw;

    #region Button
    /// <summary>
    /// カード詳細画面を閉じ、候補カード一覧へ戻ります。
    /// </summary>
    public void Button_Return()
    {
        _gameManager.PlayButtonSfx();
        picking = AllBuffCard.None;
        showCanPickPanel.gameObject.SetActive(true);
        skillDescriptionPanel.gameObject.SetActive(false);

    }
    /// <summary>
    /// 詳細表示中のバフカードを確定し、次のプレイヤーへ選択を移します。
    /// </summary>
    public void Button_ConFirm()
    {
        _gameManager.PlayButtonSfx();
        pickedCards.Add(picking);
        Button_Return();
        EndPlayerChooseSkill(chooseSkillPlayerColor);
    }
    /// <summary>再抽選の可否とボタンの画像・色を更新します。</summary>
    /// <param name="isUsed">再抽選を可能にする場合は <see langword="true" /> です。</param>
    private void SetDrawAgain(bool isUsed)
    {
        canDrawAgain = isUsed;
        Pair<Sprite, Color> target = isUsed ? pair_CanDraw : pair_CantDraw;
        image_drawAgain.sprite = target.first;
        image_drawAgain.color = target.second;
    }
    /// <summary>
    /// 再抽選権を消費し、候補カードを新しく抽選します。
    /// </summary>
    public void Button_DrawAgain()
    {
        if (!canDrawAgain) return;
        _gameManager.PlayButtonSfx();
        SetDrawAgain(false);
        CardReadyProcess().Forget();
    }
    /// <summary>指定バフカードの詳細画面を開きます。</summary>
    /// <param name="targetBuff">詳細を表示するバフカードです。</param>
    public void Button_OpenSkillDescriptionPanel(AllBuffCard targetBuff)
    {
        _gameManager.PlayButtonSfx();
        picking = targetBuff;
        showCanPickPanel.gameObject.SetActive(false);
        skillDescriptionPanel.ChangeDescription(targetBuff, 0);
        skillDescriptionPanel.gameObject.SetActive(true);
    }

    #endregion

    /// <summary>現在のプレイヤーがまだバフを選択していない駒種を取得します。</summary>
    /// <returns>新しいバフカード候補を抽選できる駒種一覧です。</returns>
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
    /// <summary>
    /// 未選択の駒種から重複しない最大3種類を選び、バフカード候補を抽選します。
    /// </summary>
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

            // 同じ駒種が複数候補へ出ないよう、抽選済みの駒種を除外します。
            playerCanPick.RemoveAt(randomTypeIndex);
            pickCardButton[i].onClick.AddListener(() => Button_OpenSkillDescriptionPanel(pickedBuff));
            cardPikButton.Add(() => Button_OpenSkillDescriptionPanel(pickedBuff));
            pickedThreeCard.Add(pickedBuff);
        }
    }
    /// <summary>
    /// カードを裏返して候補を抽選し、表へ戻す一連の準備演出を実行します。
    /// </summary>
    private async UniTask CardReadyProcess()
    {
        await UniTask.Yield();

        // 回転中の重複操作を防ぐため、すべての候補ボタンを一時的に無効化します。
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

        // ゲームパッド操作中は最初のカードへ選択マークを表示します。
        if (chooseSkillPlayer.playerInPut.nowUsingDevice == CanUseDevice.Gamepad)
        {
            pickingIndex = 0;
            pickingMark.transform.localPosition = cardPosition[pickingIndex];
            pickingMark.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// カード選択画面を閉じ、両プレイヤーが選んだバフをゲーム進行へ渡します。
    /// </summary>
    private void EndOfChooseSkill()
    {
        skillDescriptionPanel.gameObject.SetActive(false);
        showCanPickPanel.gameObject.SetActive(false);

        _gameManager.EndSkillChoose(pickedCards[0], pickedCards[1]);
    }
    /// <summary>指定プレイヤーのカード選択を開始します。</summary>
    /// <param name="color">カードを選択するプレイヤーの駒色です。</param>
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
    /// <summary>指定プレイヤーの選択を完了し、次のプレイヤーまたはゲーム進行へ移ります。</summary>
    /// <param name="color">選択を完了したプレイヤーの駒色です。</param>
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
    /// <summary>スキル選択画面のロゴ画像です。</summary>
    public Image logo;
    /// <summary>
    /// 現在の表示言語に対応するスキル選択ロゴを設定します。
    /// </summary>
    private void LanguageChange()
    {
        logo.sprite = _languageManager.sp_ChooseSkills_Logo;
    }

    #endregion


    /// <summary>
    /// 選択状態、言語画像、再抽選表示、選択マークを初期化し、白プレイヤーから開始します。
    /// </summary>
    public void Init()
    {
        pickedCards.Clear();
        isWhiteChessPlayerPick = false;
        isBlackChessPlayerPick = false;

        // 現在言語の画像と色から、再抽選可能・不可の表示情報を作成します。
        pair_CanDraw = new(_languageManager.sp_Button_CanDrawAgain, Color.white);
        pair_CantDraw = new(_languageManager.sp_Button_CannotDrawAgain, new Color(0.5f, 0.5f, 0.5f));
        LanguageChange();

        PickMarkInit();
        Button_Return();

        StartChooseSkill(ChessColor.White);
    }




}
