using UnityEngine;

public class Card : MonoBehaviour
{
    public AllBuffCard buffCard;
    private AllBuffCard oldBuffCard = AllBuffCard.None;

    public MeshRenderer card_Front;
    public MeshRenderer card_back;

    protected virtual void OnValidate()
    {
        int layer = LayerMask.NameToLayer("Card");

        if (layer != -1) gameObject.layer = layer;
    }

    public void SetCard(AllBuffCard card)
    {
        buffCard = card;
        ChangeTheCard();
    }

    public void ChoseTheCard(Player player)
    {
        if (buffCard == AllBuffCard.None) return;
    }

    private void ChangeTheCard()
    {
        if (buffCard == oldBuffCard) return;

        oldBuffCard = buffCard;
        Pair<Material, Material> materials = ResourcesData.Instance.cardMaterialDict[buffCard];

        card_Front.material = materials.first;
        card_back.material = materials.second;
    }


    private void Update()
    {
        //ChangeTheCard();
    }




}
