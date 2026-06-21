using TMPro;
using UnityEngine;

public class PickCardManual : ButtonManual
{
    private ResourcesData _resourcesData => GameManager.Instance.resourcesData;

    [Header("Card's Description ")]
    public TMP_Text descriptionName;
    public MeshRenderer pickCardCover_MeshRenderer;
    public TMP_Text descriptionLevel01;
    public TMP_Text descriptionLevel02;
    public TMP_Text descriptionLevel03;

    public Card pickedCard {  get; private set; }
    public void PickupCard(Card card)
    {
        pickedCard = card;

        CardData cardData = _resourcesData.cardDataDict[pickedCard.buffCard];
        descriptionName.text = cardData.name;
        pickCardCover_MeshRenderer.material = cardData.m_CardCover;
        descriptionLevel01.text = cardData.buffLevel01Description;
        descriptionLevel02.text = cardData.buffLevel02Description;
        descriptionLevel03.text = cardData.buffLevel03Description;

    }

    public override void Return()
    {
        this.gameObject.SetActive(false);
    }

}
