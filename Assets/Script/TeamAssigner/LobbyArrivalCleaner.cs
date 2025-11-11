using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(PhotonView))]
public class LobbyArrivalCleaner : MonoBehaviourPunCallbacks
{
    private const string LobbyCycleKey = "lobbyCycle";
    private const string ReadyCycleKey = "readyCycle";

    private void OnEnable()
    {
        
        Invoke(nameof(DoResetAll), 0.1f);
    }

    private void DoResetAll()
    {
        if (!PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom) return;

        
        photonView.RPC(nameof(RPC_ResetReadyCycleAll), RpcTarget.All);
        Debug.Log("[LobbyCleaner] Reset readyCycle -> -1 para todos");
    }

    [PunRPC]
    private void RPC_ResetReadyCycleAll()
    {
        var me = PhotonNetwork.LocalPlayer;
        if (me == null) return;

        
        me.SetCustomProperties(new PhotonHashtable { { ReadyCycleKey, -1 } });
    }
}
