using System.Collections;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;


public class LobbyReadyManager : MonoBehaviourPunCallbacks
{
    [Header("UI (opcional)")]
    [SerializeField] private TMP_Text readyCountText;

    [Header("Config")]
    [SerializeField] private int requiredPlayers = 4;
    [SerializeField] private string gameSceneName = "Egypt";

    private const string ReadyKey = "ready";
    private const string TeamKey = "team";
    private const string Blue = "Blue";
    private const string Red = "Red";

    private bool _starting = false;

    void Awake()
    {
        // redundante pero seguro
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        LogRoomState("Start()");
        Evaluate();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        LogRoomState($"OnPlayerEnteredRoom: {newPlayer.NickName}");
        Evaluate();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        LogRoomState($"OnPlayerLeftRoom: {otherPlayer.NickName}");
        _starting = false; // por si se fue alguien
        Evaluate();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
    {
        if (changedProps == null) return;
        if (!changedProps.ContainsKey(ReadyKey)) return;

        Debug.Log($"[ReadyChange] {targetPlayer.NickName} -> {changedProps[ReadyKey]}");
        Evaluate();
    }

    private void Evaluate()
    {
        var players = PhotonNetwork.PlayerList;
        int playerCount = players.Length;

        int readyCount = players.Count(p =>
            p.CustomProperties != null &&
            p.CustomProperties.ContainsKey(ReadyKey) &&
            (bool)p.CustomProperties[ReadyKey]);

        if (readyCountText != null)
            readyCountText.text = $"Ready: {readyCount}/{requiredPlayers}";

        Debug.Log($"[Eval] count={playerCount} ready={readyCount} required={requiredPlayers} master={PhotonNetwork.IsMasterClient}");

        if (!PhotonNetwork.IsMasterClient) return;
        if (_starting) return;

        // Condición de arranque:
        // - hay requiredPlayers en la sala
        // - todos esos están ready
        if (playerCount == requiredPlayers && readyCount == requiredPlayers)
        {
            StartCoroutine(StartMatchOnce());
        }
    }

    private IEnumerator StartMatchOnce()
    {
        _starting = true;

        // Asignar equipos determinísticamente (2 y 2) si les falta team
        var ordered = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToArray();
        int blue = 0, red = 0;
        foreach (var p in ordered)
        {
            string team = null;
            if (p.CustomProperties != null && p.CustomProperties.ContainsKey(TeamKey))
                team = p.CustomProperties[TeamKey] as string;

            if (string.IsNullOrEmpty(team))
            {
                team = (blue < requiredPlayers / 2) ? Blue : Red;
                photonView.RPC(nameof(RPC_SetTeam), p, team);
            }

            if (team == Blue) blue++; else if (team == Red) red++;
        }

        Debug.Log($"[StartMatch] Assigned -> Blue:{blue} Red:{red}");

        // Pequeña espera para que los RPC de team lleguen a todos
        yield return new WaitForSeconds(0.2f);

        Debug.Log("[StartMatch] Loading scene 'Egypt' (master)");
        PhotonNetwork.LoadLevel(gameSceneName);
    }

    [PunRPC]
    private void RPC_SetTeam(string team)
    {
        var me = PhotonNetwork.LocalPlayer;
        me.SetCustomProperties(new PhotonHashtable { { TeamKey, team } });
        Debug.Log($"[Team] {me.NickName} -> {team}");
    }

    private void LogRoomState(string where)
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.Log($"[RoomState:{where}] Not in room");
            return;
        }

        var players = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToArray();
        string dump = string.Join(", ", players.Select(p =>
        {
            string r = (p.CustomProperties != null && p.CustomProperties.ContainsKey(ReadyKey))
                ? ((bool)p.CustomProperties[ReadyKey] ? "R" : "nR") : "--";
            string t = (p.CustomProperties != null && p.CustomProperties.ContainsKey(TeamKey))
                ? (string)p.CustomProperties[TeamKey] : "--";
            return $"{p.ActorNumber}:{p.NickName}[{r}|{t}]";
        }));
        Debug.Log($"[RoomState:{where}] players={players.Length} :: {dump}");
    }
}
