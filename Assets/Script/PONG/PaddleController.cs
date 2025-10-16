using UnityEngine;
using Photon.Pun;

public class PaddleController : MonoBehaviourPun
{
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _limit = 20f;

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

        Vector3 camUp = _mainCam.transform.up;
        camUp.y = 0; 
        camUp.Normalize();

        transform.position += camUp * (input * _speed * Time.deltaTime);

        Vector3 offset = transform.position - _startPos;
        offset = Vector3.ClampMagnitude(offset, _limit);
        transform.position = _startPos + offset;
    }
}
