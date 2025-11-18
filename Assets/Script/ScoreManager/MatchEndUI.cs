using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;

public class MatchEndUI : MonoBehaviourPunCallbacks
{
    public GameObject endPanel;
    public TMP_Text winnerText;
    public TMP_Text countdownText;

    public float countdownSeconds = 30f;

    private bool _isCountingDown;

    public void Awake()
    {
        if (endPanel != null)
            endPanel.SetActive(false);
    }

    public override void OnEnable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnMatchEnded += HandleMatchEnded;
        }
    }

    public override void OnDisable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnMatchEnded -= HandleMatchEnded;
        }
    }

    private void HandleMatchEnded(string winningTeam)
    {
        if (_isCountingDown)
            return;

        _isCountingDown = true;

        if (endPanel != null)
            endPanel.SetActive(true);

        if (winnerText != null)
            winnerText.text = $"ha ganado el equipo {winningTeam}";

        StartCoroutine(CountdownAndExitRoutine());
    }

    private IEnumerator CountdownAndExitRoutine()
    {
        float remaining = countdownSeconds;

        while (remaining > 0f)
        {
            if (countdownText != null)
            {
                int secondsInt = Mathf.CeilToInt(remaining);
                countdownText.text = $"La sesion se cerrara en {secondsInt} segundos...";
            }

            remaining -= Time.deltaTime;
            yield return null;
        }

        
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
