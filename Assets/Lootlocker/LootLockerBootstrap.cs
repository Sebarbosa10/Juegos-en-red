using LootLocker.Requests;
using UnityEngine;

public class LootLockerBootstrap : MonoBehaviour
{
    public static bool SessionStarted { get; private set; }
    public static event System.Action OnSessionStarted;

    [SerializeField] string playerIdentifier = "1";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        StartGuest();
    }

    void StartGuest()
    {
        LootLockerSDKManager.StartGuestSession(playerIdentifier, response =>
        {
            if (!response.success)
            {
                Debug.LogError("LootLocker: Fallo conexion");
                return;
            }
            SessionStarted = true;
            Debug.Log("LootLocker: Conectado");
            OnSessionStarted?.Invoke();
        });
    }
}