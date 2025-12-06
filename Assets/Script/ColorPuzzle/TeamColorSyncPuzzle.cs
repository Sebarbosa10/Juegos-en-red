using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;



public class TeamColorSyncPuzzle : MonoBehaviourPunCallbacks
{
    public static TeamColorSyncPuzzle Instance;

    [Header("Settings")]
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
        Debug.Log($"[TeamColorSyncPuzzle] {p.NickName} ({team}) presionó color {colorIndex}");

        photonView.RPC(nameof(RPC_RegisterPress), RpcTarget.MasterClient, p.ActorNumber, team, colorIndex, now);
    }

    [PunRPC]
    private void RPC_RegisterPress(int actorNumber, string team, int colorIndex, double time, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log($"[TeamColorSyncPuzzle] MasterClient recibió: Actor {actorNumber}, Team {team}, Color {colorIndex}");

        if (solvedTeams.Contains(team))
        {
            Debug.Log($"[TeamColorSyncPuzzle] Equipo {team} ya resolvió, ignorando");
            return;
        }

        if (!lastPressByActor.ContainsKey(actorNumber))
            lastPressByActor[actorNumber] = new PressInfo();

        lastPressByActor[actorNumber].Team = team;
        lastPressByActor[actorNumber].ColorIndex = colorIndex;
        lastPressByActor[actorNumber].Time = time;

        CheckTeamSync(team, colorIndex, time);
    }

    private void CheckTeamSync(string team, int colorIndex, double currentTime)
    {
        Debug.Log($"[TeamColorSyncPuzzle] Verificando sync para {team}, color {colorIndex}");

        // Obtener jugadores del mismo equipo
        List<Player> teamList = new List<Player>();
        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (GetTeamOf(p) == team)
                teamList.Add(p);
        }

        if (teamList.Count < 2)
        {
            Debug.Log($"[TeamColorSyncPuzzle] Equipo {team} no tiene 2 jugadores");
            return;
        }

        // Verificar que AMBOS jugadores presionaron el MISMO color RECIENTEMENTE
        foreach (var p in teamList)
        {
            if (!lastPressByActor.TryGetValue(p.ActorNumber, out var press))
            {
                Debug.Log($"[TeamColorSyncPuzzle] {p.NickName} no ha presionado ningún botón");
                return;
            }

            if (press.Team != team)
            {
                Debug.Log($"[TeamColorSyncPuzzle] Press de {p.NickName} es de otro equipo");
                return;
            }

            if (press.ColorIndex != colorIndex)
            {
                Debug.Log($"[TeamColorSyncPuzzle] {p.NickName} presionó color {press.ColorIndex}, no {colorIndex}");
                return;
            }

            double timeDiff = System.Math.Abs(currentTime - press.Time);
            if (timeDiff > syncWindow)
            {
                Debug.Log($"[TeamColorSyncPuzzle] Press de {p.NickName} es muy viejo ({timeDiff}s)");
                return;
            }
        }

        // Verificar que el color coincide con el cubo
        int currentCubeColor = GetCurrentCubeColorIndex();
        if (currentCubeColor != colorIndex)
        {
            Debug.Log($"[TeamColorSyncPuzzle] Color {colorIndex} no coincide con cubo {currentCubeColor}");
            return;
        }

        // ¡VICTORIA!
        Debug.Log($"[TeamColorSyncPuzzle] ¡¡¡EQUIPO {team} GANA!!!");
        solvedTeams.Add(team);
        photonView.RPC(nameof(RPC_OnTeamSolved), RpcTarget.All, team);
    }

    [PunRPC]
    private void RPC_OnTeamSolved(string team)
    {
        Debug.Log($"[TeamColorSyncPuzzle] {team} resolvió el puzzle!");

        // Dar 2 puntos (victoria) - solo MasterClient
        if (PhotonNetwork.IsMasterClient && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPoint(team);
            ScoreManager.Instance.AddPoint(team);
            Debug.Log($"[TeamColorSyncPuzzle] {team} +2 puntos → Victoria!");
        }

        // Resetear efectos de carta
        ResetLocalPlayerCardEffects();
    }

    private void ResetLocalPlayerCardEffects()
    {
        if (PhotonNetwork.LocalPlayer.TagObject is GameObject playerObj)
        {
            var effectManager = playerObj.GetComponent<CardEffectManager>();
            if (effectManager != null)
            {
                effectManager.ResetAllEffects();
            }
        }

        if (CardEffectUI.Instance != null)
        {
            CardEffectUI.Instance.Clear();
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

    public override void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("matchStarted"))
        {
            bool matchStarted = (bool)propertiesThatChanged["matchStarted"];
            if (matchStarted)
            {
                solvedTeams.Clear();
                lastPressByActor.Clear();
                Debug.Log("[TeamColorSyncPuzzle] Puzzle reseteado");
            }
        }
    }
}
