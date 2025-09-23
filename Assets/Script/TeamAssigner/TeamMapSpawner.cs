using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

using PUNPlayer = Photon.Realtime.Player;

public class TeamMapSpawner : MonoBehaviourPunCallbacks
{
    [Header("Player prefab (Resources)")]
    [SerializeField] private string playerPrefabName = "Player";

    [Header("Spawn points")]
    [SerializeField] private Transform[] blueSpawns;
    [SerializeField] private Transform[] redSpawns;
    [SerializeField] private Transform lobbySpawn;   // 👈 spawn de lobby (centro)

    private const string TeamKey = "team";
    private const string TeamBlue = "Blue";
    private const string TeamRed = "Red";

    private bool _spawned = false;

    private void Start()
    {
        TrySpawnLobby();
    }

    public override void OnPlayerPropertiesUpdate(PUNPlayer target, PhotonHashtable changedProps)
    {
        if (!target.IsLocal) return;
        if (_spawned) return;
        if (changedProps != null && changedProps.ContainsKey(TeamKey))
        {
            TrySpawnLobby();
        }
    }

    // 🔑 escucha cambios de sala
    public override void OnRoomPropertiesUpdate(PhotonHashtable changedProps)
    {
        if (changedProps.ContainsKey("matchStarted"))
        {
            bool started = (bool)changedProps["matchStarted"];
            if (started) TeleportToTeamZone();
            else TeleportToLobby();
        }
    }

    private void TrySpawnLobby()
    {
        if (_spawned) return;
        if (!PhotonNetwork.InRoom) return;

        Vector3 pos = lobbySpawn ? lobbySpawn.position : Vector3.zero;
        Quaternion rot = lobbySpawn ? lobbySpawn.rotation : Quaternion.identity;

        if (PhotonNetwork.LocalPlayer.TagObject == null)
        {
            GameObject go = PhotonNetwork.Instantiate(playerPrefabName, pos, rot);
            PhotonNetwork.LocalPlayer.TagObject = go;
            _spawned = true;

            Debug.Log($"[TeamMapSpawner] {PhotonNetwork.NickName} spawneado en Lobby {pos}");
        }
    }

    private void TeleportToTeamZone()
    {
        string myTeam = GetMyTeam();
        Transform spawn = PickSpawnFor(PhotonNetwork.LocalPlayer, myTeam);

        if (spawn != null && PhotonNetwork.LocalPlayer.TagObject is GameObject playerObj)
        {
            playerObj.transform.position = spawn.position;
            playerObj.transform.rotation = spawn.rotation;

            Debug.Log($"[TeamMapSpawner] {PhotonNetwork.NickName} tepeado a {myTeam} en {spawn.position}");
        }
    }

    private void TeleportToLobby()
    {
        if (lobbySpawn != null && PhotonNetwork.LocalPlayer.TagObject is GameObject playerObj)
        {
            playerObj.transform.position = lobbySpawn.position;
            playerObj.transform.rotation = lobbySpawn.rotation;

            Debug.Log($"[TeamMapSpawner] {PhotonNetwork.NickName} volvió al Lobby en {lobbySpawn.position}");
        }
    }

    private string GetMyTeam()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties == null) return null;
        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(TeamKey)) return null;
        return PhotonNetwork.LocalPlayer.CustomProperties[TeamKey] as string;
    }

    private Transform PickSpawnFor(PUNPlayer player, string team)
    {
        var teamPlayers = PhotonNetwork.PlayerList
            .Where(p => p.CustomProperties != null &&
                        p.CustomProperties.ContainsKey(TeamKey) &&
                        (string)p.CustomProperties[TeamKey] == team)
            .OrderBy(p => p.ActorNumber)
            .ToArray();

        int indexInTeam = System.Array.IndexOf(teamPlayers, player);
        if (indexInTeam < 0) indexInTeam = 0;

        if (team == TeamBlue && blueSpawns.Length > 0)
            return blueSpawns[indexInTeam % blueSpawns.Length];
        if (team == TeamRed && redSpawns.Length > 0)
            return redSpawns[indexInTeam % redSpawns.Length];

        return null;
    }
}
