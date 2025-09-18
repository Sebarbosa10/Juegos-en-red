using UnityEngine;

public class SarcophagusLid : MonoBehaviour, IInteractive
{
    [Header("Refs")]
    public AnimatedInteractable animatedLid;
    public SarcophagusPuzzle puzzle;
    public int index;

    private bool locked = false;

    public void Lock() { locked = true; }
    public void Unlock() { locked = false; }

    public void SnapToInitial()
    {
        if (animatedLid != null)
            AnimatedManager.Instance.CancelAndSnapToInitial(animatedLid, true);
    }

    public void AnimateBackToInitial()
    {
        if (animatedLid != null)
            AnimatedManager.Instance.ResetAnimatedToInitial(animatedLid, true);
    }

    public void Interact()
    {

        if (locked) return;
        if (puzzle != null && puzzle.IsBusy) return;


        var net = FindObjectOfType<SarcophagusPuzzleNetSync>();
        if (net != null)
        {
            net.RequestClick(index);
            return;
        }


        if (animatedLid != null && AnimatedManager.Instance.IsAnimating(animatedLid)) return;
        if (puzzle != null && !puzzle.TryReserveIndex(index)) return;

        animatedLid?.Interact();
        puzzle?.OnLidClicked(index);
        locked = true;
    }
}
