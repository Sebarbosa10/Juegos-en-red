using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerNickname : MonoBehaviourPun
{
    [SerializeField] private TMP_Text nicknameText;
    private Transform cameraTransform;

    void Start()
    {
        
        if (nicknameText != null)
            nicknameText.text = photonView.Owner.NickName;

        
        if (Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        
        if (cameraTransform != null)
        {
            transform.LookAt(transform.position + cameraTransform.rotation * Vector3.forward,
                             cameraTransform.rotation * Vector3.up);
        }
    }
}
