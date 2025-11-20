using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _sprintSpeed = 10f;

    
    [SerializeField] private float _mouseSensivityX = 10f;
    [SerializeField] private float _mouseSensivityY = 10f;

   
    [SerializeField] private float _interactionDistance = 1f;
    [SerializeField] private LayerMask _interactableLayer = 1 << 8;

    public float Speed => _speed;
    public float SprintSpeed => _sprintSpeed;
    public bool IsSlippery { get; set; } = false;


    public float MouseSensivityX
    {
        get => _mouseSensivityX;
        set => _mouseSensivityX = value;
    }

    public float MouseSensivityY
    {
        get => _mouseSensivityY;
        set => _mouseSensivityY = value;
    }

    public void SetMovementSpeed(float walkSpeed, float sprintSpeed)
    {
        _speed = walkSpeed;
        _sprintSpeed = sprintSpeed;
    }


    private void Awake()
    {

    }

    public RaycastHit? DetectInteractive(Transform cameraTransform)
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, _interactionDistance, _interactableLayer))
        {
            return hit;
        }
        return null;
    }
}