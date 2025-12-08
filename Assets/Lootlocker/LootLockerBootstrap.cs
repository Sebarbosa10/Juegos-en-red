using LootLocker.Requests;
using UnityEngine;

public class LootLockerBootstrap : MonoBehaviour
{
    public static bool SessionStarted { get; private set; }
    public static event System.Action OnSessionStarted;

    private const string PlayerIdKey = "LootLockerPlayerId";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        StartGuest();
    }

    void StartGuest()
    {
        string playerId = GetOrCreatePlayerId();

        LootLockerSDKManager.StartGuestSession(playerId, response =>
        {
            if (!response.success)
            {
                Debug.LogError("LootLocker: Fallo conexion");
                return;
            }
            SessionStarted = true;
            Debug.Log($"LootLocker: Conectado con ID {playerId}");
            OnSessionStarted?.Invoke();
        });
    }

    string GetOrCreatePlayerId()
    {
        if (!PlayerPrefs.HasKey(PlayerIdKey))
        {
            string newId = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString(PlayerIdKey, newId);
            PlayerPrefs.Save();
        }
        return PlayerPrefs.GetString(PlayerIdKey);
    }
}