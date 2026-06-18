using UnityEngine;

public class Button_Return : MyButton
{
    public override void OnClick()
    {
        if(buttonManual != null)
        {
            buttonManual.Return();
        }
        else Debug.LogWarning("ButtonManual is not assigned in " + gameObject.name);
    }



}
