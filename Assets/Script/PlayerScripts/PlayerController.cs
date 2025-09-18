using UnityEngine;
using Photon.Pun;


[RequireComponent(typeof(PhotonView))]
public class PlayerController : MonoBehaviour
{
    private PlayerModel _playerModel;
    public PlayerModel PlayerModel => _playerModel;

    private PlayerView _playerView;
    private Rigidbody _rb;

    private PhotonView _pv;           
    private bool _isLocal;           

    private float _xRotation = 0f;
    private float _currentYRotation;
    private float _yRotationVelocity;
    private float _xRotationVelocity;

    private bool _isPaused = false;
    private bool _canMove = true;

    private void Awake()
    {
        _playerModel = GetComponent<PlayerModel>();
        _playerView = GetComponent<PlayerView>();
        _rb = GetComponent<Rigidbody>();
        _pv = GetComponent<PhotonView>();

        _isLocal = (_pv == null) ? true : _pv.IsMine;
    }

    private void Start()
    {
        if (_rb != null) _rb.freezeRotation = true;

        // ⚡ Garantizar que la cámara se active correctamente en el local
        if (_playerView != null)
            _playerView.SetLocalCameraActive(_isLocal);

        if (_isLocal)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            if (_rb != null) _rb.isKinematic = true;
        }
    }


    private void Update()
    {
        if (!_isLocal) return;    
        if (!_canMove) return;

        HandleMouseLook();
        HandleInteraction();
    }

    private void FixedUpdate()
    {
        if (!_isLocal) return;     
        if (!_canMove) return;

        HandleMovement();
    }

    private void HandleMouseLook()
    {
        if (_isPaused || !_canMove) return;

        float mouseX = Input.GetAxis("Mouse X") * _playerModel.MouseSensivityX;
        float mouseY = Input.GetAxis("Mouse Y") * _playerModel.MouseSensivityY;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        float smoothX = Mathf.SmoothDampAngle(
            _playerView.playerCamera.transform.localEulerAngles.x,
            _xRotation,
            ref _xRotationVelocity,
            0.05f);
        if (float.IsNaN(smoothX)) smoothX = 0f;
        _playerView.RotateCamera(smoothX);

        _currentYRotation += mouseX;
        float smoothY = Mathf.SmoothDampAngle(
            _playerView.transform.eulerAngles.y,
            _currentYRotation,
            ref _yRotationVelocity,
            0.05f);
        if (float.IsNaN(smoothY)) smoothY = 0f;
        _playerView.transform.rotation = Quaternion.Euler(0f, smoothY, 0f);
    }

    private void HandleMovement()
    {
        float playerSpeed = Input.GetKey(KeyCode.LeftShift) ? _playerModel.SprintSpeed : _playerModel.Speed;
        float x = Input.GetAxis("Horizontal") * playerSpeed * Time.fixedDeltaTime;
        float z = Input.GetAxis("Vertical") * playerSpeed * Time.fixedDeltaTime;

        Vector3 move = transform.right * x + transform.forward * z;
        _rb.MovePosition(_rb.position + move);
    }

    private void HandleInteraction()
    {
        var cam = _playerView.playerCamera != null ? _playerView.playerCamera.transform : null;
        if (cam == null) return;

        var hit = _playerModel.DetectInteractive(cam);

        if (hit.HasValue)
        {
            IInteractive interactive = hit.Value.collider.GetComponent<IInteractive>();

            bool canShowHand = true;
            var conditional = hit.Value.collider.GetComponent<ConditionalItem>();
            if (conditional != null && !conditional.CanInteract()) canShowHand = false;

            _playerView.ShowHandIcon(canShowHand);

            if (canShowHand && Input.GetMouseButtonDown(0) && interactive != null)
            {

                interactive.Interact();
                _playerView.ShowHandIcon(false);
            }
        }
        else
        {
            _playerView.ShowHandIcon(false);
        }
    }

    public void SetPaused(bool isPaused)
    {
        if (!_isLocal) return;

        _isPaused = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;

        if (!isPaused)
        {
            _xRotation = _playerView.playerCamera.transform.localEulerAngles.x;
            _currentYRotation = _playerView.transform.eulerAngles.y;
        }
    }

    public void SetCanMove(bool canMove)
    {
        if (!_isLocal) return;

        _canMove = canMove;
        Cursor.lockState = canMove ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !canMove;

        if (canMove)
        {
            _xRotation = _playerView.playerCamera.transform.localEulerAngles.x;
            _currentYRotation = _playerView.transform.eulerAngles.y;
        }
    }
}
