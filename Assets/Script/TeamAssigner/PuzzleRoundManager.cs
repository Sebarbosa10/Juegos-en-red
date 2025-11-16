using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class PuzzleRoundManager : MonoBehaviourPunCallbacks
{
    public static PuzzleRoundManager Instance;

    private const string TeamKey = "team";
    private const string ReadyKey = "ready";

    [Header("Lobby Spawns")]
    [SerializeField] private Transform[] lobbyBlueSpawns;
    [SerializeField] private Transform[] lobbyRedSpawns;

    [Header("Puzzle 1 Spawns")]
    [SerializeField] private Transform[] puzzle1BlueSpawns;
    [SerializeField] private Transform[] puzzle1RedSpawns;

    [Header("Puzzle 2 Spawns")]
    [SerializeField] private Transform[] puzzle2BlueSpawns;
    [SerializeField] private Transform[] puzzle2RedSpawns;

    private int currentPuzzleIndex = 1; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

   
    public void OnPuzzleCompleted(string winningTeam)
    {
        if (!PhotonNetwork.IsMasterClient) return; 

        Debug.Log($"[PuzzleRoundManager] Puzzle completado por {winningTeam}");

      
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPoint(winningTeam);
        }

        
        TeleportAllPlayersToLobby();

        
        ResetAllReadyFlags();

        
        currentPuzzleIndex = 2; 
    }

   
    public void StartCurrentPuzzle()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log($"[PuzzleRoundManager] Enviando a todos al Puzzle {currentPuzzleIndex}");

        foreach (var p in PhotonNetwork.PlayerList)
        {
            string team = GetTeamOf(p);
            if (string.IsNullOrEmpty(team)) continue;

            Transform spawn = GetPuzzleSpawnForPlayer(p, team, currentPuzzleIndex);
            if (spawn == null) continue;

            if (p.TagObject is GameObject go)
            {
                go.transform.position = spawn.position;
                go.transform.rotation = spawn.rotation;
            }
        }
    }

  

    private void TeleportAllPlayersToLobby()
    {
        Debug.Log("[PuzzleRoundManager] Teletransportando todos a la lobby...");

        foreach (var p in PhotonNetwork.PlayerList)
        {
            string team = GetTeamOf(p);
            if (string.IsNullOrEmpty(team)) continue;

            Transform spawn = GetLobbySpawnForPlayer(p, team);
            if (spawn == null) continue;

            if (p.TagObject is GameObject go)
            {
                go.transform.position = spawn.position;
                go.transform.rotation = spawn.rotation;
            }
        }
    }

    private void ResetAllReadyFlags()
    {
        Debug.Log("[PuzzleRoundManager] Reseteando flags de Ready...");

        foreach (var p in PhotonNetwork.PlayerList)
        {
            var props = new PhotonHashtable
            {
                { ReadyKey, false }
            };
            p.SetCustomProperties(props);
        }
    }

    private string GetTeamOf(Player p)
    {
        if (p.CustomProperties == null) return null;
        if (!p.CustomProperties.ContainsKey(TeamKey)) return null;
        return p.CustomProperties[TeamKey] as string;
    }

    private Transform GetLobbySpawnForPlayer(Player p, string team)
    {
        var sameTeamPlayers = PhotonNetwork.PlayerList
            .Where(x => GetTeamOf(x) == team)
            .OrderBy(x => x.ActorNumber)
            .ToArray();

        int indexInTeam = System.Array.IndexOf(sameTeamPlayers, p);
        if (indexInTeam < 0) indexInTeam = 0;

        if (team == "Blue")
        {
            if (lobbyBlueSpawns != null && lobbyBlueSpawns.Length > 0)
                return lobbyBlueSpawns[indexInTeam % lobbyBlueSpawns.Length];
        }
        else
        {
            if (lobbyRedSpawns != null && lobbyRedSpawns.Length > 0)
                return lobbyRedSpawns[indexInTeam % lobbyRedSpawns.Length];
        }

        return null;
    }

    private Transform GetPuzzleSpawnForPlayer(Player p, string team, int puzzleIndex)
    {
        Transform[] blueArray = null;
        Transform[] redArray = null;

        switch (puzzleIndex)
        {
            case 1:
                blueArray = puzzle1BlueSpawns;
                redArray = puzzle1RedSpawns;
                break;
            case 2:
                blueArray = puzzle2BlueSpawns;
                redArray = puzzle2RedSpawns;
                break;
            default:
              
                blueArray = puzzle1BlueSpawns;
                redArray = puzzle1RedSpawns;
                break;
        }

        var sameTeamPlayers = PhotonNetwork.PlayerList
            .Where(x => GetTeamOf(x) == team)
            .OrderBy(x => x.ActorNumber)
            .ToArray();

        int indexInTeam = System.Array.IndexOf(sameTeamPlayers, p);
        if (indexInTeam < 0) indexInTeam = 0;

        if (team == "Blue")
        {
            if (blueArray != null && blueArray.Length > 0)
                return blueArray[indexInTeam % blueArray.Length];
        }
        else
        {
            if (redArray != null && redArray.Length > 0)
                return redArray[indexInTeam % redArray.Length];
        }

        return null;
    }
}
