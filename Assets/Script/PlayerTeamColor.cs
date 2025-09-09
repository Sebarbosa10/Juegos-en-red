using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class PlayerTeamColor : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [Header("Renderers a colorear")]
    [SerializeField] private Renderer[] renderersToTint;

    [Header("Materiales/Colores por equipo")]
    [SerializeField] private Material yellowMat;
    [SerializeField] private Material purpleMat;
    [SerializeField] private Color fallbackYellow = new Color(1f, 0.85f, 0.1f);
    [SerializeField] private Color fallbackPurple = new Color(0.5f, 0.2f, 0.8f);

    private Team myTeam;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
       
        object[] data = photonView.InstantiationData;
        if (data != null && data.Length > 0)
            myTeam = (Team)(int)data[0];
        else
            myTeam = Team.Yellow; 

        ApplyTeamVisuals();
    }

    private void ApplyTeamVisuals()
    {
        if (renderersToTint == null || renderersToTint.Length == 0) return;

        foreach (var r in renderersToTint)
        {
            if (!r) continue;

            
            if (yellowMat && purpleMat)
            {
                r.material = (myTeam == Team.Yellow) ? yellowMat : purpleMat;
            }
            else
            {
                
                var m = r.material;
                m.color = (myTeam == Team.Yellow) ? fallbackYellow : fallbackPurple;
            }
        }
    }
}
