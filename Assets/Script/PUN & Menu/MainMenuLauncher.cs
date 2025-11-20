using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MainMenuLauncher : MonoBehaviourPunCallbacks
{
   
    public TMP_InputField nicknameInput;
    public Button connectButton;

   
    public GameObject roomsPanel;

    
    public TMP_InputField roomNameInput;
    public Button createRoomButton;

    
    [SerializeField] private string lobbySceneName = "Lobby";
    [SerializeField] private byte maxPlayers = 4;

    private const string NickKey = "playerNickname";

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        

        connectButton.onClick.AddListener(Connect);
        createRoomButton.onClick.AddListener(CreateRoom);

        
        if (PlayerPrefs.HasKey(NickKey))
            nicknameInput.text = PlayerPrefs.GetString(NickKey);

        connectButton.interactable = !string.IsNullOrWhiteSpace(nicknameInput.text);
        nicknameInput.onValueChanged.AddListener(n =>
        {
            connectButton.interactable = !string.IsNullOrWhiteSpace(n);
        });

        if (roomsPanel) roomsPanel.SetActive(false);

        
        if (PhotonNetwork.IsConnectedAndReady)
        {
            
            PhotonNetwork.JoinLobby(TypedLobby.Default);
            connectButton.interactable = false;
        }
    }

    void Connect()
    {
        var nick = nicknameInput.text?.Trim();
        if (string.IsNullOrWhiteSpace(nick)) return;

        PlayerPrefs.SetString(NickKey, nick);
        PhotonNetwork.NickName = nick.ToUpper();

        connectButton.interactable = false;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        bool ok = PhotonNetwork.JoinLobby(TypedLobby.Default);
        if (!ok)
            connectButton.interactable = true;
    }

    public override void OnJoinedLobby()
    {
        if (roomsPanel) roomsPanel.SetActive(true);
    }

    public override void OnLeftLobby()
    {
        if (roomsPanel) roomsPanel.SetActive(false);
    }

    private void CreateRoom()
    {
        string rn = roomNameInput != null ? roomNameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(rn))
            rn = $"Lobby-{Random.Range(10000, 99999)}";

        var opts = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            IsOpen = true,
            IsVisible = true
        };

        PhotonNetwork.CreateRoom(rn, opts, TypedLobby.Default);
    }

    public void JoinRoom(string roomName)
    {
        if (string.IsNullOrEmpty(roomName)) return;
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        connectButton.interactable = true;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        connectButton.interactable = true;
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel(lobbySceneName);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
      
        connectButton.interactable = true;

        if (roomsPanel)
            roomsPanel.SetActive(false);
    }
}
