using UnityEngine;
using LootLocker.Requests;
using Photon.Pun;

public class LootLockerBootstrap : MonoBehaviour
{
    public static bool SessionStarted { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        StartGuest();
    }

    void StartGuest()
    {
  
        string playerIdentifier = PhotonNetwork.LocalPlayer.UserId;

        LootLockerSDKManager.StartGuestSession(playerIdentifier, response =>
        {
            if (!response.success)
            {
                Debug.LogError("LootLocker: Failed to start session");
                return;
            }

            SessionStarted = true;
            Debug.Log("LootLocker session started: " + playerIdentifier);
        });
    }
}
