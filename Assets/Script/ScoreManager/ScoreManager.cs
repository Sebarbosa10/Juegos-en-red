using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class ScoreManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static ScoreManager Instance;

    private const byte ScoreEventCode = 1;
    private const byte WinEventCode = 2;

    private readonly ExitGames.Client.Photon.Hashtable scores =
        new ExitGames.Client.Photon.Hashtable();

    [SerializeField] private int maxScore = 3;
    [SerializeField] private string endGameSceneName = "EndGame";

    public static string LastWinnerTeam { get; private set; }

    public event System.Action<int, int> OnScoreUpdated;

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

    private void Start()
    {
        PhotonNetwork.AddCallbackTarget(this);

        scores["Blue"] = 0;
        scores["Red"] = 0;

        OnScoreUpdated?.Invoke(0, 0);
    }

    private void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // -------------------------
    // RESET SCORES
    // -------------------------
    public void ResetScores()
    {
        scores["Blue"] = 0;
        scores["Red"] = 0;

        OnScoreUpdated?.Invoke(0, 0);
    }

    // -------------------------
    // CLEAR STATE
    // -------------------------
    public static void ClearState()
    {
        if (Instance != null)
        {
            Instance.ResetScores();
            LastWinnerTeam = null;
        }
    }

    // -------------------------

    public void AddPoint(string team)
    {
        object[] content = { team };

        PhotonNetwork.RaiseEvent(
            ScoreEventCode,
            content,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == ScoreEventCode)
        {
            string team = (string)((object[])photonEvent.CustomData)[0];

            scores[team] = (int)scores[team] + 1;

            int blueScore = (int)scores["Blue"];
            int redScore = (int)scores["Red"];

            OnScoreUpdated?.Invoke(blueScore, redScore);

            CheckWinCondition();
        }
        else if (photonEvent.Code == WinEventCode)
        {
            string winningTeam = (string)((object[])photonEvent.CustomData)[0];
            LastWinnerTeam = winningTeam;

            string myTeam =
                PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("team")
                ? (string)PhotonNetwork.LocalPlayer.CustomProperties["team"]
                : "Unknown";

            bool iWon = (myTeam == winningTeam);

            // SUBMIT SCORE FOR ALL PLAYERS BEFORE SCENE CHANGE
            LeaderboardService.SubmitMatchResult(
                "matchresults",
                PhotonNetwork.LocalPlayer.NickName,
                myTeam,
                iWon,
                success =>
                {
                    // Only master changes scene AFTER everyone had time to submit
                    if (PhotonNetwork.IsMasterClient)
                        StartCoroutine(DelayedSceneLoad());
                });
        }
    }

    private void CheckWinCondition()
    {
        foreach (var key in scores.Keys)
        {
            string team = key as string;
            int score = (int)scores[key];

            if (score >= maxScore)
            {
                if (PhotonNetwork.IsMasterClient)
                    RaiseWinEvent(team);

                break;
            }
        }
    }

    private void RaiseWinEvent(string winningTeam)
    {
        object[] content = { winningTeam };

        PhotonNetwork.RaiseEvent(
            WinEventCode,
            content,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable);
    }

    private System.Collections.IEnumerator DelayedSceneLoad()
    {
        // Wait for all 4 players to submit their result (win/lose)
        yield return new WaitForSeconds(1f);

        PhotonNetwork.LoadLevel(endGameSceneName);
    }
}
