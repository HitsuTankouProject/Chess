using UnityEngine;

public class Button_Card : MyButton
{
    public Card card;

    public override void OnClick()
    {
        if (buttonManual != null && buttonManual is PickCardManual pickCardManual)
        {
            pickCardManual.gameObject.SetActive(true);
            pickCardManual.PickupCard(card);
        }


        else Debug.LogWarning("ButtonManual is not assigned in " + gameObject.name);

    }


}