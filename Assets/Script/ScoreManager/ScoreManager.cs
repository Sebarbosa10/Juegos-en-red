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

    private int playersReported = 0;

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

    public void ResetScores()
    {
        scores["Blue"] = 0;
        scores["Red"] = 0;

        OnScoreUpdated?.Invoke(0, 0);
    }

    public static void ClearState()
    {
        if (Instance != null)
        {
            Instance.ResetScores();
            LastWinnerTeam = null;
        }
    }

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

            StartCoroutine(SubmitResultAndNotify(winningTeam));
        }
    }

    private System.Collections.IEnumerator SubmitResultAndNotify(string winningTeam)
    {
        string myTeam =
            PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("team")
            ? (string)PhotonNetwork.LocalPlayer.CustomProperties["team"]
            : "Unknown";

        bool iWon = (myTeam == winningTeam);

        bool done = false;

        LeaderboardService.SubmitMatchResult(
            "matchresults",
            PhotonNetwork.LocalPlayer.NickName,
            myTeam,
            iWon,
            _ => { done = true; }
        );

        while (!done) yield return null;

        photonView.RPC(nameof(RPC_PlayerReported), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void RPC_PlayerReported()
    {
        playersReported++;

        if (playersReported >= PhotonNetwork.PlayerList.Length)
        {
            PhotonNetwork.LoadLevel(endGameSceneName);
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
}
