using UnityEngine;

public enum AnimatedMode { Toggle, OneShot, Incremental }
public enum AnimatedType { Rotation, Translation }

public class AnimatedInteractable : MonoBehaviour, IInteractive
{
    
    public AnimatedMode mode = AnimatedMode.Toggle;
    public AnimatedType type = AnimatedType.Rotation;
    public Vector3 axis = Vector3.up;
    public float amount = 90f;
    public float speed = 2f;

    [HideInInspector] public bool isActive = false;
    [HideInInspector] public int stepCount = 0;

    [HideInInspector] public Vector3 initialLocalPos;
    [HideInInspector] public Quaternion initialLocalRot;

    private void Awake()
    {
        initialLocalPos = transform.localPosition;
        initialLocalRot = transform.localRotation;
    }

    public void Interact()
    {
        var net = GetComponent<AnimatedNetSync>();
        if (net != null)
            net.InteractNetworked();
        else
            AnimatedManager.Instance.HandleInteraction(this);
    }
}
