using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;

public class EndGameUI : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private const string TeamKey = "team";
    private const string LeaderboardKey = "final";

    private bool _goingBackToMenu = false;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        string winningTeam = ScoreManager.LastWinnerTeam;

        if (string.IsNullOrEmpty(winningTeam))
        {
            var props = PhotonNetwork.CurrentRoom?.CustomProperties;
            if (props != null && props.ContainsKey("winnerTeam"))
                winningTeam = props["winnerTeam"] as string;
        }

        string myTeam = GetLocalTeam();

        bool hasWinner = !string.IsNullOrEmpty(winningTeam);
        bool hasMyTeam = !string.IsNullOrEmpty(myTeam);
        bool iWon = hasWinner && hasMyTeam && winningTeam == myTeam;

        if (!hasWinner || !hasMyTeam) return;

        if (victoryPanel != null) victoryPanel.SetActive(iWon);
        if (defeatPanel != null) defeatPanel.SetActive(!iWon);

        // Enviar a LootLocker
        SubmitToLootLocker(myTeam, iWon);
    }

    private void SubmitToLootLocker(string team, bool won)
    {
        if (!LootLockerBootstrap.SessionStarted)
        {
            Debug.LogWarning("LootLocker session not started");
            return;
        }

        LeaderboardService.SubmitScore(LeaderboardKey, team, won);
    }

    private string GetLocalTeam()
    {
        var p = PhotonNetwork.LocalPlayer;
        if (p?.CustomProperties == null) return "";
        return p.CustomProperties.TryGetValue(TeamKey, out object t) ? (t as string ?? "") : "";
    }

    public void OnClick_BackToMenu()
    {
        if (_goingBackToMenu) return;
        _goingBackToMenu = true;

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            PhotonNetwork.Disconnect();
        }
    }

    public override void OnLeftRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        ScoreManager.ClearState();
        SceneManager.LoadScene(mainMenuSceneName);
    }
}