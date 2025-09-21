using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;


public class LobbyReadyButton : MonoBehaviourPunCallbacks
{
    
    [SerializeField] private Button readyButton;
    [SerializeField] private GameObject readyIndicator;

   
    [SerializeField] private KeyCode readyKey = KeyCode.R;

    private const string ReadyKey = "ready";
    private bool isReady = false;

    void Start()
    {
        if (readyButton != null)
            readyButton.onClick.AddListener(SetReady);

        if (PhotonNetwork.LocalPlayer.CustomProperties != null &&
            PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(ReadyKey))
        {
            isReady = (bool)PhotonNetwork.LocalPlayer.CustomProperties[ReadyKey];
        }

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
        var props = new PhotonHashtable { { ReadyKey, true } };
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

    private void CheckAllReady()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.ContainsKey(ReadyKey) || !(bool)player.CustomProperties[ReadyKey])
            {
                return; // alguien no está listo todavía
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[Lobby] Todos listos. Repartiendo cartas...");
            var cardManager = FindObjectOfType<CardManagerPhoton>();
            if (cardManager != null)
            {
                cardManager.DealCards();
            }
        }
    }



    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player target, PhotonHashtable changedProps)
    {
        if (changedProps == null) return;
        if (!changedProps.ContainsKey(ReadyKey)) return;

        if (target.IsLocal)
        {
            isReady = (bool)changedProps[ReadyKey];
            RefreshUI();
        }

        // siempre chequeamos, así el Master detecta cuando todos están listos
        CheckAllReady();
    }

}
