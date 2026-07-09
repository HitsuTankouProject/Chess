using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

public class IngamePanel : MonoBehaviour
{
    private ResourcesData _resourcesData => GameManager.Instance.resourcesData;

    [Header("Now Turn")]
    public Image nowTurnTag;
    public Image whiteActionTag;
    public Image blackActionTag;

    [Header("Turn Change")]
    public RectTransform turnChange_panel;
    public TMP_Text turnChange_text;
    private const float turnChange_time = 1.0f;
    private readonly Pair<int, int> turnChange_word_size = new(0, 250);
    private readonly Pair<int, int> turnChange_panel_open = new(270, 270);
    private readonly Pair<int, int> turnChange_panel_close = new(1080, 1080);
    //public IEnumerator TurnChange(ChessColor changeTo)
    //{
    //    whiteActionTag.enabled = false;
    //    blackActionTag.enabled = false;
    //    yield return TurnChangeAnimation(true);
    //    yield return TurnChangeAnimation(false);

    //    nowTurnTag.sprite = _resourcesData.PlayerSprite(changeTo);

    //    bool isWriteTurn = changeTo == ChessColor.White;

    //    whiteActionTag.enabled = isWriteTurn;
    //    blackActionTag.enabled = !isWriteTurn;
    //}

    public async UniTask TurnChange(ChessColor changeTo)
    {
        whiteActionTag.enabled = false;
        blackActionTag.enabled = false;
        await TurnChangeAnimation(true);
        await TurnChangeAnimation(false);

        nowTurnTag.sprite = _resourcesData.PlayerSprite(changeTo);

        bool isWriteTurn = changeTo == ChessColor.White;

        whiteActionTag.enabled = isWriteTurn;
        blackActionTag.enabled = !isWriteTurn;
    }


    //private IEnumerator TurnChangeAnimation(bool isOpen)
    //{
    //    float timer = 0f;

        //    float startBottom = isOpen ? turnChange_panel_close.first : turnChange_panel_open.first;
        //    float endBottom = isOpen ? turnChange_panel_open.first : turnChange_panel_close.first;

        //    float startTop = isOpen ? turnChange_panel_close.second : turnChange_panel_open.second;
        //    float endTop = isOpen ? turnChange_panel_open.second : turnChange_panel_close.second;

        //    float startSize = isOpen ? turnChange_word_size.first : turnChange_word_size.second;
        //    float endSize = isOpen ? turnChange_word_size.second : turnChange_word_size.first;

        //    while (timer < turnChange_time)
        //    {
        //        timer += Time.deltaTime;
        //        float t = Mathf.Clamp01(timer / turnChange_time);

        //        Vector2 offsetMin = turnChange_panel.offsetMin;
        //        offsetMin.y = Mathf.Lerp(startBottom, endBottom, t);

        //        Vector2 offsetMax = turnChange_panel.offsetMax;
        //        offsetMax.y = -Mathf.Lerp(startTop, endTop, t);

        //        turnChange_panel.offsetMin = offsetMin;
        //        turnChange_panel.offsetMax = offsetMax;

        //        turnChange_text.fontSize = Mathf.Lerp(startSize, endSize, t);

        //        yield return null;
        //    }

        //    // Snap to final values
        //    Vector2 min = turnChange_panel.offsetMin;
        //    min.y = endBottom;
        //    turnChange_panel.offsetMin = min;

        //    Vector2 max = turnChange_panel.offsetMax;
        //    max.y = -endTop;
        //    turnChange_panel.offsetMax = max;

        //    turnChange_text.fontSize = endSize;
        //}

    private async UniTask TurnChangeAnimation(bool isOpen)
    {
        float timer = 0f;

        float startBottom = isOpen ? turnChange_panel_close.first : turnChange_panel_open.first;
        float endBottom = isOpen ? turnChange_panel_open.first : turnChange_panel_close.first;

        float startTop = isOpen ? turnChange_panel_close.second : turnChange_panel_open.second;
        float endTop = isOpen ? turnChange_panel_open.second : turnChange_panel_close.second;

        float startSize = isOpen ? turnChange_word_size.first : turnChange_word_size.second;
        float endSize = isOpen ? turnChange_word_size.second : turnChange_word_size.first;

        while (timer < turnChange_time)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / turnChange_time);

            Vector2 offsetMin = turnChange_panel.offsetMin;
            offsetMin.y = Mathf.Lerp(startBottom, endBottom, t);

            Vector2 offsetMax = turnChange_panel.offsetMax;
            offsetMax.y = -Mathf.Lerp(startTop, endTop, t);

            turnChange_panel.offsetMin = offsetMin;
            turnChange_panel.offsetMax = offsetMax;

            turnChange_text.fontSize = Mathf.Lerp(startSize, endSize, t);

            await UniTask.Yield();
        }

        // Snap to final values
        Vector2 min = turnChange_panel.offsetMin;
        min.y = endBottom;
        turnChange_panel.offsetMin = min;

        Vector2 max = turnChange_panel.offsetMax;
        max.y = -endTop;
        turnChange_panel.offsetMax = max;

        turnChange_text.fontSize = endSize;
    }
}
