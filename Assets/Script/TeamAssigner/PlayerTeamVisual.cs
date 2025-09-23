using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerTeamVisual : MonoBehaviourPunCallbacks
{
    [Header("Materials")]
    [SerializeField] private Material blueMat;
    [SerializeField] private Material redMat;

    [Header("Renderers a pintar")]
    [SerializeField] private Renderer[] renderersToTint; 
    private const string TeamKey = "team";
    private const string TeamBlue = "Blue";
    private const string TeamRed = "Red";

    void Awake()
    {
       
        if (renderersToTint == null || renderersToTint.Length == 0)
        {
            var r = GetComponentInChildren<Renderer>();
            renderersToTint = r ? new Renderer[] { r } : new Renderer[0];
        }
    }

    void Start()
    {
        ApplyTeamNow();
    }

    public override void OnPlayerPropertiesUpdate(Player target, PhotonHashtable changedProps)
    {
        
        if (target == photonView.Owner && changedProps != null && changedProps.ContainsKey(TeamKey))
        {
            ApplyTeamNow();
        }
    }

    private void ApplyTeamNow()
    {
        var owner = photonView.Owner;
        if (owner == null || owner.CustomProperties == null) return;
        if (!owner.CustomProperties.ContainsKey(TeamKey)) return;

        string team = owner.CustomProperties[TeamKey] as string;

        
        Material targetMat = (team == TeamBlue) ? blueMat : redMat;
        if (targetMat == null) return;

        foreach (var r in renderersToTint)
        {
            if (!r) continue;
            
            var mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = targetMat;
            r.materials = mats;
        }

       
    }
}
