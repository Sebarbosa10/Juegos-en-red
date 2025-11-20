using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class PlayerController : MonoBehaviourPun, IPunObservable
{
    private PlayerModel _playerModel;
    public PlayerModel PlayerModel => _playerModel;

    private PlayerView _playerView;
    private Rigidbody _rb;
    private PlayerWiggle _wiggle;

    private PhotonView _pv;
    private bool _isLocal;

    private float _xRotation = 0f;
    private float _currentYRotation;
    private float _yRotationVelocity;
    private float _xRotationVelocity;

    private bool _isPaused = false;
    private bool _canMove = true;

    private Vector3 _currentVelocity;

    // Network movement sync
    private bool _netIsMoving;

    private void Awake()
    {
        _playerModel = GetComponent<PlayerModel>();
        _playerView = GetComponent<PlayerView>();
        _rb = GetComponent<Rigidbody>();
        _pv = GetComponent<PhotonView>();
        _wiggle = GetComponentInChildren<PlayerWiggle>();

        _isLocal = _pv.IsMine;
    }

    private void Start()
    {
        if (_rb != null)
            _rb.freezeRotation = true;

        if (_playerView != null)
            _playerView.SetLocalCameraActive(_isLocal);

        if (_isLocal)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            if (_rb != null)
                _rb.isKinematic = true;
        }
    }

    private void Update()
    {
        if (_isLocal)
        {
            if (!_canMove) return;
            HandleMouseLook();
            HandleInteraction();
        }
        else
        {
            if (_wiggle != null)
                _wiggle.SetMoving(_netIsMoving);
        }
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
        float playerSpeed = Input.GetKey(KeyCode.LeftShift)
            ? _playerModel.SprintSpeed
            : _playerModel.Speed;

        Vector3 moveDir =
            (transform.right * Input.GetAxis("Horizontal") +
             transform.forward * Input.GetAxis("Vertical")).normalized;

        bool isMoving = moveDir.magnitude > 0.1f;

        if (isMoving)
        {
            _currentVelocity = moveDir * playerSpeed * Time.fixedDeltaTime;
        }
        else
        {
            float friction = _playerModel.IsSlippery ? 0.99f : 0.5f;
            _currentVelocity *= friction;
        }

        if (_wiggle != null)
            _wiggle.SetMoving(isMoving);

        _rb.MovePosition(_rb.position + _currentVelocity);
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
            if (conditional != null && !conditional.CanInteract())
                canShowHand = false;

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
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            bool movingState = _wiggle != null && _wiggle.IsMoving;
            stream.SendNext(movingState);
        }
        else
        {
            _netIsMoving = (bool)stream.ReceiveNext();
        }
    }
}
