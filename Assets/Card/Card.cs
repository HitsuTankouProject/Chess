using UnityEngine;
using System.Collections;

public class CardData
{
    public string name { get; private set; }

    public Material m_CardCover { get; private set; }
    public string buffLevel01Description { get; private set; }
    public string buffLevel02Description { get; private set; }
    public string buffLevel03Description { get; private set; }

    public CardData(string cardName, Material cover, string level1, string level2, string level3)
    {
        name = cardName;
        m_CardCover = cover;
        buffLevel01Description = level1;
        buffLevel02Description = level2;
        buffLevel03Description = level3;
    }
    public void ChangeLanguage(string cardName, string level1, string level2, string level3)
    {
        name = cardName;
        buffLevel01Description = level1;
        buffLevel02Description = level2;
        buffLevel03Description = level3;
    }
}



public enum CardFace { Front, Back }

public class Card : MonoBehaviour
{
    private GameManager _gameManager => GameManager.Instance;
    private ResourcesData _resourcesData => _gameManager.resourcesData;


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

        card_Front.material = _resourcesData.cardMaterialDict[buffCard];
    }

    private const float cardTurnTime = 0.35f;

    private float FinalFaceTo(CardFace faceTo) => faceTo == CardFace.Front ? 0f : 180f;

    public IEnumerator TurnTheCard(CardFace faceTo)
    {
        float targetY = FinalFaceTo(faceTo);
        float startY = transform.localEulerAngles.y;

        if (Mathf.Abs(Mathf.DeltaAngle(startY, targetY)) < 0.1f)
            yield break;

        float elapsedTime = 0f;

        while (elapsedTime < cardTurnTime)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / cardTurnTime);

            float y = Mathf.LerpAngle(startY, targetY, t);

            transform.localEulerAngles = new Vector3(0, y, 0);

            yield return null;
        }

        transform.localEulerAngles = new Vector3(0, targetY, 0);
    }


}
