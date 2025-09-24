using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MainMenuLauncher : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public TMP_InputField inputField;
    public Button connectButton;

    [Header("Scenes")]
    [SerializeField] private string lobbySceneName = "Lobby"; // Lobby
    [SerializeField] private string roomName = "Lobby";  //EgyptLobby
    [SerializeField] private byte maxPlayers = 4;

    private string nickname;
    private const string nicknameKey = "playerNickname";

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "0.1";

        connectButton.onClick.AddListener(Connect);
        inputField.onValueChanged.AddListener(n =>
        {
            nickname = n;
            connectButton.interactable = !string.IsNullOrWhiteSpace(n);
        });

        if (PlayerPrefs.HasKey(nicknameKey))
        {
            inputField.text = PlayerPrefs.GetString(nicknameKey);
            nickname = inputField.text;
            connectButton.interactable = !string.IsNullOrWhiteSpace(nickname);
        }
    }

    void Connect()
    {
        if (string.IsNullOrWhiteSpace(nickname)) return;

        PlayerPrefs.SetString(nicknameKey, nickname);
        PhotonNetwork.NickName = nickname.ToUpper();

        connectButton.interactable = false;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[MainMenu] Conectado al servidor. Intentando entrar a sala...");

        var opts = new RoomOptions { MaxPlayers = maxPlayers, IsOpen = true, IsVisible = true };
        PhotonNetwork.JoinOrCreateRoom(roomName, opts, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[MainMenu] Entré a sala {PhotonNetwork.CurrentRoom.Name} ({PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers})");

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(lobbySceneName);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        connectButton.interactable = true;
        Debug.LogWarning($"[MainMenu] Desconectado: {cause}");
    }
}

