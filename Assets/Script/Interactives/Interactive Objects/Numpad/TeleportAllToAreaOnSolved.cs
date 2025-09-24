using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TeleportAllToAreaOnSolved : MonoBehaviourPun
{
    [Header("Spawns en el lobby de ESTA escena (mínimo 4)")]
    [SerializeField] public Transform[] spawnPoints;

    [Header("Opcional: separar por equipo si usás CustomProperties[\"team\"] = \"Blue\"/\"Red\"")]
    [SerializeField] private bool useTeamSlots = false;
    [SerializeField] private Transform[] blueSpawns; // tamaño 2
    [SerializeField] private Transform[] redSpawns;  // tamaño 2

    // Enganchá este método al onCorrectCode de CADA TeamNumpadController
    public void TeleportAll()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            // Un solo llamado, todos ejecutan la misma lógica local (lista de players es determinística)
            photonView.RPC(nameof(RPC_TeleportAll), RpcTarget.All);
        }
        else
        {
            // Modo singleplayer / pruebas sin PUN
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

    // Asigna spawns por índice global (ActorNumber) — útil si no te importa el equipo
    private void TeleportLocalByIndex()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        var players = PhotonNetwork.PlayerList; // ordenados por ActorNumber
        int myIndex = System.Array.FindIndex(players, p => p == PhotonNetwork.LocalPlayer);
        if (myIndex < 0) return;

        var sp = spawnPoints[myIndex % spawnPoints.Length];
        TeleportLocalOwnedPlayer(sp.position, sp.rotation);
    }

    // Asigna 2 spawns a Blue y 2 a Red (o lo que definas)
    private void TeleportLocalByTeam()
    {
        string team = GetLocalTeam();
        Transform[] pool = (team == "Blue") ? blueSpawns : redSpawns;
        if (pool == null || pool.Length == 0) return;

        // índice dentro del subgrupo de tu equipo
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

    // Teleporta SOLO al jugador local (el que este cliente controla)
    private static void TeleportLocalOwnedPlayer(Vector3 pos, Quaternion rot)
    {
        // Busca el objeto del jugador local (con PhotonView.IsMine)
        PhotonView mine = null;

        // Si sabés el componente de tu player (ej.: PlayerController), es más rápido:
        // mine = FindObjectsOfType<PlayerController>(true).Select(pc => pc.GetComponent<PhotonView>()).FirstOrDefault(pv => pv && pv.IsMine);

        // Genérico:
        var allPVs = Object.FindObjectsOfType<PhotonView>(true);
        foreach (var pv in allPVs)
        {
            if (pv != null && pv.IsMine)
            {
                // Heurística: evitar agarrar PVs de UI u otros; si tu prefab del player tiene un tag/layer, filtralo acá.
                mine = pv;
                break;
            }
        }
        if (mine == null) return;

        var tr = mine.transform;

        // Manejo seguro según componente de movimiento
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
            rb.isKinematic = false; // por las dudas
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = pos;
            rb.rotation = rot;
            return;
        }

        tr.SetPositionAndRotation(pos, rot);
    }
}

