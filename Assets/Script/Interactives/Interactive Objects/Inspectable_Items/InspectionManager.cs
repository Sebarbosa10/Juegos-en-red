using UnityEngine;

public class InspectionManager : MonoBehaviour
{
    public static InspectionManager Instance { get; private set; }

    [Header("Inspection Settings")]
    [SerializeField] private Transform inspectionAnchor; // in front of camera
    [SerializeField] private float rotationSpeed = 100f;

    private InspectableItem _currentItem; // Item being inspected
    private PlayerController _player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _player = Object.FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        if (_currentItem != null)
        {
            RotateItem();

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
                EndInspection();
        }
    }

    public void StartInspection(InspectableItem item)
    {
        if (_currentItem != null) return; // already one being inspected

        _currentItem = item;
        _currentItem.StartInspection(inspectionAnchor);

        if (_player != null)
            _player.SetCanMove(false);
    }

    public void EndInspection()
    {
        if (_currentItem == null) return;

        _currentItem.EndInspection();

        if (_player != null)
            _player.SetCanMove(true);

        _currentItem = null;
    }

    private void RotateItem()
    {
        float rotX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        float rotY = -Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        _currentItem.transform.Rotate(Camera.main.transform.up, rotX, Space.World);
        _currentItem.transform.Rotate(Camera.main.transform.right, rotY, Space.World);
    }

}
