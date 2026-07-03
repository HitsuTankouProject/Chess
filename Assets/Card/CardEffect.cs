using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CardEffect : MonoBehaviour
{
    private ResourcesData _resourcesData => GameManager.Instance.resourcesData;

    //private bool isEffectActive =>
    //    _inGame.inGameStage == InGameStage.TurnStart
    //    || _inGame.inGameStage == InGameStage.TurnChanging;

    [Header("Card Settings")]
    public Card card;
    public AllBuffCard buffCard;
    public uint cardLevel = 1;

    private float rotateSpeed = 90;
    private float floatHeight = 0.25f;
    private float floatSpeed = 2f;
    private Vector3 cardStartPos;

    [Header("Chess Settings")]
    public MeshFilter chessMeshFilter;

    public void CardEffectInit(AllBuffCard targetBuff, uint level)
    {
        if(targetBuff == AllBuffCard.None)
        {
            card.gameObject.SetActive(false);
            chessMeshFilter.gameObject.SetActive(false);
        }

        buffCard = targetBuff;
        cardLevel = level;
        card.SetCard(buffCard);
        cardStartPos = card.transform.localPosition;


    }
    private void Effect()
    {
        //if(!isEffectActive) return;

        float time = Time.time;
        card.gameObject.transform.localPosition = cardStartPos + Vector3.up * Mathf.Sin(time * floatSpeed) * floatHeight;
        card.gameObject.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

    }

    private void Start()
    {
        CardEffectInit(buffCard, 1);
    }

    private void Update() => Effect();
}


