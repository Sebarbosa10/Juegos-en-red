using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TeleportAllToAreaOnSolved : MonoBehaviourPun
{
    [Header("Spawns in lobby")]
    [SerializeField] public Transform[] spawnPoints;

    [Header("Back To Lobby Spawns")]
    [SerializeField] private bool useTeamSlots = false;
    [SerializeField] private Transform[] blueSpawns; 
    [SerializeField] private Transform[] redSpawns;  

    public void TeleportAll()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            photonView.RPC(nameof(RPC_TeleportAll), RpcTarget.All);
        }
        else
        {
            DoLocalTeleport();
        }
    }

    [PunRPC]
    private void RPC_TeleportAll()
    {
        DoLocalTeleport();
    }

    private void DoLocalTeleport()
    {
        if (useTeamSlots)
            TeleportLocalByTeam();
        else
            TeleportLocalByIndex();
    }
    private void TeleportLocalByIndex()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        var players = PhotonNetwork.PlayerList; 
        int myIndex = System.Array.FindIndex(players, p => p == PhotonNetwork.LocalPlayer);
        if (myIndex < 0) return;

        var sp = spawnPoints[myIndex % spawnPoints.Length];
        TeleportLocalOwnedPlayer(sp.position, sp.rotation);
    }

    private void TeleportLocalByTeam()
    {
        string team = GetLocalTeam();
        Transform[] pool = (team == "Blue") ? blueSpawns : redSpawns;
        if (pool == null || pool.Length == 0) return;

        var teamPlayers = PhotonNetwork.PlayerList
            .Where(p => GetTeam(p) == team)
            .OrderBy(p => p.ActorNumber)
            .ToArray();

        int myTeamIndex = System.Array.FindIndex(teamPlayers, p => p == PhotonNetwork.LocalPlayer);
        if (myTeamIndex < 0) return;

        var sp = pool[myTeamIndex % pool.Length];
        TeleportLocalOwnedPlayer(sp.position, sp.rotation);
    }

    private static string GetTeam(Player p)
    {
        if (p?.CustomProperties == null) return "";
        return p.CustomProperties.TryGetValue("team", out object t) ? (t as string ?? "") : "";
    }

    private static string GetLocalTeam() => GetTeam(PhotonNetwork.LocalPlayer);

    private static void TeleportLocalOwnedPlayer(Vector3 pos, Quaternion rot)
    {
        PhotonView mine = null;

        var allPVs = Object.FindObjectsOfType<PhotonView>(true);
        foreach (var pv in allPVs)
        {
            if (pv != null && pv.IsMine)
            {
                mine = pv;
                break;
            }
        }
        if (mine == null) return;

        var tr = mine.transform;

        var cc = mine.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            tr.SetPositionAndRotation(pos, rot);
            cc.enabled = true;
            return;
        }

        var rb = mine.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = pos;
            rb.rotation = rot;
            return;
        }

        tr.SetPositionAndRotation(pos, rot);
    }
}

