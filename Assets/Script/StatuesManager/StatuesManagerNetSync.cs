using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class StatuesManagerNetSync : MonoBehaviourPun, IPunObservable
{
    [Header("Refs")]
    [SerializeField] private StatuesManager statuesManager;

    [SerializeField] private Collider[] statueCollidersToDisable;


    private bool solvedBroadcasted = false;

    private void Reset()
    {
        if (statuesManager == null) statuesManager = GetComponent<StatuesManager>();
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (solvedBroadcasted) return;


        if (statuesManager.IsSolved)
        {
            solvedBroadcasted = true;
            statuesManager.onSolved?.Invoke();
            photonView.RPC(nameof(RPC_SolvedAll), RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void RPC_SolvedAll()
    {
        solvedBroadcasted = true;

    
        if (statueCollidersToDisable != null)
            foreach (var c in statueCollidersToDisable)
                if (c) c.enabled = false;

        statuesManager.onSolved?.Invoke();
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) 
        {
            stream.SendNext(solvedBroadcasted);
        }
        else
        {
            bool wasSolved = solvedBroadcasted;
            solvedBroadcasted = (bool)stream.ReceiveNext();
            if (!wasSolved && solvedBroadcasted)
            {
                RPC_SolvedAll(); 
            }
        }
    }
}
