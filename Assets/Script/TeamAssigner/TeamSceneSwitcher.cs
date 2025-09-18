using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class TeamSceneSwitcher : MonoBehaviourPunCallbacks
{
    [Header("Map scenes")]
    [SerializeField] private string blueScene = "EgyptMapBlue";
    [SerializeField] private string redScene = "EgyptMapRed";

    private const string TeamKey = "team";
    private const string TeamRoomKey = "teamRoom";
    private const string MatchStartedKey = "matchStarted";
    private const string TeamBlue = "Blue";
    private const string TeamRed = "Red";

    private bool _leavingLobby = false;
    private string _targetTeam;
    private string _targetRoom;
    private string _targetScene;

    void Start() { TryProceed(); }

    public override void OnRoomPropertiesUpdate(PhotonHashtable changedProps)
    {
        TryProceed();
    }

    public override void OnPlayerPropertiesUpdate(Player target, PhotonHashtable changedProps)
    {
        if (!target.IsLocal) return;
        TryProceed();
    }

    private void TryProceed()
    {
        if (_leavingLobby) return;
        if (!PhotonNetwork.InRoom) return;

        // ¿Partida iniciada?
        bool started = PhotonNetwork.CurrentRoom.CustomProperties != null &&
                       PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(MatchStartedKey) &&
                       (bool)PhotonNetwork.CurrentRoom.CustomProperties[MatchStartedKey];
        if (!started) return;


        if (PhotonNetwork.LocalPlayer.CustomProperties == null) return;
        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(TeamKey)) return;
        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(TeamRoomKey)) return;

        _targetTeam = PhotonNetwork.LocalPlayer.CustomProperties[TeamKey] as string;
        _targetRoom = PhotonNetwork.LocalPlayer.CustomProperties[TeamRoomKey] as string;
        _targetScene = (_targetTeam == TeamBlue) ? blueScene : redScene;

        if (string.IsNullOrEmpty(_targetRoom) || string.IsNullOrEmpty(_targetScene)) return;

        _leavingLobby = true;
        Debug.Log($"[Switch] Saliendo de Lobby → {_targetRoom} ({_targetTeam})");
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("[Switch] Ya salí del lobby, esperando reconexión a Master...");

    }

    public override void OnConnectedToMaster()
    {
        if (!_leavingLobby || string.IsNullOrEmpty(_targetRoom)) return;

        PhotonNetwork.AutomaticallySyncScene = true;

        TryJoinTeamRoom();
    }

    private int _joinAttempts = 0;
    private void TryJoinTeamRoom()
    {
        _joinAttempts++;
        var opts = new RoomOptions { MaxPlayers = 2, IsOpen = true, IsVisible = false };
        Debug.Log($"[Switch] JoinOrCreate '{_targetRoom}' (attempt #{_joinAttempts})");
        PhotonNetwork.JoinOrCreateRoom(_targetRoom, opts, TypedLobby.Default);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[Switch] OnJoinRoomFailed: {returnCode} - {message}");
        if (_joinAttempts < 5) // reintenta hasta 5 veces
            StartCoroutine(RetryJoinLater(0.5f));
        else
            Debug.LogError("[Switch] Fallaron múltiples intentos de join al team-room.");
    }

    private System.Collections.IEnumerator RetryJoinLater(float delay)
    {
        yield return new WaitForSeconds(delay);
        TryJoinTeamRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[TeamRoom] Entré a '{_targetRoom}' ({PhotonNetwork.CurrentRoom.PlayerCount}/2)");

        PhotonNetwork.AutomaticallySyncScene = true;

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(LoadTeamSceneSafely());
    }

    private System.Collections.IEnumerator LoadTeamSceneSafely()
    {
        float wait = 0f;
        while (wait < 1.0f && PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            wait += Time.deltaTime;
            yield return null;
        }

        Debug.Log("[TeamRoom] Cargando escena de equipo: " + _targetScene);
        PhotonNetwork.LoadLevel(_targetScene);
    }
}
