using Photon.Pun;
using UnityEngine;

public class PlayerView : MonoBehaviourPun
{
    public Camera playerCamera;
    public GameObject handIcon;

    void Awake()
    {
        // ⚡ Siempre asegúrate de que la cámara arranque desactivada
        if (playerCamera != null)
        {
            playerCamera.enabled = false;
            var al = playerCamera.GetComponent<AudioListener>();
            if (al != null) al.enabled = false;
        }
    }

    void Start()
    {
        // Activo cámara solo si es mi Player
        SetLocalCameraActive(photonView.IsMine);
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
        if (playerCamera != null)
        {
            playerCamera.enabled = isLocal;

            var al = playerCamera.GetComponent<AudioListener>();
            if (al != null) al.enabled = isLocal;
        }

        if (!isLocal && handIcon != null) handIcon.SetActive(false);
    }
}
