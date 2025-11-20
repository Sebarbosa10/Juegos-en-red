using UnityEngine;
using UnityEngine.Events;

public class LightReceiver : MonoBehaviour
{
    public enum ReceiverRole { None, GateOpener, FinalDoor }

    
    public bool isLit;
    public bool latchOn = false;              
    public bool requireContinuous = true;   

    
    public ReceiverRole role = ReceiverRole.None;

    
    public AnimatedInteractable gateMover;    
    public bool forceOpen = true;             
    public bool revertOnUnlit = false;        

    
    public GameObject objectToEnable;
    public GameObject objectToDisable;

    
    public VerticalGate verticalGate;

    public float lastLitTime { get; private set; }

    
    public UnityEvent onLit;
    public UnityEvent onUnlit;

    public void SetLit(bool lit)
    {
        if (latchOn && isLit) return;
        if (isLit == lit) return;

        isLit = lit;

        if (lit)
        {
            lastLitTime = Time.time;
            onLit?.Invoke();

            switch (role)
            {
                case ReceiverRole.GateOpener:
                    if (gateMover)
                    {
                        if (forceOpen)
                        {
                            AnimatedManager.Instance.ResetAnimatedToInitial(gateMover, markClosed: false);
                            AnimatedManager.Instance.HandleInteraction(gateMover);
                            gateMover.isActive = true;
                        }
                        else
                        {
                            gateMover.Interact();
                        }
                    }

                    if (objectToEnable) objectToEnable.SetActive(true);
                    if (objectToDisable) objectToDisable.SetActive(false);
                    break;

                case ReceiverRole.FinalDoor:
                    if (verticalGate) verticalGate.Open();
                    break;
            }

            if (latchOn) return;
        }
        else
        {
            if (!requireContinuous) return;

            onUnlit?.Invoke();

            switch (role)
            {
                case ReceiverRole.GateOpener:
                    if (revertOnUnlit)
                    {
                        if (gateMover)
                            AnimatedManager.Instance.CancelAndSnapToInitial(gateMover, markClosed: true);

                        if (objectToEnable) objectToEnable.SetActive(false);
                        if (objectToDisable) objectToDisable.SetActive(true);
                    }
                    break;

                case ReceiverRole.FinalDoor:
                    break;
            }
        }
    }
}
