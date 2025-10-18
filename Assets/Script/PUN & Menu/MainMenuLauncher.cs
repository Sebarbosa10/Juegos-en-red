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

    [Header("Photon")]
    [SerializeField] private string lobbySceneName = "Lobby";     
    [SerializeField] private string roomName = "Lobby";    
    [SerializeField] private byte maxPlayers = 4;

    private string nickname;
    private const string nicknameKey = "playerNickname";

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "0.1";

        
        connectButton.onClick.AddListener(Connect);
        inputField.onValueChanged.AddListener(OnNicknameChanged);

        if (PlayerPrefs.HasKey(nicknameKey))
        {
            inputField.text = PlayerPrefs.GetString(nicknameKey);
            nickname = inputField.text;
        }
        connectButton.interactable = !string.IsNullOrWhiteSpace(nickname);
    }

    void OnDestroy()
    {
        
        connectButton.onClick.RemoveListener(Connect);
        inputField.onValueChanged.RemoveListener(OnNicknameChanged);
    }

    private void OnNicknameChanged(string n)
    {
        nickname = (n ?? "").Trim();
        connectButton.interactable = !string.IsNullOrWhiteSpace(nickname);
    }

    void Connect()
    {
        nickname = (nickname ?? "").Trim();
        if (string.IsNullOrWhiteSpace(nickname)) return;

        
        if (nickname.Length > 16) nickname = nickname.Substring(0, 16);

        PlayerPrefs.SetString(nicknameKey, nickname);
        PhotonNetwork.NickName = nickname.ToUpperInvariant();

        connectButton.interactable = false;
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("[MainMenu] Conectando a Photon…");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[MainMenu] Conectado a Master. Intentando entrar/crear sala…");
        var opts = new RoomOptions { MaxPlayers = maxPlayers, IsOpen = true, IsVisible = true };
        PhotonNetwork.JoinOrCreateRoom(roomName, opts, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[MainMenu] Entré a sala {PhotonNetwork.CurrentRoom.Name} " +
                  $"({PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers})");

        if (PhotonNetwork.IsMasterClient)
        {
            
            PhotonNetwork.LoadLevel(lobbySceneName);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        connectButton.interactable = true;
        Debug.LogError($"[MainMenu] JoinRoom FAILED ({returnCode}): {message}");
        
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        connectButton.interactable = true;
        Debug.LogError($"[MainMenu] CreateRoom FAILED ({returnCode}): {message}");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        connectButton.interactable = true;
        Debug.LogWarning($"[MainMenu] Desconectado: {cause}");
    }
}
