using UnityEngine;

public class PauseManual : ButtonManual
{

    public override void Pause()
    {

        Debug.Log("PauseManual: Pause");
        gameObject.SetActive(!gameObject.activeSelf);

    }
    public override void Return()
    {
        Debug.Log("PauseManual: Return");
    }


}
