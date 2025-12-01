using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

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
            Debug.Log("[TeamManager] Instancia creada");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Refrescar cuando cambian las propiedades de un jugador
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
    {
        if (changedProps.ContainsKey("team"))
        {
            Debug.Log($"[TeamManager] {targetPlayer.NickName} cambió de equipo, refrescando...");
            RefreshTeams();
        }
    }

    public void RefreshTeams()
    {
        Debug.Log("[TeamManager] ========== RefreshTeams() ==========");

        blueTeam.Clear();
        redTeam.Clear();

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p.CustomProperties != null && p.CustomProperties.ContainsKey("team"))
            {
                string team = (string)p.CustomProperties["team"];
                Debug.Log($"[TeamManager] {p.NickName} (Actor {p.ActorNumber}) -> Equipo: {team}");

                if (team == "Blue")
                    blueTeam.Add(p);
                else if (team == "Red")
                    redTeam.Add(p);
            }
            else
            {
                Debug.LogWarning($"[TeamManager] {p.NickName} NO tiene equipo asignado!");
            }
        }

        blueTeam = blueTeam.OrderBy(p => p.ActorNumber).ToList();
        redTeam = redTeam.OrderBy(p => p.ActorNumber).ToList();

        Debug.Log($"[TeamManager] Equipo Blue ({blueTeam.Count}): {string.Join(", ", blueTeam.Select(p => p.NickName))}");
        Debug.Log($"[TeamManager] Equipo Red ({redTeam.Count}): {string.Join(", ", redTeam.Select(p => p.NickName))}");
    }

    public Player GetRival(Player player)
    {
        if (player == null)
        {
            Debug.LogError("[TeamManager] GetRival: player es NULL!");
            return null;
        }

        if (!player.CustomProperties.ContainsKey("team"))
        {
            Debug.LogWarning($"[TeamManager] GetRival: {player.NickName} no tiene equipo");
            return null;
        }

        string team = (string)player.CustomProperties["team"];

        if (team == "Blue")
        {
            int index = blueTeam.IndexOf(player);
            if (index >= 0 && index < redTeam.Count)
            {
                return redTeam[index];
            }
        }
        else if (team == "Red")
        {
            int index = redTeam.IndexOf(player);
            if (index >= 0 && index < blueTeam.Count)
            {
                return blueTeam[index];
            }
        }

        Debug.LogWarning($"[TeamManager] No se encontró rival para {player.NickName}");
        return null;
    }
}


