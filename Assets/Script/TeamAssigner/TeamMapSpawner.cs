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

    private const string TeamKey = "team";
    private const string TeamBlue = "Blue";
    private const string TeamRed = "Red";

    private bool _spawned = false;

    private void Start()
    {
        TrySpawn();
    }

    public override void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
    {
        if (propertiesThatChanged == null) return;

        if (propertiesThatChanged.ContainsKey("matchStarted"))
        {
            bool matchStarted = (bool)propertiesThatChanged["matchStarted"];
            if (matchStarted)
            {
                Debug.Log("[Spawner] MatchStart detectado → teletransportando jugador a su zona");
                ForceRespawnAtTeamZone();
            }
        }
    }

    private void ForceRespawnAtTeamZone()
    {
        string myTeam = GetMyTeam();
        if (string.IsNullOrEmpty(myTeam)) return;

        Transform spawn = PickSpawnFor(PhotonNetwork.LocalPlayer, myTeam);
        if (spawn == null) return;

        if (PhotonNetwork.LocalPlayer.TagObject is GameObject myPlayer)
        {
            myPlayer.transform.position = spawn.position;
            myPlayer.transform.rotation = spawn.rotation;
            Debug.Log($"[Spawner] {PhotonNetwork.NickName} movido a {myTeam} spawn {spawn.position}");
        }
    }


    private void TrySpawn()
    {
        if (_spawned) return;
        if (!PhotonNetwork.InRoom) return;

        string myTeam = GetMyTeam();
        if (string.IsNullOrEmpty(myTeam)) return; 

        Transform spawn = PickSpawnFor(PhotonNetwork.LocalPlayer, myTeam);
        Vector3 pos = spawn ? spawn.position : Vector3.zero;
        Quaternion rot = spawn ? spawn.rotation : Quaternion.identity;

        if (PhotonNetwork.LocalPlayer.TagObject == null)
        {
            GameObject go = PhotonNetwork.Instantiate(playerPrefabName, pos, rot);
            PhotonNetwork.LocalPlayer.TagObject = go;
            _spawned = true;

            Debug.Log($"[TeamMapSpawner] {PhotonNetwork.NickName} ({myTeam}) spawneado en {pos}");
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

        if (team == TeamBlue)
        {
            if (blueSpawns != null && blueSpawns.Length > 0)
                return blueSpawns[indexInTeam % blueSpawns.Length];
        }
        else 
        {
            if (redSpawns != null && redSpawns.Length > 0)
                return redSpawns[indexInTeam % redSpawns.Length];
        }

        return null;
    }
}
