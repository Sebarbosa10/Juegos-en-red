using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon; 
using System.Collections;

public class ScoreManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static ScoreManager Instance;

    private const byte ScoreEventCode = 1;
    private const byte WinEventCode = 1;

    private int maxScore = 1; // Mejor de 3
    private readonly ExitGames.Client.Photon.Hashtable scores = new ExitGames.Client.Photon.Hashtable();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void Start()
    {
        PhotonNetwork.AddCallbackTarget(this);
        scores["blue"] = 0;
        scores["red"] = 0;

        Debug.Log("[ScoreManager] Iniciado con equipos blue=0, red=0");
    }

    public void AddPoint(string team)
    {
        Debug.Log($"[ScoreManager] Se pidió agregar un punto al equipo: {team}");
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

            CheckWinCondition();
        }
        else if (photonEvent.Code == WinEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            string winningTeam = (string)data[0];

            Debug.Log($"[ScoreManager] Evento de victoria recibido. Ganador: {winningTeam}");
            StartCoroutine(GoToEndScreens(winningTeam));
        }
    }

    private void CheckWinCondition()
    {
        foreach (var key in scores.Keys)
        {
            string team = key as string;
            int score = (int)scores[key];

            Debug.Log($"[ScoreManager] Chequeando condición de victoria: {team} tiene {score}/{maxScore}");

            if (score >= maxScore)
            {
                Debug.Log($"[ScoreManager] Equipo {team} alcanzó el puntaje máximo. Disparando evento de victoria.");
                RaiseWinEvent(team);
                break;
            }
        }
    }

    private void RaiseWinEvent(string winningTeam)
    {
        Debug.Log($"[ScoreManager] Enviando evento de victoria. Ganador: {winningTeam}");
        object[] content = new object[] { winningTeam };
        PhotonNetwork.RaiseEvent(
            WinEventCode,
            content,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable
        );
    }

    private IEnumerator GoToEndScreens(string winningTeam)
    {
        Debug.Log("[ScoreManager] Juego terminado. Transición en 5 segundos...");
        yield return new WaitForSeconds(5f);

        string myTeam = "";
        var p = PhotonNetwork.LocalPlayer;
        if (p?.CustomProperties != null && p.CustomProperties.TryGetValue("team", out object t))
            myTeam = t as string ?? "";

        Debug.Log($"[ScoreManager] Soy del equipo {myTeam}. El ganador es {winningTeam}");

        if (myTeam == winningTeam)
        {
            Debug.Log("[ScoreManager] Soy del equipo ganador  cargando WinScreen");
            PhotonNetwork.LoadLevel("WinScreen");
        }
        else
        {
            Debug.Log("[ScoreManager] Soy del equipo perdedor  cargando LoseScreen");
            PhotonNetwork.LoadLevel("LoseScreen");
        }
    }
}
