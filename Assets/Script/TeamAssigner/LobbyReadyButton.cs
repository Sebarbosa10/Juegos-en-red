using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyReadyButton : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button readyButton;
    [SerializeField] private GameObject readyIndicator;

    [SerializeField] private KeyCode readyKey = KeyCode.R;

    private const string ReadyKeyProp = "ready";
    private bool isReady = false;

    void Start()
    {
        // Siempre arrancar NO listo al entrar a la lobby
        if (PhotonNetwork.InRoom)
        {
            var props = new PhotonHashtable { { ReadyKeyProp, false } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            isReady = false;
        }
        else if (PhotonNetwork.LocalPlayer.CustomProperties != null &&
                 PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(ReadyKeyProp))
        {
            isReady = (bool)PhotonNetwork.LocalPlayer.CustomProperties[ReadyKeyProp];
        }

        if (readyButton != null)
            readyButton.onClick.AddListener(SetReady);

        RefreshUI();
    }

    void Update()
    {
        if (!PhotonNetwork.InRoom) return;
        if (isReady) return;

        if (Input.GetKeyDown(readyKey))
        {
            SetReady();
        }
    }

    private void SetReady()
    {
        var props = new PhotonHashtable { { ReadyKeyProp, true } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        isReady = true;
        RefreshUI();

        Debug.Log($"[Lobby] {PhotonNetwork.NickName} está listo (Ready).");
    }

    private void RefreshUI()
    {
        if (readyButton != null) readyButton.interactable = !isReady;
        if (readyIndicator != null) readyIndicator.SetActive(isReady);
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player target, PhotonHashtable changedProps)
    {
        if (changedProps == null) return;
        if (!changedProps.ContainsKey(ReadyKeyProp)) return;

        if (target.IsLocal)
        {
            isReady = (bool)changedProps[ReadyKeyProp];
            RefreshUI();
        }
    }
}
