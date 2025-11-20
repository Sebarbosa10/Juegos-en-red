using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;

public class EndGameUI : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;

    [Header("Escenas")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private const string TeamKey = "team";
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

        Debug.Log($"[EndGameUI] winnerTeam={winningTeam}, myTeam={myTeam}, iWon={iWon}");

        if (!hasWinner || !hasMyTeam) return;

        if (victoryPanel != null) victoryPanel.SetActive(iWon);
        if (defeatPanel != null) defeatPanel.SetActive(!iWon);
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
            
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    public override void OnLeftRoom()
    {
        
        ScoreManager.ClearState();

        
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
