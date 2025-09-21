using System.Collections;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum MatchState { Idle, Playing, BetweenRounds, Finished }

[RequireComponent(typeof(PhotonView))]
public class MatchManager : MonoBehaviourPunCallbacks
{
    public static MatchManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private int targetScore = 3;   // best of 3 ? llega a 3
    [SerializeField] private float intermissionSeconds = 2f; // pausa entre ronda y ronda

    [Header("Opcional: teleports")]
    [Tooltip("Teletransporta al 'lobby interno' al terminar una ronda (puede ser el mismo que ya tenés).")]
    [SerializeField] private TeleportAllToAreaOnSolved endRoundTeleport;
    [Tooltip("Teletransporta a posiciones de inicio cuando comienza cada ronda (puede ser otro Teleport).")]
    [SerializeField] private TeleportAllToAreaOnSolved startRoundTeleport;

    [Header("Hooks (opcional)")]
    [SerializeField] private NumpadRoundReset[] padsToReset;   // arrastrá ambos numpads (Blue/Red)

    [Header("Debug/State (read-only)")]
    [SerializeField] private int blueScore;
    [SerializeField] private int redScore;
    [SerializeField] private int roundNumber = 0;
    [SerializeField] private MatchState state = MatchState.Idle;

    // --- Room property keys ---
    const string RP_Blue = "scoreBlue";
    const string RP_Red = "scoreRed";
    const string RP_Round = "round";
    const string RP_State = "mstate"; // int de MatchState

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Inicializa room properties si no existen (solo una vez)
            var roomProps = new Hashtable();
            roomProps[RP_Blue] = 0;
            roomProps[RP_Red] = 0;
            roomProps[RP_Round] = 0;
            roomProps[RP_State] = (int)MatchState.Idle;
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

            // Arranca la primera ronda
            StartCoroutine(CoBeginNextRound());
        }
        else
        {
            // Clientes leen el estado actual (por si ya estaba seteado)
            PullFromRoomProps();
        }
    }

    // Llamado por el relay cuando un numpad acierta
    public void TeamScored(string team)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            // Reenvía al master por RPC para evitar condiciones de carrera
            photonView.RPC(nameof(RPC_ReportScore), RpcTarget.MasterClient, team);
            return;
        }
        ApplyScoreAndMaybeNext(team);
    }

    [PunRPC]
    void RPC_ReportScore(string team)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        ApplyScoreAndMaybeNext(team);
    }

    private void ApplyScoreAndMaybeNext(string team)
    {
        if (state != MatchState.Playing) return; // evita doble conteo

        // Suma punto al equipo correcto
        if (string.Equals(team, "Blue")) blueScore++;
        else redScore++;

        // Sube a room props para que todos vean el score
        var props = new Hashtable { { RP_Blue, blueScore }, { RP_Red, redScore } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        // Anuncia fin de ronda a todos
        state = MatchState.BetweenRounds;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { RP_State, (int)state } });

        // Teleport de fin de ronda (opcional)
        photonView.RPC(nameof(RPC_EndRoundTeleport), RpcTarget.All);

        // ¿alguien llegó a target?
        if (blueScore >= targetScore || redScore >= targetScore)
        {
            // Fin de match
            photonView.RPC(nameof(RPC_OnMatchFinished), RpcTarget.All, blueScore >= targetScore ? "Blue" : "Red");
            return;
        }

        // Siguiente ronda tras una pequeña pausa
        StartCoroutine(CoBeginNextRound());
    }

    IEnumerator CoBeginNextRound()
    {
        yield return new WaitForSeconds(intermissionSeconds);

        // Aumenta número de ronda
        roundNumber++;
        var props = new Hashtable { { RP_Round, roundNumber } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        // Reset de pads en todos
        photonView.RPC(nameof(RPC_ResetPads), RpcTarget.All);

        // Teleport de inicio (opcional)
        photonView.RPC(nameof(RPC_StartRoundTeleport), RpcTarget.All);

        // Pone estado en Playing
        state = MatchState.Playing;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { RP_State, (int)state } });
    }

    // --- RPCs visuales/acciones en todos los clientes ---

    [PunRPC]
    void RPC_EndRoundTeleport()
    {
        if (endRoundTeleport) endRoundTeleport.TeleportAll();
    }

    [PunRPC]
    void RPC_StartRoundTeleport()
    {
        if (startRoundTeleport) startRoundTeleport.TeleportAll();
    }

    [PunRPC]
    void RPC_ResetPads()
    {
        if (padsToReset == null) return;
        foreach (var p in padsToReset) if (p) p.ResetForNewRound();
    }

    [PunRPC]
    void RPC_OnMatchFinished(string winnerTeam)
    {
        state = MatchState.Finished;
        // Podés mostrar UI de victoria, bloquear input, etc.
        Debug.Log($"[Match] ¡Ganó {winnerTeam}! Final {blueScore}-{redScore}");
        // Si querés, teletransportá a un podio:
        if (endRoundTeleport) endRoundTeleport.TeleportAll();
    }

    // --- Hooks de sync para clientes que entran tarde / reconexión ---
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        PullFromRoomProps();
    }

    private void PullFromRoomProps()
    {
        var room = PhotonNetwork.CurrentRoom;
        if (room == null) return;
        if (room.CustomProperties.TryGetValue(RP_Blue, out var sb)) blueScore = (int)sb;
        if (room.CustomProperties.TryGetValue(RP_Red, out var sr)) redScore = (int)sr;
        if (room.CustomProperties.TryGetValue(RP_Round, out var rn)) roundNumber = (int)rn;
        if (room.CustomProperties.TryGetValue(RP_State, out var st)) state = (MatchState)(int)st;
    }

    // Getters (por si querés una UI)
    public int BlueScore => blueScore;
    public int RedScore => redScore;
    public int Round => roundNumber;
    public MatchState State => state;
}

