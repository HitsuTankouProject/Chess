using UnityEngine;

public enum ButtonAction
{
    None = 0,
    Pause, Return, Confirm, 
    StartGame, BackToGameTitle, DrawAgain

}

public abstract class ButtonManual : MonoBehaviour
{
    public virtual void PickTheCard(Card card) { }

    public virtual void Pause() { }
    public virtual void Return() { }
    public virtual void Confirm() { }
    public virtual void StartGame() { }
    public virtual void BackToGameTitle() { }
    
    public virtual void DrawAgain() { }

}