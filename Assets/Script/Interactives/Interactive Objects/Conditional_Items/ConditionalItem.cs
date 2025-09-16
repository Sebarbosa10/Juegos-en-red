using UnityEngine;

public class ConditionalItem : MonoBehaviour, IInteractive
{
    [SerializeField] private string requiredFlag;

    public string RequiredFlag => requiredFlag;

    /// <summary>
    /// Check if this item can currently be interacted with
    /// </summary>
    public bool CanInteract()
    {
        // Si no hay flag asignado → siempre interactuable
        if (string.IsNullOrEmpty(requiredFlag))
            return true;

        // Si hay flag → depende del progreso
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
            Debug.LogError("ConditionalManager no encontrado en escena.");
        }
    }
}
