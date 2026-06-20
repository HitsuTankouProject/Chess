using UnityEngine;

public class Button_Card : MyButton
{
    public Card card;
    
    public override void OnClick()
    {
        if (buttonManual != null)
        {
            buttonManual.PickTheCard(card);
        }
        else Debug.LogWarning("ButtonManual is not assigned in " + gameObject.name);

    }


}