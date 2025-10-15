using UnityEngine;
using Photon.Pun;

public class PaddleController : MonoBehaviourPun
{
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _limit = 20f; // límite visible en pantalla

    private Camera _mainCam;
    private Vector3 _startPos;

    private void Start()
    {
        _mainCam = Camera.main;
        _startPos = transform.position;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        float input = Input.GetAxis("Vertical");

        // Dirección visual (según cámara)
        Vector3 camUp = _mainCam.transform.up;
        camUp.y = 0; // evitar altura
        camUp.Normalize();

        transform.position += camUp * (input * _speed * Time.deltaTime);

        // Clamp de posición relativa al punto de spawn
        Vector3 offset = transform.position - _startPos;
        offset = Vector3.ClampMagnitude(offset, _limit);
        transform.position = _startPos + offset;
    }
}
