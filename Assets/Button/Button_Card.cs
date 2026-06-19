using UnityEngine;

public class Button_Card : MyButton
{
    public Card card;
    
    public override void OnClick()
    {
        buttonManual.PickTheCard(card);
    }


}