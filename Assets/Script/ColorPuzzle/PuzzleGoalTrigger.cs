using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class PuzzleGoalTrigger : MonoBehaviour
{
    [Header("Lobby Spawns")]
    [SerializeField] private Transform lobbyBlueSpawn;
    [SerializeField] private Transform lobbyRedSpawn;

    private bool alreadyScored = false;

    private const string TeamKey = "team";
    private const string ReadyKey = "ready";
    private const string MatchStartedKey = "matchStarted";
    private const string RoundIndexKey = "roundIndex";

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyScored) return;

        var pv = other.GetComponentInParent<PhotonView>();
        if (pv == null || !pv.IsMine) return;

        string team = GetTeamOf(pv.Owner);
        if (string.IsNullOrEmpty(team)) return;
        if (!PhotonNetwork.IsMasterClient) return;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPoint(team);  // Puzzle 2 → +1
            Debug.Log($"[PuzzleGoalTrigger] Equipo {team} completó Puzzle 2 (+1).");
        }

        TeleportAllPlayersToLobby();
        ResetAllReadyFlags();

        var props = new PhotonHashtable
        {
            { MatchStartedKey, false },
            { RoundIndexKey, 3 }  // siguiente vez que den Ready → Puzzle 3
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        Debug.Log("[PuzzleGoalTrigger] matchStarted=false, roundIndex=3 (listo para Puzzle 3).");

        alreadyScored = true;
    }

    private void TeleportAllPlayersToLobby()
    {
        foreach (var p in PhotonNetwork.PlayerList)
        {
            string team = GetTeamOf(p);
            if (string.IsNullOrEmpty(team)) continue;

            Transform targetSpawn = null;
            if (team == "Blue")
                targetSpawn = lobbyBlueSpawn;
            else if (team == "Red")
                targetSpawn = lobbyRedSpawn;

            if (targetSpawn == null) continue;

            if (p.TagObject is GameObject go)
            {
                go.transform.position = targetSpawn.position;
                go.transform.rotation = targetSpawn.rotation;
            }
        }

        Debug.Log("[PuzzleGoalTrigger] Todos los jugadores teletransportados a la lobby (después de Puzzle 2).");
    }

    private void ResetAllReadyFlags()
    {
        foreach (var p in PhotonNetwork.PlayerList)
        {
            var props = new PhotonHashtable
            {
                { ReadyKey, false }
            };
            p.SetCustomProperties(props);
        }

        Debug.Log("[PuzzleGoalTrigger] Flags de Ready reseteados a false para todos (después de Puzzle 2).");
    }

    private string GetTeamOf(Player p)
    {
        if (p?.CustomProperties == null) return "";
        return p.CustomProperties.TryGetValue(TeamKey, out object value)
            ? (value as string ?? "")
            : "";
    }
}
