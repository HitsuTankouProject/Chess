using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider))]
public abstract class MyButton : MonoBehaviour
{
    protected virtual void OnValidate()
    {
        int layer = LayerMask.NameToLayer("Button");

        if (layer != -1) gameObject.layer = layer;
    }

    public ButtonManual buttonManual;
    public abstract void OnClick();


}
