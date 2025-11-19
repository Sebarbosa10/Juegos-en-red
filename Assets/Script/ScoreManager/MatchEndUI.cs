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
    public string SceneName = "MainMenu";

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
        Debug.Log($"[MatchEndUI] HandleMatchEnded winningTeam='{winningTeam}', winnerText asignado={winnerText != null}");

        if (_isCountingDown)
            return;

        _isCountingDown = true;

        if (endPanel != null)
        {
            endPanel.SetActive(true);
            Debug.Log("[MatchEndUI] endPanel activado");
        }

        if (winnerText != null)
        {
            winnerText.text = $"El equipo {winningTeam} ha ganado";
            Debug.Log($"[MatchEndUI] winnerText.text seteado a: {winnerText.text}");
        }
        else
        {
            Debug.LogWarning("[MatchEndUI] winnerText es NULL (no está asignado en el inspector)");
        }

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
                countdownText.text = $"La sesion se cerrara en {secondsInt}";
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
            SceneManager.LoadScene(SceneName);
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(SceneName);
    }
}
