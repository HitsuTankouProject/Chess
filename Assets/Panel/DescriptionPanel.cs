using Cysharp.Threading.Tasks;
using Data;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

/// <summary>
/// ゲームルール、操作方法、駒と盤面、バフ一覧の説明画面を管理します。
/// ページ切り替え、ゲームパッド入力、言語別画像とバフ名の更新、
/// バフ一覧から詳細説明画面への遷移と復帰を制御します。
/// </summary>
public class DescriptionPanel : MonoBehaviour
{
    /// <summary>ゲーム全体を管理する共有インスタンスを取得します。</summary>
    private GameManager _gameManager => GameManager.Instance;
    /// <summary>現在言語のUI画像とカードテキストを管理するオブジェクトを取得します。</summary>
    private LanguageManager _languageManager => GameManager.Instance.languageManager;
    /// <summary>ボタンとバフ一覧の初期設定が完了しているかどうかを示します。</summary>
    private bool isInit = false;
    /// <summary>説明画面の最小ページ番号です。</summary>
    private int minPage = 0;
    /// <summary>現在表示している説明ページ番号です。</summary>
    private int nowPage = 0;
    /// <summary>説明画面の最大ページ番号です。</summary>
    private int maxPage = 3;
    /// <summary>ページ番号ごとの表示処理です。</summary>
    private Action[] buttonActions = new Action[4];
    /// <summary>各ページ番号と対応する説明画面表示処理を登録します。</summary>
    private void ButtonActionsInit()
    {
        buttonActions[0] = Button_OpenRulesDescription;
        buttonActions[1] = Button_OpenInputDescription;
        buttonActions[2] = Button_OpenChessAndBoardDescription;
        buttonActions[3] = Button_OpenBuffDescription;
    }
    /// <summary>ルール画面を表示し、入力待機と言語表示を開始します。</summary>
    public void Init()
    {
        Button_OpenRulesDescription();
        nowPage = 1;
        WaitGamePadInput_GameDescription().Forget();
        LanguageChange();

        // ボタンイベントとバフ一覧の登録は初回だけ行います。
        if (isInit)return;
        isInit = true;
        AllBuffInit();
        ButtonActionsInit();

    }
    /// <summary>
    /// 説明画面を表示している間、ゲームパッド入力を待機して各操作を実行します。
    /// </summary>
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
    /// <summary>
    /// 次の説明ページへ進みます。
    /// </summary>
    private void NextPage()
    {
        _gameManager.PlayButtonSfx();
        nowPage = Mathf.Min(nowPage + 1, maxPage);
        Debug.Log(nowPage);
        buttonActions[nowPage]();
    }
    /// <summary>
    /// 前の説明ページへ戻ります。
    /// </summary>
    private void BackPage()
    {
        _gameManager.PlayButtonSfx();
        nowPage = Mathf.Max(nowPage - 1, minPage);
        Debug.Log(nowPage);
        buttonActions[nowPage]();
    }
    /// <summary>
    /// 4種類すべての説明パネルを非表示にします。
    /// </summary>
    private void CloseAllTheObjectDescription()
    {
        rules_Description.SetActive(false);
        input_Description.SetActive(false);
        chessAndBoard_Description.SetActive(false);
        buff_Description.SetActive(false );
    }

    #region Language Change

    [Header("Language Change")]
    /// <summary>タイトルへ戻るボタンの画像です。</summary>
    public Image button_GameTitle;
    /// <summary>ゲーム開始ボタンの画像です。</summary>
    public Image button_GameStart;
    /// <summary>ルール項目ボタンの画像です。</summary>
    public Image button_Rules;
    /// <summary>ルール説明の画像です。</summary>
    public Image rules_intro;
    /// <summary>操作方法項目ボタンの画像です。</summary>
    public Image button_Control;
    /// <summary>操作方法説明の画像です。</summary>
    public Image control_intro;
    /// <summary>駒と盤面項目ボタンの画像です。</summary>
    public Image button_ChessAndBoard;
    /// <summary>駒と盤面の説明画像です。</summary>
    public Image chessAndBoard_intro;
    /// <summary>バフ項目ボタンの画像です。</summary>
    public Image button_Buffs;
    /// <summary>全バフカードの表示名テキストです。</summary>
    public TMP_Text[] buffNames;
    /// <summary>
    /// 現在言語のカードデータからバフ一覧の表示名を更新します。
    /// </summary>
    private void LanguageUpdate_GameDescription()
    {
        LanguageData target = _languageManager.NowUsingLanguageData();
        if (buffNames.Length > (int)AllBuffCard.AllBuffCount)
            Debug.LogError("gameDescription_BuffsName > AllBuffCount");
        for (int i = 0; i < (int)AllBuffCard.AllBuffCount; i++)
        {
            CardData targetBuffData = _languageManager.cardDataDict[(AllBuffCard)i];
            buffNames[i].text = targetBuffData.name;
        }
    }
    /// <summary>
    /// 説明画面の全画像とバフ名を現在の表示言語へ更新します。
    /// </summary>
    public void LanguageChange()
    {
        button_GameTitle.sprite = _languageManager.sp_GameTitle;
        button_GameStart.sprite = _languageManager.sp_GameStart;

        button_Rules.sprite = _languageManager.sp_Rules;
        rules_intro.sprite = _languageManager.sp_Rules_Intro;

        button_Control.sprite = _languageManager.sp_Control;
        control_intro.sprite = _languageManager.sp_Control_Intro;

        button_ChessAndBoard.sprite = _languageManager.sp_ChessAndBoard;
        chessAndBoard_intro.sprite = _languageManager.sp_ChessAndBoard_Intro;

        button_Buffs.sprite = _languageManager.sp_Buffs;

        LanguageUpdate_GameDescription();
    }

    #endregion

    [Header("Rules Description")]
    /// <summary>ゲームルール説明パネルです。</summary>
    public GameObject rules_Description;
    /// <summary>
    /// 他の説明を閉じてゲームルールページを表示します。
    /// </summary>
    public void Button_OpenRulesDescription()
    {
        _gameManager.PlayButtonSfx();
        CloseAllTheObjectDescription();
        nowPage = 0;
        rules_Description.SetActive(true);
    }

    [Header("Input Description")]
    /// <summary>操作方法説明パネルです。</summary>
    public GameObject input_Description;
    /// <summary>
    /// 他の説明を閉じて操作方法ページを表示します。
    /// </summary>
    public void Button_OpenInputDescription()
    {
        _gameManager.PlayButtonSfx();
        CloseAllTheObjectDescription();
        nowPage = 1;
        input_Description.SetActive(true);
    }

    [Header("ChessAndBoard Description")]
    /// <summary>駒と盤面の説明パネルです。</summary>
    public GameObject chessAndBoard_Description;
    /// <summary>
    /// 他の説明を閉じて駒と盤面のページを表示します。
    /// </summary>
    public void Button_OpenChessAndBoardDescription()
    {
        _gameManager.PlayButtonSfx();
        CloseAllTheObjectDescription();
        nowPage = 2;
        chessAndBoard_Description.SetActive(true);
    }


    [Header("Buff Description")]
    /// <summary>バフカード一覧パネルです。</summary>
    public GameObject buff_Description;
    /// <summary>選択したバフの詳細を表示するパネルです。</summary>
    public SkillDescriptionPanel skillDescriptionPanel;
    /// <summary>各バフカードの詳細を開くボタンです。</summary>
    public Button[] buffs;
    /// <summary>ゲームパッドで現在選択中のバフを示す画像です。</summary>
    public Image pickImage;
    /// <summary>現在選択しているバフボタンの配列番号です。</summary>
    private int pickIndex = 0;
    /// <summary>各バフボタンに対応する選択画像の座標です。</summary>
    private Vector3[] cardPositions;
    /// <summary>バフ番号と詳細画面を開く処理の対応表です。</summary>
    Dictionary<int, Action> cardButton = new();
    /// <summary>
    /// 全バフボタンへ詳細表示イベントを登録し、選択画像の座標を保存します。
    /// </summary>
    private void AllBuffInit()
    {
        cardPositions = new Vector3[buffs.Length];

        foreach (AllBuffCard buffCardName in Enum.GetValues(typeof(AllBuffCard)))
        {
            if (buffCardName == AllBuffCard.None || buffCardName == AllBuffCard.AllBuffCount) continue;
            int index = (int)buffCardName;
            buffs[index].onClick.RemoveAllListeners();
            buffs[index].onClick.AddListener(() => Button_OpenSkillDescriptionPanel(buffCardName));
            cardButton[index] = () => Button_OpenSkillDescriptionPanel(buffCardName);
            cardPositions[index] = buffs[index].gameObject.transform.localPosition;
        }
    }
    /// <summary>ゲームパッドで選択中のバフ番号を移動し、選択画像を更新します。</summary>
    /// <param name="value">現在の選択番号へ加算する移動量です。</param>
    private void SwitchPick(int value)
    {
        if (!buff_Description.activeSelf) return;
        _gameManager.PlayButtonSfx();
        pickIndex += value;
        if (pickIndex < 0) pickIndex = 0;
        if (pickIndex > buffs.Length) pickIndex = buffs.Length;

        pickImage.gameObject.transform.localPosition = cardPositions[pickIndex];

    }
    /// <summary>''
    /// 他の説明を閉じてバフ一覧ページを表示します。
    /// </summary>
    public void Button_OpenBuffDescription()
    {
        _gameManager.PlayButtonSfx();
        CloseAllTheObjectDescription();
        nowPage = 3;
        pickIndex = 0;
        pickImage.transform.localPosition = cardPositions[pickIndex];
        buff_Description.gameObject.SetActive(true);

    }
    /// <summary>指定バフカードの詳細説明画面を開きます。</summary>
    /// <param name="targetBuff">詳細を表示するバフカードです。</param>
    public void Button_OpenSkillDescriptionPanel(AllBuffCard targetBuff)
    {
        if (!buff_Description.activeSelf) return;
        _gameManager.PlayButtonSfx();
        buff_Description.gameObject.SetActive(false);
        skillDescriptionPanel.ChangeDescription(targetBuff, 0);
        skillDescriptionPanel.gameObject.SetActive(true);
    }
    /// <summary>
    /// バフ詳細画面を閉じてバフ一覧へ戻ります。
    /// </summary>
    public void Button_Return()
    {
        if (!skillDescriptionPanel.gameObject.activeSelf) return ;
        _gameManager.PlayButtonSfx();

        skillDescriptionPanel.gameObject.SetActive(false);
        buff_Description.gameObject.SetActive(true);

    }

}
