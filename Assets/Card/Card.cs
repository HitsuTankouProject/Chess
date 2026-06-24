using UnityEngine;
using System.Collections;




public enum CardFace { Front, Back }

public class Card : MonoBehaviour
{
    private GameManager _gameManager => GameManager.Instance;
    private ResourcesData _resourcesData => _gameManager.resourcesData;

    public AllBuffCard buffCard;

    public MeshRenderer card_Front;
    public MeshRenderer card_back;
    public MeshRenderer card_effectPrt;


    public void SetCard(AllBuffCard card, bool isOpenEffect = false, uint effectLevel = 0)
    {
        buffCard = card;
        card_Front.material = _resourcesData.cardDataDict[buffCard].m_CardCover;
        if (!isOpenEffect) card_effectPrt.enabled = false;
        else
        {
            card_effectPrt.enabled = true;
            //card_effectPrt.material=
        }


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
