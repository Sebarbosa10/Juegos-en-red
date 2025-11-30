using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class ScoreManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static ScoreManager Instance;

    private const byte ScoreEventCode = 1;
    private const byte WinEventCode = 2;

    [Header("Config")]
    [SerializeField] private int maxScore = 3;         
    [SerializeField] private string endGameSceneName = "EndGame";

    private readonly PhotonHashtable scores = new PhotonHashtable();

    public static string LastWinnerTeam { get; private set; }

    public event System.Action<int, int> OnScoreUpdated;

    private const string RoundIndexKey = "roundIndex";

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

    private void OnDestroy()
    {
        if (PhotonNetwork.NetworkingClient != null)
            PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void Start()
    {
        PhotonNetwork.AddCallbackTarget(this);
        scores["Blue"] = 0;
        scores["Red"] = 0;

        Debug.Log("[ScoreManager] Iniciado con Blue=0, Red=0");
        OnScoreUpdated?.Invoke(0, 0);
    }

    public void ResetScores()
    {
        scores["Blue"] = 0;
        scores["Red"] = 0;
        OnScoreUpdated?.Invoke(0, 0);
        Debug.Log("[ScoreManager] ResetScores → Blue=0, Red=0");
    }

    public static void ClearState()
    {
        if (Instance != null)
        {
            Instance.ResetScores();
        }
        LastWinnerTeam = null;
        Debug.Log("[ScoreManager] ClearState → scores reseteados y LastWinnerTeam=null");
    }

    public override void OnLeftRoom()
    {
        
        ClearState();
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
        else if (photonEvent.Code == WinEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            string winningTeam = (string)data[0];

            Debug.Log($"[ScoreManager] WinEvent recibido → ganador {winningTeam}");

            LastWinnerTeam = winningTeam;

            var props = new PhotonHashtable { { "winnerTeam", winningTeam } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel(endGameSceneName);
            }
        }
    }

    private int GetRoundIndex()
    {
        var roomProps = PhotonNetwork.CurrentRoom?.CustomProperties;
        if (roomProps != null && roomProps.ContainsKey(RoundIndexKey))
        {
            object value = roomProps[RoundIndexKey];
            if (value is int ri)
                return ri;
            if (int.TryParse(value.ToString(), out int parsed))
                return parsed;
        }

        return 1; 
    }

    private void CheckWinCondition()
    {
        int roundIndex = GetRoundIndex();
        if (roundIndex < 3)
        {
            
            Debug.Log($"[ScoreManager] roundIndex={roundIndex}, no se evalúa victoria todavía.");
            return;
        }

        foreach (var key in scores.Keys)
        {
            string team = key as string;
            int score = (int)scores[key];

            Debug.Log($"[ScoreManager] Chequeando condición: {team} tiene {score}/{maxScore} (round={roundIndex})");

            if (score >= maxScore)
            {
                Debug.Log($"[ScoreManager] Equipo {team} alcanzó el puntaje máximo → WIN (round={roundIndex})");
                if (PhotonNetwork.IsMasterClient)
                {
                    RaiseWinEvent(team);
                }
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
}
