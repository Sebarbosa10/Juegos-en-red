using Photon.Pun;
using UnityEngine;



public class PlayerView : MonoBehaviourPun
{
  
    public Camera playerCamera;
    public GameObject handIcon;

    void Awake()
    {
   
        if (playerCamera != null)
        {
            playerCamera.enabled = false;

            var audioListener = playerCamera.GetComponent<AudioListener>();
            if (audioListener != null)
                audioListener.enabled = false;
        }
    }

    void Start()
    {

        SetLocalCameraActive(photonView.IsMine);
    }

    public void RotateCamera(float xRotation)
    {
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    public void ShowHandIcon(bool show)
    {
        if (handIcon != null)
            handIcon.SetActive(show);
    }

    public void SetLocalCameraActive(bool isLocal)
    {
        if (playerCamera != null)
        {
            playerCamera.enabled = isLocal;

            var audioListener = playerCamera.GetComponent<AudioListener>();
            if (audioListener != null)
                audioListener.enabled = isLocal;
        }

        if (!isLocal && handIcon != null)
        {
            handIcon.SetActive(false);
        }
    }
}
