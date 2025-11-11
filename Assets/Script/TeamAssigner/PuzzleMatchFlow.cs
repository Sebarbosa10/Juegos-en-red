using UnityEngine;
using Photon.Pun;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class PuzzleMatchFlow : MonoBehaviourPun
{
    public static PuzzleMatchFlow Instance { get; private set; }  // 👈 singleton

    [SerializeField] private string lobbySceneName = "Lobby";
    private const string LobbyCycleKey = "lobbyCycle";

    private void Awake()
    {
        // Singleton simple por escena; si hubiera dos, destruimos el duplicado
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // (Opcional) si querés que sobreviva al cambio de escena:
        // DontDestroyOnLoad(gameObject);
    }

    public void CompleteMatch()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[PuzzleFlow] Solo el Master termina la ronda.");
            return;
        }

        int cur = 0;
        var rp = PhotonNetwork.CurrentRoom.CustomProperties;
        if (rp != null && rp.ContainsKey(LobbyCycleKey))
            cur = (int)rp[LobbyCycleKey];

        int next = cur + 1;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new PhotonHashtable { { LobbyCycleKey, next } });
        Debug.Log($"[PuzzleFlow] Fin de mapa. lobbyCycle: {cur} → {next}. Volviendo a Lobby...");

        PhotonNetwork.LoadLevel(lobbySceneName);
    }
}
