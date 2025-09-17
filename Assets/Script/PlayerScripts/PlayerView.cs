using Photon.Pun;
using UnityEngine;

public class PlayerView : MonoBehaviourPun
{
    public Camera playerCamera;
    public GameObject handIcon;


    void Start()
    {
        // Solo activo mi cámara y mi handIcon si el jugador es local
        if (photonView.IsMine)
        {
            SetLocalCameraActive(true);
        }
        else
        {
            SetLocalCameraActive(false);
        }
    }

    public void Move(Vector3 movement)
    {
        transform.Translate(movement);
    }

    public void Rotate(float mouseX)
    {
        transform.Rotate(Vector3.up * mouseX);
    }

    public void RotateCamera(float xRotation)
    {
        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    public void ShowHandIcon(bool show)
    {
        if (handIcon != null) handIcon.SetActive(show);
    }


    public void SetLocalCameraActive(bool isLocal)
    {
        if (playerCamera != null) playerCamera.enabled = isLocal;

        var al = playerCamera != null ? playerCamera.GetComponent<AudioListener>() : null;
        if (al != null) al.enabled = isLocal;

        if (!isLocal && handIcon != null) handIcon.SetActive(false);
    }
}
