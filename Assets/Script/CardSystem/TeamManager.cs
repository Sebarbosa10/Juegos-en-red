using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;

public class TeamManager : MonoBehaviourPunCallbacks
{
    public static TeamManager Instance { get; private set; }

    private List<Player> blueTeam = new List<Player>();
    private List<Player> redTeam = new List<Player>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RefreshTeams()
    {
        blueTeam = PhotonNetwork.PlayerList
            .Where(p => p.CustomProperties.ContainsKey("team") && (string)p.CustomProperties["team"] == "Blue")
            .OrderBy(p => p.ActorNumber)
            .ToList();

        redTeam = PhotonNetwork.PlayerList
            .Where(p => p.CustomProperties.ContainsKey("team") && (string)p.CustomProperties["team"] == "Red")
            .OrderBy(p => p.ActorNumber)
            .ToList();

        Debug.Log($"[TeamManager] Blue: {blueTeam.Count}, Red: {redTeam.Count}");
    }

    public Player GetRival(Player player)
    {
        if (!player.CustomProperties.ContainsKey("team"))
        {
            Debug.LogWarning("[TeamManager] Player has no team assigned.");
            return null;
        }

        string team = (string)player.CustomProperties["team"];

        if (team == "Blue")
        {
            int index = blueTeam.IndexOf(player);
            return (index >= 0 && index < redTeam.Count) ? redTeam[index] : null;
        }
        else if (team == "Red")
        {
            int index = redTeam.IndexOf(player);
            return (index >= 0 && index < blueTeam.Count) ? blueTeam[index] : null;
        }

        return null;
    }

}

