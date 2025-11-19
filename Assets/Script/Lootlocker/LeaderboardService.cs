using UnityEngine;
using LootLocker.Requests;

public static class LeaderboardService
{
    public static void SubmitResult(string leaderboardKey, string playerName, string team, bool playerWon)
    {
        if (!LootLockerBootstrap.SessionStarted)
        {
            Debug.LogError("Cannot submit score: LootLocker session not started.");
            return;
        }

        string result = playerWon ? "Win" : "Lose";

        string metadata = $"{{\"name\":\"{playerName}\",\"team\":\"{team}\",\"result\":\"{result}\"}}";

        LootLockerSDKManager.SubmitScore(
            playerName,                 // member_id
            playerWon ? 1 : 0,          // score: 1 = win, 0 = lose
            leaderboardKey,             // leaderboard key
            metadata,
            response =>
            {
                if (!response.success)
                {
                    Debug.LogError("Failed to submit leaderboard result.");
                    return;
                }

                Debug.Log("Leaderboard result submitted successfully.");
            }
        );
    }
}
