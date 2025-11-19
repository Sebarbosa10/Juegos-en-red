using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;

public class MatchEndUI : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public GameObject endPanel;
    public TMP_Text winnerText;
    public TMP_Text countdownText;

    [Header("Settings")]
    public float countdownSeconds = 30f;
    public string mainMenu = "MainMenu";

    private bool _isCountingDown;

    private void Awake()
    {
        if (endPanel != null)
            endPanel.SetActive(false);
    }

    public override void OnEnable()
    {
        base.OnEnable();

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

        base.OnDisable();
    }

    private void HandleMatchEnded(string winningTeam)
    {
        if (_isCountingDown)
            return;

        _isCountingDown = true;

        if (endPanel != null)
            endPanel.SetActive(true);

        if (winnerText != null)
            winnerText.text = $"El equipo {winningTeam} ha ganado";

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
                countdownText.text = $"La sesión se cerrará en {secondsInt}";
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
            SceneManager.LoadScene(mainMenu);
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(mainMenu);
    }
}
