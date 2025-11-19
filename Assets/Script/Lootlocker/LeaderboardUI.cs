using UnityEngine;
using TMPro;
using System.Text;
using LootLocker.Requests;

[System.Serializable]
public class LeaderboardMeta
{
    public string name;
    public string team;
    public string result;
}

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private string leaderboardKey = "jueveskey";
    [SerializeField] private int count = 20;
    [SerializeField] private TextMeshProUGUI tableText;

    public void Refresh()
    {
        if (!LootLockerBootstrap.SessionStarted)
        {
            tableText.text = "Logging in...";
            return;
        }

        LootLockerSDKManager.GetScoreList(leaderboardKey, count, 0, response =>
        {
            if (!response.success)
            {
                tableText.text = "Error loading leaderboard...";
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Rank   Name          Team     Result");
            sb.AppendLine("--------------------------------------");

            foreach (var item in response.items)
            {
                // metadata raw JSON string
                string raw = item.metadata;

                string name = "Unknown";
                string team = "-";
                string result = "-";

                if (!string.IsNullOrEmpty(raw))
                {
                    try
                    {
                        LeaderboardMeta meta = JsonUtility.FromJson<LeaderboardMeta>(raw);

                        if (meta != null)
                        {
                            if (!string.IsNullOrEmpty(meta.name)) name = meta.name;
                            if (!string.IsNullOrEmpty(meta.team)) team = meta.team;
                            if (!string.IsNullOrEmpty(meta.result)) result = meta.result;
                        }
                    }
                    catch
                    {
                        Debug.LogWarning("Metadata JSON parse fail: " + raw);
                    }
                }

                sb.AppendLine($"{item.rank,2}     {name,-12}  {team,-6}   {result}");
            }

            tableText.text = sb.ToString();
        });
    }
}
