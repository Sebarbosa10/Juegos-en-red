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
    [SerializeField] private int totalRounds = 3;
    [SerializeField] private string endGameSceneName = "EndGame";

    private readonly PhotonHashtable scores = new PhotonHashtable();

    public static string LastWinnerTeam { get; private set; }

    public event System.Action<int, int> OnScoreUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[ScoreManager] Instancia creada");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        Debug.Log("[ScoreManager] Callback registrado");
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
        Debug.Log("[ScoreManager] Callback removido");
    }

    private void Start()
    {
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
        Debug.Log("[ScoreManager] Scores reseteados");
    }

    public static void ClearState()
    {
        if (Instance != null)
        {
            Instance.ResetScores();
        }
        LastWinnerTeam = null;
    }

    public override void OnLeftRoom()
    {
        ClearState();
    }

    public void AddPoint(string team)
    {
        Debug.LogWarning($"[ScoreManager] AddPoint llamado para equipo: {team}");

        object[] content = new object[] { team };
        PhotonNetwork.RaiseEvent(
            ScoreEventCode,
            content,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable
        );

        Debug.LogWarning($"[ScoreManager] Evento ScoreEventCode enviado");
    }

    public void OnEvent(EventData photonEvent)
    {
        Debug.LogWarning($"[ScoreManager] OnEvent recibido, Code: {photonEvent.Code}");

        if (photonEvent.Code == ScoreEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            string team = (string)data[0];

            int blueAntes = scores.ContainsKey("Blue") ? (int)scores["Blue"] : 0;
            int redAntes = scores.ContainsKey("Red") ? (int)scores["Red"] : 0;

            Debug.LogWarning($"[ScoreManager] PUNTO RECIBIDO para {team}");
            Debug.LogWarning($"[ScoreManager] Marcador ANTES: Blue={blueAntes}, Red={redAntes}");

            if (!scores.ContainsKey(team))
            {
                scores[team] = 0;
            }

            scores[team] = (int)scores[team] + 1;

            int blueDespues = scores.ContainsKey("Blue") ? (int)scores["Blue"] : 0;
            int redDespues = scores.ContainsKey("Red") ? (int)scores["Red"] : 0;

            Debug.LogWarning($"[ScoreManager] Marcador DESPUÉS: Blue={blueDespues}, Red={redDespues}");

            OnScoreUpdated?.Invoke(blueDespues, redDespues);

            CheckWinCondition();
        }
        else if (photonEvent.Code == WinEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            string winningTeam = (string)data[0];

            Debug.LogWarning($"[ScoreManager] WIN EVENT RECIBIDO - Ganador: {winningTeam}");

            LastWinnerTeam = winningTeam;

            if (PhotonNetwork.CurrentRoom != null)
            {
                var props = new PhotonHashtable { { "winnerTeam", winningTeam } };
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning($"[ScoreManager] Soy MasterClient, cargando escena: {endGameSceneName}");
                PhotonNetwork.LoadLevel(endGameSceneName);
            }
        }
    }

    private void CheckWinCondition()
    {
        int blueScore = scores.ContainsKey("Blue") ? (int)scores["Blue"] : 0;
        int redScore = scores.ContainsKey("Red") ? (int)scores["Red"] : 0;
        int totalPointsPlayed = blueScore + redScore;

        Debug.LogWarning($"[ScoreManager] CheckWinCondition: Blue={blueScore}, Red={redScore}, Total={totalPointsPlayed}/{totalRounds}");

        int pointsNeededToWin = (totalRounds / 2) + 1;

        Debug.LogWarning($"[ScoreManager] Puntos necesarios para ganar: {pointsNeededToWin}");

        if (blueScore >= pointsNeededToWin)
        {
            Debug.LogWarning($"[ScoreManager] BLUE GANA con {blueScore} puntos!");
            if (PhotonNetwork.IsMasterClient)
            {
                RaiseWinEvent("Blue");
            }
            return;
        }

        if (redScore >= pointsNeededToWin)
        {
            Debug.LogWarning($"[ScoreManager] RED GANA con {redScore} puntos!");
            if (PhotonNetwork.IsMasterClient)
            {
                RaiseWinEvent("Red");
            }
            return;
        }

        if (totalPointsPlayed >= totalRounds)
        {
            string winner = DetermineWinner(blueScore, redScore);
            Debug.LogWarning($"[ScoreManager] Todas las rondas jugadas. Ganador: {winner}");

            if (PhotonNetwork.IsMasterClient && !string.IsNullOrEmpty(winner))
            {
                RaiseWinEvent(winner);
            }
        }
        else
        {
            Debug.LogWarning($"[ScoreManager] Aún no hay ganador, quedan {totalRounds - totalPointsPlayed} rondas");
        }
    }

    private string DetermineWinner(int blueScore, int redScore)
    {
        if (blueScore > redScore) return "Blue";
        if (redScore > blueScore) return "Red";
        return "Blue"; // Empate, Blue gana por defecto
    }

    private void RaiseWinEvent(string winningTeam)
    {
        Debug.LogWarning($"[ScoreManager] RaiseWinEvent para: {winningTeam}");

        object[] content = new object[] { winningTeam };
        PhotonNetwork.RaiseEvent(
            WinEventCode,
            content,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable
        );
    }

    public int GetScore(string team)
    {
        return scores.ContainsKey(team) ? (int)scores[team] : 0;
    }

    public int GetBlueScore() => GetScore("Blue");
    public int GetRedScore() => GetScore("Red");
}