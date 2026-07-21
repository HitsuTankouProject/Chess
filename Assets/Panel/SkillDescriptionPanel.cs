using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
/// <summary>
/// 選択されたバフカードの詳細情報を表示するUIパネルです。
/// 現在の表示言語に対応するカード画像、名前、3段階の効果説明を設定し、
/// バフの現在レベルに応じて取得済みレベルのマークを表示します。
/// 決定・戻るボタンの画像も、現在の言語設定へ更新します。
/// </summary>
public class SkillDescriptionPanel : MonoBehaviour
{
    /// <summary>現在言語のカードデータとボタン画像を管理するオブジェクトを取得します。</summary>
    private LanguageManager _languageManager => GameManager.Instance.languageManager;
    /// <summary>選択されたバフカードの表面画像を表示します。</summary>
    public Image cardImage;
    /// <summary>選択されたバフカードの名前を表示します。</summary>
    public TMP_Text cardName;
    /// <summary>レベル1から3までのバフ効果説明を表示するテキスト配列です。</summary>
    public TMP_Text[] cardBuffLevelDescriptions;
    /// <summary>現在取得済みのバフレベルを示すマーク配列です。</summary>
    public Image[] nowBuffMark;

    [Header("Button Image")]
    /// <summary>決定ボタンに表示する画像です。</summary>
    public Image image_confirm;
    /// <summary>戻るボタンに表示する画像です。</summary>
    public Image image_return;
    /// <summary>
    /// 決定・戻るボタンを現在の表示言語に対応する画像へ更新します。
    /// </summary>
    private void ChangeButtonImage()
    {
        // コンポーネントが設定済みで画像が異なる場合だけ差し替えます。
        if (image_confirm != null&& image_confirm.sprite != _languageManager.sp_Button_Confirm)
            image_confirm.sprite = _languageManager.sp_Button_Confirm;

        if (image_return != null && image_return.sprite != _languageManager.sp_Button_Return)
            image_return.sprite = _languageManager.sp_Button_Return;
            
    }
    /// <summary>指定バフカードの画像、名前、効果説明、現在レベルを表示します。</summary>
    /// <param name="buffCard">詳細を表示するバフカードです。</param>
    /// <param name="nowLevel">現在取得しているバフレベルです。</param>
    public void ChangeDescription(AllBuffCard buffCard, uint nowLevel)
    {
        ChangeButtonImage();

        Data.CardData cardData = _languageManager.cardDataDict[buffCard];

        cardImage.sprite = cardData.sp_CardCover;
        cardName.text = cardData.name;

        // CSVから読み込まれた3段階の効果説明を対応する欄へ設定します。
        cardBuffLevelDescriptions[0].text = cardData.buffLevel01Description;
        cardBuffLevelDescriptions[1].text = cardData.buffLevel02Description;
        cardBuffLevelDescriptions[2].text = cardData.buffLevel03Description;
        // 現在レベル以下のマークだけを表示し、取得済み効果を示します。
        for (int i = 0; i < nowBuffMark.Length; i++)
        {
            nowBuffMark[i].enabled = nowLevel >= i + 1;
        }

    }
    /// <summary>
    /// ボタン効果音を再生し、バフ詳細パネルを閉じます。
    /// </summary>
    public void Button_Return()
    {
        GameManager.Instance.PlayButtonSfx();
        gameObject.SetActive(false);
    }
}
