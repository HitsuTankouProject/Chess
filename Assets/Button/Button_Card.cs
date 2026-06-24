using UnityEngine;

public class Button_Card : MyButton
{
    public Card card;
    private bool canClick = true;
    public void CanClick(bool click) => canClick = click;
    public override void OnClick()
    {
        if (!canClick || !buttonManual.gameObject.activeSelf) return;
        if (buttonManual != null && buttonManual is PickCardManual pickCardManual)
        {
            pickCardManual.gameObject.SetActive(true);
            pickCardManual.PickupCard(card);
        }


        else Debug.LogWarning("ButtonManual is not assigned in " + gameObject.name);

    }


}