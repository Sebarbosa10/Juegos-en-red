using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class ScoreManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static ScoreManager Instance;

    private const byte ScoreEventCode = 1;
    private const byte WinEventCode = 2;

    [SerializeField] private int maxScore = 2;

    private readonly ExitGames.Client.Photon.Hashtable scores = new ExitGames.Client.Photon.Hashtable();

    public event System.Action<int, int> OnScoreUpdated;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        base.OnDisable();
    }

    private void Start()
    {
        scores["Blue"] = 0;
        scores["Red"] = 0;

        Debug.Log("[ScoreManager] Iniciado con equipos Blue=0, Red=0");
    }

    public void AddPoint(string team)
    {
        Debug.Log($"[ScoreManager] AddPoint recibido → equipo {team}");
        object[] content = new object[] { team };
        PhotonNetwork.RaiseEvent(
            ScoreEventCode,
            content,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable
        );
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == ScoreEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            string team = (string)data[0];

            if (!scores.ContainsKey(team))
            {
                Debug.LogWarning($"[ScoreManager] Equipo {team} no estaba en la tabla, inicializando en 0.");
                scores[team] = 0;
            }

            scores[team] = (int)scores[team] + 1;
            Debug.Log($"[ScoreManager] Team {team} ahora tiene {scores[team]} puntos");

            int blueScore = scores.ContainsKey("Blue") ? (int)scores["Blue"] : 0;
            int redScore = scores.ContainsKey("Red") ? (int)scores["Red"] : 0;
            OnScoreUpdated?.Invoke(blueScore, redScore);

            CheckWinCondition();
        }
    }

    private void CheckWinCondition()
    {
        foreach (var key in scores.Keys)
        {
            string team = key as string;
            int score = (int)scores[key];

            Debug.Log($"[ScoreManager] Chequeando condición: {team} tiene {score}/{maxScore}");

            if (score >= maxScore)
            {
                Debug.Log($"[ScoreManager] Equipo {team} alcanzó el puntaje máximo  WIN");
                RaiseWinEvent(team);
                break;
            }
        }
    }

    private void RaiseWinEvent(string winningTeam)
    {
        object[] content = new object[] { winningTeam };
        PhotonNetwork.RaiseEvent(
            WinEventCode,
            content,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable
        );
    }

    public void ResetScores()
    {
        scores["Blue"] = 0;
        scores["Red"] = 0;

        int blueScore = (int)scores["Blue"];
        int redScore = (int)scores["Red"];

        OnScoreUpdated?.Invoke(blueScore, redScore);
        Debug.Log("[ScoreManager] Scores reseteados a 0 - 0");
    }
}
