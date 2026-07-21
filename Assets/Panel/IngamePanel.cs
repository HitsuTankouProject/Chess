using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

/// <summary>
/// 対局中の手番情報と手番交代演出を表示するUIパネルです。
/// 現在手番のプレイヤー画像、白黒それぞれのアクションタグを更新し、
/// 手番交代時にはパネルの上下幅と文字サイズを補間するアニメーションを提供します。
/// </summary>
public class IngamePanel : MonoBehaviour
{
    /// <summary>プレイヤー色に対応する画像リソースを取得します。</summary>
    private ResourcesData _resourcesData => GameManager.Instance.resourcesData;
    [Header("Now Turn")]
    /// <summary>現在手番のプレイヤーを示す画像です。</summary>
    public Image nowTurnTag;
    /// <summary>白プレイヤーが操作中であることを示す画像です。</summary>
    public Image whiteActionTag;
    /// <summary>黒プレイヤーが操作中であることを示す画像です。</summary>
    public Image blackActionTag;

    [Header("Turn Change")]
    /// <summary>手番交代時に開閉するパネルのRectTransformです。</summary>
    public RectTransform turnChange_panel;
    /// <summary>手番交代パネルに表示するテキストです。</summary>
    public TMP_Text turnChange_text;
    /// <summary>手番交代パネルの開閉にかける時間（秒）です。</summary>
    private const float turnChange_time = 1.0f;
    /// <summary>手番交代文字の閉じた状態と開いた状態のフォントサイズです。</summary>
    private readonly Pair<int, int> turnChange_word_size = new(0, 250);
    /// <summary>手番交代パネルを開いた状態の下端・上端オフセットです。</summary>
    private readonly Pair<int, int> turnChange_panel_open = new(270, 270);
    /// <summary>手番交代パネルを閉じた状態の下端・上端オフセットです。</summary>
    private readonly Pair<int, int> turnChange_panel_close = new(540, 540);
    /// <summary>
    /// 対局開始時の手番表示を白プレイヤーへ初期化します。
    /// </summary>
    public void Init()
    {
        whiteActionTag.enabled = true;
        blackActionTag.enabled = false;
        nowTurnTag.sprite = _resourcesData.PlayerSprite(ChessColor.White);

    }
    /// <summary>指定プレイヤーへ手番表示を切り替えます。</summary>
    /// <param name="changeTo">新しく手番を開始するプレイヤーの駒色です。</param>
    public async UniTask TurnChange(ChessColor changeTo)
    {
        // 切り替え中は両プレイヤーのアクションタグを一度非表示にします。
        whiteActionTag.enabled = false;
        blackActionTag.enabled = false;

        nowTurnTag.sprite = _resourcesData.PlayerSprite(changeTo);

        bool isWriteTurn = changeTo == ChessColor.White;

        whiteActionTag.enabled = isWriteTurn;
        blackActionTag.enabled = !isWriteTurn;
    }

}
