using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PuzzleGoalTrigger : MonoBehaviour
{
    private bool alreadyScored = false;

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
            ScoreManager.Instance.AddPoint(team);
            Debug.Log($"[PuzzleGoalTrigger] Equipo {team} alcanzó el objetivo y recibió +2 puntos.");
        }

        alreadyScored = true;
    }

    private string GetTeamOf(Player p)
    {
        if (p?.CustomProperties == null) return "";
        return p.CustomProperties.TryGetValue("team", out object value)
            ? (value as string ?? "")
            : "";
    }
}
