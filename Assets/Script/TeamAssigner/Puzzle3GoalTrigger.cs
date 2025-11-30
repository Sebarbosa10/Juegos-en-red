using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Puzzle3GoalTrigger : MonoBehaviour
{
    private bool alreadyScored = false;

    private const string TeamKey = "team";

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyScored) return;

        var pv = other.GetComponentInParent<PhotonView>();
        if (pv == null || !pv.IsMine) return;

        string team = GetTeamOf(pv.Owner);
        if (string.IsNullOrEmpty(team)) return;
        if (!PhotonNetwork.IsMasterClient) return;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPoint(team);  
            Debug.Log($"[Puzzle3GoalTrigger] Equipo {team} completó Puzzle 3 (+1).");
        }

       
        alreadyScored = true;
    }

    private string GetTeamOf(Player p)
    {
        if (p?.CustomProperties == null) return "";
        return p.CustomProperties.TryGetValue(TeamKey, out object value)
            ? (value as string ?? "")
            : "";
    }
}
