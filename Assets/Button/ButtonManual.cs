using UnityEngine;

public abstract class ButtonManual : MonoBehaviour
{
    public virtual void PickTheCard(Card card) { }

    public virtual void Pause() { }
    public virtual void Return() { }
    public virtual void Conform() { }
    public virtual void StartGame() { }
    public virtual void BackToGameTitle() { }
    
    public virtual void DrawAgain() { }

}