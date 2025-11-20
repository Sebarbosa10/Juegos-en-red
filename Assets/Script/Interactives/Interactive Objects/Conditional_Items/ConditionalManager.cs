using UnityEngine;

public class ConditionalManager : MonoBehaviour
{
    public static ConditionalManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void HandleInteraction(ConditionalItem item)
    {
        if (GameProgressManager.Instance != null &&
            GameProgressManager.Instance.HasProgressFlag(item.RequiredFlag))
        {
            var anim = item.GetComponent<AnimatedInteractable>();
            if (anim != null)
            {
                AnimatedManager.Instance.HandleInteraction(anim);
            }
        }
       
    }
}
