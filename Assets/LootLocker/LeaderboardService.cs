using UnityEngine;
using LootLocker.Requests;

public static class LeaderboardService
{
    public static void SubmitMatchResult(
        string leaderboardKey,
        string playerName,
        string team,
        bool playerWon,
        System.Action<bool> onDone = null)
    {
        if (!LootLockerBootstrap.SessionStarted)
        {
            Debug.LogError("LootLocker: Session not started.");
            onDone?.Invoke(false);
            return;
        }

        int score = playerWon ? 1 : 0;
        string result = playerWon ? "win" : "lose";

        Metadata meta = new Metadata()
        {
            name = playerName,
            team = team,
            result = result
        };

        string metadata = JsonUtility.ToJson(meta);

        LootLockerSDKManager.SubmitScore(
            playerName,
            score,
            leaderboardKey,
            metadata,
            response =>
            {
                if (!response.success)
                {
                    Debug.LogError("Leaderboard submit failed: " + response.errorData?.message);
                    onDone?.Invoke(false);
                    return;
                }

                Debug.Log("Leaderboard: Score submitted.");
                onDone?.Invoke(true);
            });
    }
}

[System.Serializable]
public class Metadata
{
    public string name;
    public string team;
    public string result;
}
