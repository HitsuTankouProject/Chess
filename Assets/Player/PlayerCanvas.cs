using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

/// <summary>
/// プレイヤーごとの対局UI、ポーズ画面、選択済みバフ、確認ダイアログを管理します。
/// マウス・ゲームパッドに応じたカード選択表示、バフ詳細画面、
/// 投了とタイトル復帰の確認処理、現在言語へのUI画像更新を行います。
/// </summary>
public class PlayerCanvas : MonoBehaviour
{
    /// <summary>ゲーム全体を管理する共有インスタンスを取得します。</summary>
    private GameManager _gameManager => GameManager.Instance;
    /// <summary>現在言語のUI画像を取得します。</summary>
    private LanguageManager _languageManager => _gameManager.languageManager;
    /// <summary>ポーズ中に表示するパネルです。</summary>
    public GameObject pausePanel;
    /// <summary>このUIを所有するプレイヤーです。</summary>
    private Player _player;
    /// <summary>所有者がゲームパッドを使用中かどうかを取得します。</summary>
    private bool isPlayerUseGamePad => _player.playerInPut.nowUsingDevice == CanUseDevice.Gamepad;
    /// <summary>このプレイヤー側の盤面を表示するカメラです。</summary>
    public Camera playerCamera;
    /// <summary>現在ポーズ中かどうかを取得します。</summary>
    public bool isPause { get; private set; }
    /// <summary>所有者、選択済みバフ、言語画像を使用してUIを初期化します。</summary>
    public void Init(Player player, List<AllBuffCard> choseBuffs)
    {
        _player = player;
        PauseInit(choseBuffs);
        ChangeLanguage();
    }

    #region Language Change
    [Header("Language Change")]
    /// <summary>操作案内画像です。</summary>
    public Image image_ActionMark;
    /// <summary>ポーズボタン画像です。</summary>
    public Image image_Pause;
    /// <summary>タイトル復帰ボタン画像です。</summary>
    public Image image_GameTitle;
    /// <summary>投了ボタン画像です。</summary>
    public Image image_Surrender;
    /// <summary>決定ボタン画像です。</summary>
    public Image image_Confirm;
    /// <summary>戻るボタン画像です。</summary>
    public Image image_Return;
    /// <summary>すべてのUI画像を現在の表示言語へ更新します。</summary>
    private void ChangeLanguage()
    {
        image_ActionMark.sprite = _languageManager.sp_ActionMark;
        image_Pause.sprite = _languageManager.sp_Button_Pause;

        image_GameTitle.sprite = _languageManager.sp_GameTitle;
        image_Surrender.sprite = _languageManager.sp_Button_Surrender;

        image_Confirm.sprite = _languageManager.sp_Button_Confirm;
        image_Return.sprite = _languageManager.sp_Button_Return;

    }

    #endregion


    #region Button

    /// <summary>
    /// 対局中のポーズ状態とゲームパッド用選択マークを切り替えます。
    /// </summary>
    public void Button_Pause()
    {
        if (GameManager.Instance.nowGameStage != GameStage.InGame || isConfirming) return;
        _gameManager.PlayButtonSfx();
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
    /// <summary>
    /// 指定バフの現在レベルを取得して詳細画面を開きます。
    /// </summary>
    public void Button_OpenSkillDescriptionPanel(AllBuffCard targetBuff)
    {
        _gameManager.PlayButtonSfx();
        uint nowLevel = _player.cardBuffMap[targetBuff].nowBuffLevel;
        Debug.Log(nowLevel);
        skillDescriptionPanel.ChangeDescription(targetBuff, nowLevel);
        skillDescriptionPanel.gameObject.SetActive(true);
    }
    /// <summary>
    /// バフ詳細画面を閉じます。
    /// </summary>
    public void Button_CloseSkillDescriptionPanel()
    {
        _gameManager.PlayButtonSfx();
        skillDescriptionPanel.gameObject.SetActive(false);

    }
    /// <summary>
    /// 投了確認パネルを開きます。
    /// </summary>
    public void Button_Surrender()
    {
        if (GameManager.Instance.nowGameStage != GameStage.InGame || isConfirming) return;
        _gameManager.PlayButtonSfx();
        OpenConfirmPanel(ConfirmStage.Surrender);
    }
    /// <summary>
    /// タイトル復帰確認パネルを開きます。
    /// </summary>
    public void Button_BackToGameTitle()
    {
        if (GameManager.Instance.nowGameStage != GameStage.InGame || isConfirming) return;
        _gameManager.PlayButtonSfx();
        OpenConfirmPanel(ConfirmStage.BackToGameTitle);

    }
    /// <summary>
    /// 確認操作を取り消して確認パネルを閉じます。
    /// </summary>
    public void Button_Return()
    {
        _gameManager.PlayButtonSfx();
        confirmStage = ConfirmStage.None;
        confirmPanel.SetActive(false);
    }
    /// <summary>
    /// 現在の確認内容に応じて投了またはタイトル復帰を実行します。
    /// </summary>
    public void Button_Confirm()
    {
        _gameManager.PlayButtonSfx();
        switch (confirmStage)
        {
            case ConfirmStage.Surrender: GameManager.Instance.Surrender(_player.usingChess); break;
            case ConfirmStage.BackToGameTitle: GameManager.Instance.Button_BackToGameTitle(); break;
            default: return;
        }
        isPause = false;
        pausePanel.SetActive(isPause);
        skillDescriptionPanel.gameObject.SetActive(false);
        Button_Return();
    }

    public ControllerMarkPrinter  controllerMarkPrinter;




    #endregion

    #region Pause
    [Header("Pause")]
    /// <summary>ゲームパッドで現在選択中のバフカードを示す画像です。</summary>
    public Image pickCard;
    /// <summary>
    /// ゲームパッドで現在選択しているバフカードの配列番号です。
    /// ポーズ画面を開いた際は0へ戻り、左右のカード移動操作によって更新されます。
    /// この値を使用して選択マークの位置と、開くバフ詳細を決定します。
    /// </summary>
    private int pickCardIndex = 0;
    /// <summary>
    /// ゲームパッドで選択できるバフカードの最大配列番号を取得します。
    /// 選択済みバフ数から1を引いた値を返し、<see cref="NextCard" /> で
    /// 選択番号が実際のカード数を越えないようにする上限として使用します。
    /// </summary>
    private int maxCanPick => _player.choseBuffs.Count - 1;
    /// <summary>選択済みバフカードを表示するボタンです。</summary>
    public Button[] cards;
    private List<Vector3> cardsPos = new();
    /// <summary>各カードの詳細画面を開く処理です。</summary>
    public List<Action> cardActions = new();
    /// <summary>
    /// 選択済みバフをカードボタンへ設定し、詳細表示イベントを登録します。
    /// </summary>
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
                cards[i].image.sprite = LanguageManager.Instance.cardDataDict[targetBuff].sp_CardCover;

                cards[i].onClick.AddListener(() => Button_OpenSkillDescriptionPanel(targetBuff));
                cardsPos.Add(cards[i].transform.localPosition);
                cardActions.Add(() => Button_OpenSkillDescriptionPanel(targetBuff));
            }
            else
                cards[i].image.sprite = GameManager.Instance.languageManager.cradDataList.sp_CardBack;
        }
    }
    /// <summary>
    /// ゲームパッドで選択中のバフ詳細を開きます。
    /// </summary>
    public void WatchBuffSkillDescription()
    {
        if (!isPlayerUseGamePad) return;
        cardActions[pickCardIndex]();
    }
    /// <summary>
    /// ゲームパッドの選択対象を次のバフカードへ移動します。
    /// </summary>
    public void NextCard()
    {
        if (!isPlayerUseGamePad) return;
        _gameManager.PlayButtonSfx();
        pickCardIndex = Mathf.Min(pickCardIndex + 1, maxCanPick);
        pickCard.transform.localPosition = cardsPos[pickCardIndex];
    }
    /// <summary>
    /// ゲームパッドの選択対象を前のバフカードへ移動します。
    /// </summary>
    public void BackCard()
    {
        if (!isPlayerUseGamePad) return;
        _gameManager.PlayButtonSfx();
        pickCardIndex = Mathf.Max(pickCardIndex - 1, 0);
        pickCard.transform.localPosition = cardsPos[pickCardIndex];
    }


    #endregion

    #region Confirm
    [Header("Confirm Panel")]
    /// <summary>投了・タイトル復帰の確認パネルです。</summary>
    public GameObject confirmPanel;
    /// <summary>現在の確認内容を表示する画像です。</summary>
    public Image confirmImage;
    /// <summary>
    /// 投了またはタイトル復帰の確認パネルを現在表示しているか取得します。
    /// 確認中は、ポーズ切り替えや別の確認画面を開く操作を受け付けません。
    /// </summary>
    public bool isConfirming => confirmPanel.activeSelf;
    /// <summary>確認パネルでユーザーへ確認している操作の種類を表します。</summary>
    private enum ConfirmStage 
    {
        /// <summary>確認対象の操作が設定されていない状態です。</summary>
        None,
        /// <summary>現在のプレイヤーが投了する操作を確認しています。</summary>
        Surrender,
        /// <summary>対局を終了してゲームタイトルへ戻る操作を確認しています。</summary>
        BackToGameTitle
    }
    /// <summary>
    /// 現在の確認パネルで選択されている操作を保持します。
    /// 決定ボタンが押された際、この値に応じて投了またはタイトル復帰を実行します。
    /// </summary>
    private ConfirmStage confirmStage;
    /// <summary>
    /// 現在の確認内容に対応する言語別画像を取得します。
    /// </summary>
    private Sprite Sp_Confirm()
    {
        switch (confirmStage)
        {
            case ConfirmStage.None: return null;
            case ConfirmStage.Surrender: return _languageManager.sp_Confirm_Surrender;
            case ConfirmStage.BackToGameTitle: return _languageManager.sp_Confirm_BackToGameTitle;
            default: return null;
        }
    }
    /// <summary>
    /// 指定内容の確認画像を設定して確認パネルを開きます。
    /// </summary>
    private void OpenConfirmPanel(ConfirmStage stage)
    {
        confirmStage = stage;
        confirmImage.sprite = Sp_Confirm();
        confirmPanel.SetActive(true);
    }

    


    #endregion

    #region SkillDescriptionPanel
    [Header("Skill Description Panel ")]
    /// <summary>選択済みバフの詳細を表示するパネルです。</summary>
    public SkillDescriptionPanel skillDescriptionPanel;
    #endregion
}
