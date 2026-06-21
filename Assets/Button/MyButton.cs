using UnityEngine;
using System.Collections.Generic;
using System.Collections;


[ExecuteAlways]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(SpriteRenderer))]

public class MyButton : MonoBehaviour
{
    protected virtual void OnValidate()
    {
        int layer = LayerMask.NameToLayer("Button");

        if (layer != -1) gameObject.layer = layer;

        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        spriteRenderer.color = c_NonClick;
    }

    public ButtonManual buttonManual;
    public ButtonAction buttonAction;
    public SpriteRenderer spriteRenderer;

    private const float clickEffectTime = 0.05f;
    private readonly Color c_NonClick = Color.white;
    private readonly Color c_Click = new Color(0.5f, 0.5f, 0.5f);
    private IEnumerator ButtonEffect()
    {
        spriteRenderer.color = c_Click;
        yield return new WaitForSeconds(clickEffectTime);
        spriteRenderer.color = c_NonClick;
    }
    public virtual void OnClick()
    {
        if (!enabled) return;
        StartCoroutine(ButtonEffect());
        if (buttonManual == null) 
            Debug.LogWarning("ButtonManual is not assigned in " + gameObject.name);

        switch (buttonAction)
        {
            case ButtonAction.Pause: buttonManual.Pause();return;
            case ButtonAction.Return:buttonManual.Return(); return;
            case ButtonAction.Confirm: buttonManual.Confirm(); return;
            case ButtonAction.StartGame:buttonManual.StartGame(); return;
            case ButtonAction.BackToGameTitle:buttonManual.BackToGameTitle(); return;
            case ButtonAction.DrawAgain:buttonManual.DrawAgain(); return;

            default:break;
        }

        Debug.LogWarning("No anyFuntion" + gameObject.name);


    }


}
