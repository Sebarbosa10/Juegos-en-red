using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PongLauncher : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button connectButton;

    [Header("Config")]
    [SerializeField] private string pongSceneName = "PongScene";
    [SerializeField] private byte maxPlayers = 2;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1.0";

        connectButton.onClick.AddListener(OnConnectClicked);

        connectButton.interactable = false;
        nicknameInput.onValueChanged.AddListener(name =>
        {
            connectButton.interactable = !string.IsNullOrWhiteSpace(name);
        });
    }

    private void OnConnectClicked()
    {
        PhotonNetwork.NickName = nicknameInput.text;
        PhotonNetwork.ConnectUsingSettings();
        connectButton.interactable = false;
    }

    public override void OnConnectedToMaster()
    {
        var opts = new RoomOptions { MaxPlayers = maxPlayers };
        PhotonNetwork.JoinOrCreateRoom("PongRoom", opts, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {


        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(pongSceneName);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {


        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers)
        {
            // Sincroniza el nivel automáticamente
            PhotonNetwork.LoadLevel(pongSceneName);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {

        connectButton.interactable = true;
    }
}
