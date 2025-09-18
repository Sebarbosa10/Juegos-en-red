using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;


public class LobbyReadyButton : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button readyButton;
    [SerializeField] private GameObject readyIndicator;
    [SerializeField] private KeyCode readyKey = KeyCode.R; // ✅ Nueva línea

    private const string ReadyKey = "ready";
    private bool isReady = false;

    void Start()
    {
        if (readyButton != null)
            readyButton.onClick.AddListener(SetReady);

        RefreshUI();
    }

    void Update()
    {
        if (!PhotonNetwork.InRoom) return;
        if (isReady) return; // ya está listo

        // ✅ También escucha la tecla
        if (Input.GetKeyDown(readyKey))
        {
            SetReady();
        }
    }

    private void SetReady()
    {
        var props = new PhotonHashtable { { ReadyKey, true } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        isReady = true;
        RefreshUI();

        Debug.Log($"[Lobby] {PhotonNetwork.NickName} está listo.");
    }

    private void RefreshUI()
    {
        if (readyButton != null) readyButton.interactable = !isReady;
        if (readyIndicator != null) readyIndicator.SetActive(isReady);
    }
}
