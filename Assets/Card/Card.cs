using UnityEngine;
using CaraData;

public class Card : MonoBehaviour
{
    public AllBuffCara buffCard;
    private AllBuffCara oldBuffCard = AllBuffCara.None;

    public MeshRenderer card_Front;
    public MeshRenderer card_back;

    private void Start()
    {
        CaraData.CaraData.CaraDataInit();
    }

    public void SetCard(AllBuffCara card)
    {
        buffCard = card;
        ChangeTheCard();
    }

    public void ChoseTheCard(Player player)
    {
        if (buffCard == AllBuffCara.None) return;
    }

    private void ChangeTheCard()
    {
        if (buffCard == oldBuffCard) return;

        oldBuffCard = buffCard;
        Pair<Material, Material> materials = CaraData.CaraData.cardMaterialDict[buffCard];

        card_Front.material = materials.first;
        card_back.material = materials.second;
    }


    private void Update()
    {
        ChangeTheCard();
    }




}
