using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using ExitGames.Client.Photon;


using Hashtable = ExitGames.Client.Photon.Hashtable;


public enum Team
{
    Yellow = 0,
    Purple = 1
}


public class TeamAssigner : MonoBehaviourPunCallbacks
{
    private const string TEAM_KEY = "team";

    public override void OnJoinedRoom()
    {
       
        if (PhotonNetwork.IsMasterClient)
            EnsureTeamAssigned(PhotonNetwork.LocalPlayer);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        
        if (PhotonNetwork.IsMasterClient)
            EnsureTeamAssigned(newPlayer);
    }

    private void EnsureTeamAssigned(Player player)
    {
        if (player.CustomProperties.ContainsKey(TEAM_KEY))
            return; 


        
        int yellowCount = PhotonNetwork.PlayerList
            .Count(p => (int?)p.CustomProperties[TEAM_KEY] == (int)Team.Yellow);
        int purpleCount = PhotonNetwork.PlayerList
            .Count(p => (int?)p.CustomProperties[TEAM_KEY] == (int)Team.Purple);

        Team assigned;
        if (yellowCount < 2) assigned = Team.Yellow;
        else if (purpleCount < 2) assigned = Team.Purple;
        else
        {
            
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.CurrentRoom.IsOpen = false;
            Debug.LogWarning("[Teams] Ambos equipos llenos. No se asignó equipo.");
            return;
        }

        var props = new Hashtable { { TEAM_KEY, (int)assigned } };
        player.SetCustomProperties(props);

        Debug.Log($"[Teams] Asignado {assigned} a {player.NickName}");
    }
}

