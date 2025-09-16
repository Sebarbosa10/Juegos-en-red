using UnityEngine;

public enum AnimatedMode { Toggle, OneShot, Incremental }
public enum AnimatedType { Rotation, Translation }

public class AnimatedInteractable : MonoBehaviour, IInteractive
{
    [Header("Animation Settings")]
    public AnimatedMode mode = AnimatedMode.Toggle;   // Toggle, OneShot, Incremental
    public AnimatedType type = AnimatedType.Rotation; // Rotation or Translation
    public Vector3 axis = Vector3.up;                 // Axis of movement
    public float amount = 90f;                        // Degrees or units
    public float speed = 2f;                          // Movement speed

    // Internal state
    [HideInInspector] public bool isActive = false;   // Current state (for Toggle)
    [HideInInspector] public int stepCount = 0;       // Steps done (for Incremental)

    public void Interact()
    {
        AnimatedManager.Instance.HandleInteraction(this);
    }
}
