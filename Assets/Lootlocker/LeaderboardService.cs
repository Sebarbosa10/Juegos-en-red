using LootLocker.Requests;
using UnityEngine;

public class LeaderboardService : MonoBehaviour
{
    public static void SubmitScore(string leaderboardKey, string team, bool won, System.Action<bool> onDone = null)
    {
        int score = won ? 1 : 0;
        string metadata = $"{team},{(won ? "Win" : "Loss")}";

        LootLockerSDKManager.SubmitScore("", score, leaderboardKey, metadata, response =>
        {
            if (!response.success)
            {
                Debug.LogError("Fallo el score");
                onDone?.Invoke(false);
                return;
            }
            Debug.Log($"Score enviado: {team} - {(won ? "Win" : "Loss")}");
            onDone?.Invoke(true);
        });
    }
}