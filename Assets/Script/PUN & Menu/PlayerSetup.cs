using Photon.Pun;
using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
   
    public Camera playerCamera;                
    public bool retagAsMain = true;            
    public bool ensureAudioListener = true;    

    [Header("Local-only scripts")]
    public MonoBehaviour[] localOnlyComponents; 

    void Awake()
    {
        var pv = GetComponent<PhotonView>();
        bool isMine = pv && pv.IsMine;

        
        if (localOnlyComponents != null)
            foreach (var c in localOnlyComponents) if (c) c.enabled = isMine;

       
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(isMine);

            
            if (isMine)
            {
                if (retagAsMain) playerCamera.tag = "MainCamera";

                var listener = playerCamera.GetComponent<AudioListener>();
                if (ensureAudioListener)
                {
                    if (!listener) listener = playerCamera.gameObject.AddComponent<AudioListener>();
                    listener.enabled = true;
                }

               
                var follow = playerCamera.GetComponent<CameraTopDownFollow>();
                if (follow) follow.target = transform;
            }
            else
            {
                playerCamera.tag = "Untagged";
                var listener = playerCamera.GetComponent<AudioListener>();
                if (listener) listener.enabled = false;
            }
        }

        
        DisableSceneCamerasIfLocal(isMine);
    }

    void DisableSceneCamerasIfLocal(bool isMine)
    {
        if (!isMine) return;

        
        var cams = FindObjectsOfType<Camera>();
        foreach (var cam in cams)
        {
            if (cam == playerCamera) continue;
            if (cam.CompareTag("MainCamera"))
            {
                cam.gameObject.SetActive(false);
            }
        }
    }
}
