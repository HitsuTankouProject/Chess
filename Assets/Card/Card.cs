using UnityEngine;

public enum CardFace { Front, Back }

public class Card : MonoBehaviour
{


    private GameManager _gameManager => GameManager.Instance;

    public AllBuffCard buffCard;
    private AllBuffCard oldBuffCard = AllBuffCard.None;

    public MeshRenderer card_Front;
    public MeshRenderer card_back;

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
        Pair<Material, Material> materials = _gameManager.CardMaterials(buffCard);

        card_Front.material = materials.first;
        card_back.material = materials.second;
    }

    
    private Vector3 RotateAngle() { return Vector3.one; }



    private void Update()
    {
        ChangeTheCard();
    }




}
