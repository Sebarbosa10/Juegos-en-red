using UnityEngine;

public class SarcophagusLid : MonoBehaviour, IInteractive
{
    [Header("Refs")]
    public AnimatedInteractable animatedLid;
    public SarcophagusPuzzle puzzle;
    public int index;

    [SerializeField] private SarcophagusPuzzleNetSync net;

    private bool locked = false;

    private void Awake()
    {
        if (!puzzle) puzzle = GetComponentInParent<SarcophagusPuzzle>();
        if (!net) net = GetComponentInParent<SarcophagusPuzzleNetSync>();
    }

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
