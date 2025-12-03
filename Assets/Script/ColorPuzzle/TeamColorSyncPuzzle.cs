using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;


public class TeamColorSyncPuzzle : MonoBehaviourPunCallbacks
{
    public static TeamColorSyncPuzzle Instance;

    [Header("Objetos a desaparecer cuando el equipo resuelve")]
    [SerializeField] private GameObject[] objectsToDisableOnSolve;

    [Header("Configuración")]
    [SerializeField] private float syncWindow = 0.5f;

    private const string TeamKey = "team";

    private class PressInfo
    {
        public string Team;
        public int ColorIndex;
        public double Time;
    }

    private Dictionary<int, PressInfo> lastPressByActor = new Dictionary<int, PressInfo>();
    private bool puzzleSolved = false;

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterLocalPress(int colorIndex)
    {
        if (puzzleSolved) return;

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
        if (puzzleSolved) return;

        if (!lastPressByActor.ContainsKey(actorNumber))
            lastPressByActor[actorNumber] = new PressInfo();

        lastPressByActor[actorNumber].Team = team;
        lastPressByActor[actorNumber].ColorIndex = colorIndex;
        lastPressByActor[actorNumber].Time = time;

        CheckSync(colorIndex, time);
    }

    private void CheckSync(int colorIndex, double time)
    {
       
        List<PressInfo> matchingPresses = new List<PressInfo>();

        foreach (var press in lastPressByActor.Values)
        {
            if (press.ColorIndex == colorIndex)
            {
                matchingPresses.Add(press);
            }
        }

        
        if (matchingPresses.Count < 2) return;

        
        for (int i = 0; i < matchingPresses.Count; i++)
        {
            for (int j = i + 1; j < matchingPresses.Count; j++)
            {
                double dt = System.Math.Abs(matchingPresses[i].Time - matchingPresses[j].Time);
                if (dt > syncWindow) return;
            }
        }

        
        int currentCubeColor = GetCurrentCubeColorIndex();
        if (currentCubeColor != colorIndex) return;

        
        puzzleSolved = true;
        photonView.RPC(nameof(RPC_PuzzleSolved), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PuzzleSolved()
    {
        Debug.Log("[TeamColorSyncPuzzle] ¡Puzzle resuelto! Desapareciendo objetos...");

        puzzleSolved = true;

        
        foreach (var obj in objectsToDisableOnSolve)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Debug.Log($"[TeamColorSyncPuzzle] Objeto desactivado: {obj.name}");
            }
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
        if (p?.CustomProperties == null) return "";
        return p.CustomProperties.TryGetValue(TeamKey, out object t) ? (t as string ?? "") : "";
    }
}
