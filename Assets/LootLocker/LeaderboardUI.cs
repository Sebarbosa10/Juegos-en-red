using UnityEngine;
using LootLocker.Requests;
using System.Text;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private string leaderboardKey = "matchresults";
    [SerializeField] private int count = 10;
    [SerializeField] private TMPro.TextMeshProUGUI tableText;

    public void Refresh()
    {
        if (!LootLockerBootstrap.SessionStarted)
        {
            tableText.text = "Connecting...";
            return;
        }

        LootLockerSDKManager.GetScoreList(leaderboardKey, count, 0, response =>
        {
            if (!response.success)
            {
                tableText.text = "Error loading leaderboard.";
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Rank  Name          Team    Result   Score");
            sb.AppendLine("------------------------------------------------");

            var items = response.items;

            if (items == null || items.Length == 0)
            {
                sb.AppendLine("No results yet.");
            }
            else
            {
                foreach (var item in items)
                {
                    string name = "Unknown";
                    string team = "?";
                    string result = "?";

                    if (!string.IsNullOrEmpty(item.metadata))
                    {
                        Metadata meta = JsonUtility.FromJson<Metadata>(item.metadata);

                        if (meta != null)
                        {
                            name = meta.name;
                            team = meta.team;
                            result = meta.result;
                        }
                    }

                    sb.AppendLine($"{item.rank,4}  {name,-12}  {team,-6}  {result,-6}  {item.score,3}");
                }
            }

            tableText.text = sb.ToString();
        });
    }
}
