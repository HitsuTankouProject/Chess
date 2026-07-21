using UnityEngine;

/// <summary>
/// 1つの表示言語で使用するテキストとUI画像をまとめたデータです。
/// CSV形式の翻訳テキストに加え、タイトル、ゲーム説明、スキル選択、
/// 対局、リザルト画面で使用する言語別スプライトを保持します。
/// UnityのProjectウィンドウから言語ごとのアセットとして作成できます。
/// </summary>
[CreateAssetMenu(fileName = "LanguageData", menuName = "GameLanguage/LanguageData")]
public class LanguageData : ScriptableObject
{
    /// <summary>言語ごとの翻訳テキストを格納したCSVファイルです。</summary>
    public TextAsset csvFile;
    [Header("GameStage")]
    /// <summary>ゲームタイトル画面で表示するタイトル画像です。</summary>
    public Sprite sp_GameTitle;
    /// <summary>ゲーム開始ボタンに表示する画像です。</summary>
    public Sprite sp_GameStart;
    /// <summary>ゲーム説明ボタンに表示する画像です。</summary>
    public Sprite sp_Description;

    [Header("Common")]
    /// <summary>各画面で共通して使用する戻るボタンの画像です。</summary>
    public Sprite sp_Button_Return;
    /// <summary>各画面で共通して使用する決定ボタンの画像です。</summary>
    public Sprite sp_Button_Confirm;

    [Header("Description")]
    /// <summary>ゲームルール項目の見出し画像です。</summary>
    public Sprite sp_Rules;
    /// <summary>ゲームルールの説明画像です。</summary>
    public Sprite sp_Rules_Intro;
    /// <summary>操作方法項目の見出し画像です。</summary>
    public Sprite sp_Control;
    /// <summary>操作方法の説明画像です。</summary>
    public Sprite sp_Control_Intro;
    /// <summary>駒と盤面項目の見出し画像です。</summary>
    public Sprite sp_ChessAndBoard;
    /// <summary>駒と盤面の説明画像です。</summary>
    public Sprite sp_ChessAndBoard_Intro;
    /// <summary>バフ説明項目で表示する画像です。</summary>
    public Sprite sp_Buffs;

    [Header("Choose Skills")]
    /// <summary>スキル選択画面で表示するロゴ画像です。</summary>
    public Sprite sp_ChooseSkills_Logo;
    /// <summary>スキルを再抽選できる場合に表示するボタン画像です。</summary>
    public Sprite sp_Button_CanDrawAgain;
    /// <summary>スキルを再抽選できない場合に表示するボタン画像です。</summary>
    public Sprite sp_Button_CannotDrawAgain;

    [Header("InGame")]
    /// <summary>対局中の操作内容を示すアクションマーク画像です。</summary>
    public Sprite sp_ActionMark;
    /// <summary>ポーズボタンに表示する画像です。</summary>
    public Sprite sp_Button_Pause;
    /// <summary>投了ボタンに表示する画像です。</summary>
    public Sprite sp_Button_Surrender;
    /// <summary>投了確認画面に表示する画像です。</summary>
    public Sprite sp_Confirm_Surrender;
    /// <summary>タイトルへ戻る確認画面に表示する画像です。</summary>
    public Sprite sp_Confirm_BackToGameTitle;

    [Header("Release")]
    /// <summary>リザルト画面で勝者を示す画像です。</summary>
    public Sprite sp_Release_Winner;
    /// <summary>ゲーム再開ボタンに表示する画像です。</summary>
    public Sprite sp_Button_Resume;
    /// <summary>ゲーム終了ボタンに表示する画像です。</summary>
    public Sprite sp_Button_Quit;




}
