using UnityEngine;

public class ConditionalItem : MonoBehaviour, IInteractive
{
    [SerializeField] private string requiredFlag;

    public string RequiredFlag => requiredFlag;


    public bool CanInteract()
    {
  
        if (string.IsNullOrEmpty(requiredFlag))
            return true;


        return GameProgressManager.Instance != null &&
               GameProgressManager.Instance.HasProgressFlag(requiredFlag);
    }

    public void Interact()
    {
        if (ConditionalManager.Instance != null)
        {
            ConditionalManager.Instance.HandleInteraction(this);
        }
        else
        {
            
        }
    }
}
