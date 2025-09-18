using UnityEngine;

public class VerticalGate : MonoBehaviour
{
    [Header("ANIMATED INTERACTIBLE")]
    public AnimatedInteractable mover; 

    [Tooltip("Gate")]
    public bool forceOpen = true;

    public void Open()
    {
        if (mover == null) return;
        if (forceOpen)
        {
 
            AnimatedManager.Instance.ResetAnimatedToInitial(mover, markClosed: false); 
            AnimatedManager.Instance.HandleInteraction(mover); 
            mover.isActive = true;
        }
        else
        {
            mover.Interact(); 
        }
    }

    public void Close()
    {
        if (mover == null) return;
        AnimatedManager.Instance.CancelAndSnapToInitial(mover, markClosed: true);
    }
}
