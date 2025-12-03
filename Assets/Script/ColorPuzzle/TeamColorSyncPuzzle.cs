using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;

public class TeamColorSyncPuzzle : MonoBehaviourPunCallbacks
{
    public static TeamColorSyncPuzzle Instance;

    [SerializeField] private GameObject blueDoor;
    [SerializeField] private GameObject redDoor;

    [SerializeField] private float syncWindow = 0.5f;

    private const string TeamKey = "team";

    private class PressInfo
    {
        public string Team;
        public int ColorIndex;
        public double Time;
    }

    private Dictionary<int, PressInfo> lastPressByActor = new Dictionary<int, PressInfo>();
    private HashSet<string> solvedTeams = new HashSet<string>();

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterLocalPress(int colorIndex)
    {
        var p = PhotonNetwork.LocalPlayer;
        string team = GetTeamOf(p);
        if (string.IsNullOrEmpty(team)) return;

        double now = PhotonNetwork.Time;
        photonView.RPC(nameof(RPC_RegisterPress), RpcTarget.MasterClient, p.ActorNumber, team, colorIndex, now);
    }

    [PunRPC]
    private void RPC_RegisterPress(int actorNumber, string team, int colorIndex, double time, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (solvedTeams.Contains(team)) return;

        if (!lastPressByActor.ContainsKey(actorNumber))
            lastPressByActor[actorNumber] = new PressInfo();

        lastPressByActor[actorNumber].Team = team;
        lastPressByActor[actorNumber].ColorIndex = colorIndex;
        lastPressByActor[actorNumber].Time = time;

        CheckTeamSync(team, colorIndex, time);
    }

    private void CheckTeamSync(string team, int colorIndex, double time)
    {
        var teamPlayers = PhotonNetwork.PlayerList;
        var teamList = new List<Player>();

        foreach (var p in teamPlayers)
        {
            if (GetTeamOf(p) == team)
                teamList.Add(p);
        }

        if (teamList.Count < 2) return;

        PressInfo a = null;
        PressInfo b = null;

        foreach (var p in teamList)
        {
            if (!lastPressByActor.TryGetValue(p.ActorNumber, out var press))
                return;
            if (press.Team != team) return;
            if (press.ColorIndex != colorIndex) return;

            if (a == null) a = press;
            else b = press;
        }

        if (a == null || b == null) return;

        double dt = System.Math.Abs(a.Time - b.Time);
        if (dt > syncWindow)
        {
            return;
        }

        int currentCubeColorIndex = GetCurrentCubeColorIndex();
        if (currentCubeColorIndex != colorIndex)
        {
            return;
        }

       
        solvedTeams.Add(team);
        photonView.RPC(nameof(RPC_OnTeamSolved), RpcTarget.All, team);
    }

    [PunRPC]
    private void RPC_OnTeamSolved(string team)
    {
        Debug.Log($"[TeamColorSyncPuzzle] Equipo {team} resolvió el puzzle de colores");

        
        if (team == "Blue" && blueDoor != null)
        {
            blueDoor.SetActive(false);
            Debug.Log("[TeamColorSyncPuzzle] Puerta Blue abierta");
        }

        if (team == "Red" && redDoor != null)
        {
            redDoor.SetActive(false);
            Debug.Log("[TeamColorSyncPuzzle] Puerta Red abierta");
        }


    }

    private int GetCurrentCubeColorIndex()
    {
        if (ColorCubeController.Instance != null)
        {
            return ColorCubeController.Instance.CurrentColorIndex;
        }
        return 0;
    }

    private string GetTeamOf(Player p)
    {
        if (p.CustomProperties == null) return "";
        return p.CustomProperties.TryGetValue(TeamKey, out object t) ? (t as string ?? "") : "";
    }
}