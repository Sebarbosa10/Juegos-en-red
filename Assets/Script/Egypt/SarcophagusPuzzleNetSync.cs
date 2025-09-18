using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class SarcophagusPuzzleNetSync : MonoBehaviourPun, IPunObservable
{
    [Header("Refs")]
    [SerializeField] private SarcophagusPuzzle puzzle;  
    [SerializeField] private SarcophagusLid[] lids;     

    private readonly HashSet<int> reserved = new HashSet<int>();
    private readonly List<int> attemptOrder = new List<int>();
    private bool solved;
    private bool resetting;

    private void Reset()
    {
        if (puzzle == null) puzzle = GetComponent<SarcophagusPuzzle>();
        if (lids == null || lids.Length == 0) lids = puzzle?.lids;
    }

    public void RequestClick(int lidIndex)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            HandleClickAsMaster(lidIndex);
        }
        else
        {
            photonView.RPC(nameof(RPC_RequestClick), RpcTarget.MasterClient, lidIndex);
        }
    }

    [PunRPC]
    private void RPC_RequestClick(int lidIndex, PhotonMessageInfo _info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        HandleClickAsMaster(lidIndex);
    }

    private void HandleClickAsMaster(int lidIndex)
    {
        if (puzzle == null || lids == null || lidIndex < 0 || lidIndex >= lids.Length) return;
        if (solved || resetting) return;

        if (reserved.Contains(lidIndex)) return;
        if (attemptOrder.Count >= puzzle.correctOrder.Length) return;


        reserved.Add(lidIndex);
        photonView.RPC(nameof(RPC_PlayLid), RpcTarget.All, lidIndex);

        attemptOrder.Add(lidIndex);


        if (attemptOrder.Count >= puzzle.correctOrder.Length)
            StartCoroutine(EvaluateAsMasterRoutine());
    }

    private IEnumerator EvaluateAsMasterRoutine()
    {

        bool anyAnimating;
        do
        {
            anyAnimating = false;
            foreach (var lid in lids)
            {
                if (lid != null && lid.animatedLid != null &&
                    AnimatedManager.Instance.IsAnimating(lid.animatedLid))
                {
                    anyAnimating = true;
                    break;
                }
            }
            yield return null;
        } while (anyAnimating);

        bool ok = attemptOrder.Count == puzzle.correctOrder.Length;
        if (ok)
        {
            for (int i = 0; i < puzzle.correctOrder.Length; i++)
                if (attemptOrder[i] != puzzle.correctOrder[i]) { ok = false; break; }
        }

        if (ok)
        {
            solved = true;
            puzzle.onSolved?.Invoke(); 
            photonView.RPC(nameof(RPC_Solved), RpcTarget.All);
        }
        else
        {
            resetting = true;
            photonView.RPC(nameof(RPC_ResetStart), RpcTarget.All, puzzle.animatedReset);
            attemptOrder.Clear();
            reserved.Clear();
            resetting = false;
        }
    }

    [PunRPC]
    private void RPC_PlayLid(int lidIndex)
    {
        if (lids == null || lidIndex < 0 || lidIndex >= lids.Length) return;
        var lid = lids[lidIndex];
        if (lid == null) return;


        lid.Lock();
        if (lid.animatedLid != null)
            AnimatedManager.Instance.HandleInteraction(lid.animatedLid);
    }

    [PunRPC]
    private void RPC_ResetStart(bool animatedReset)
    {

        if (lids == null) return;

        foreach (var lid in lids)
        {
            if (lid == null) continue;
            lid.Unlock();
            if (animatedReset) lid.AnimateBackToInitial();
            else lid.SnapToInitial();
        }
    }

    [PunRPC]
    private void RPC_Solved()
    {
        solved = true;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
           
            stream.SendNext(solved);
            var locks = new bool[lids.Length];
            for (int i = 0; i < lids.Length; i++)
            {
                locks[i] = reserved.Contains(i);
            }
            stream.SendNext(locks);
        }
        else
        {
            solved = (bool)stream.ReceiveNext();

            var locks = (bool[])stream.ReceiveNext();
            if (lids != null && locks != null && locks.Length == lids.Length)
            {
                for (int i = 0; i < lids.Length; i++)
                {
                    if (lids[i] == null) continue;
                    if (locks[i]) lids[i].Lock(); else lids[i].Unlock();
                }
            }
            //if (solved)
            //{
               
            //}
        }
    }
}
