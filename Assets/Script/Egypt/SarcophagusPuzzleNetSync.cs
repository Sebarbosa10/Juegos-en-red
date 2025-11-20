using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class SarcophagusPuzzleNetSync : MonoBehaviourPun, IPunObservable
{
    
    [SerializeField] private SarcophagusPuzzle puzzle;
    [SerializeField] private SarcophagusLid[] lids;

    private readonly HashSet<int> reserved = new HashSet<int>();
    private readonly List<int> attemptOrder = new List<int>();
    private bool solved;
    private bool resetting;

    private void Reset()
    {
        if (puzzle == null) puzzle = GetComponent<SarcophagusPuzzle>();
        if ((lids == null || lids.Length == 0) && puzzle != null) lids = puzzle.lids;
    }

    public void RequestClick(int lidIndex)
    {
        if (PhotonNetwork.IsMasterClient)
            HandleClickAsMaster(lidIndex);
        else
            photonView.RPC(nameof(RPC_RequestClick), RpcTarget.MasterClient, lidIndex);
    }

    [PunRPC]
    private void RPC_RequestClick(int lidIndex, PhotonMessageInfo _)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        HandleClickAsMaster(lidIndex);
    }

    private void HandleClickAsMaster(int lidIndex)
    {
        if (Invalid(lidIndex)) return;
        if (solved || resetting) return;
        if (reserved.Contains(lidIndex)) return;
        if (attemptOrder.Count >= puzzle.correctOrder.Length) return;

        var lid = lids[lidIndex];
        var ai = lid.animatedLid;

        Vector3 startPos = ai.transform.localPosition;
        Quaternion startRot = ai.transform.localRotation;

        Vector3 endPos = startPos;
        Quaternion endRot = startRot;

        float dir = +1f;
        if (ai.type == AnimatedType.Rotation)
            endRot = startRot * Quaternion.Euler(ai.axis * ai.amount * dir);
        else
            endPos = startPos + ai.axis.normalized * ai.amount * dir;

        float duration = Mathf.Max(0.0001f, 1f / Mathf.Max(0.0001f, ai.speed));

        reserved.Add(lidIndex);
        photonView.RPC(nameof(RPC_PlayAbsolute),
                       RpcTarget.All,
                       lidIndex,
                       startPos, startRot, endPos, endRot, duration);

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
                    anyAnimating = true; break;
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
    private void RPC_PlayAbsolute(int lidIndex,
                                  Vector3 startPos, Quaternion startRot,
                                  Vector3 endPos, Quaternion endRot,
                                  float duration)
    {
        if (Invalid(lidIndex)) return;
        var lid = lids[lidIndex];
        var ai = lid.animatedLid;

        lid.Lock();

        AnimatedManager.Instance.AnimateAbsolute(ai, startPos, startRot, endPos, endRot, duration, markActive: true);
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

            int n = (lids != null) ? lids.Length : 0;
            stream.SendNext(n);
            for (int i = 0; i < n; i++)
            {
                var lid = lids[i];
                bool isLocked = lid != null ? IsLockedAuthoritative(i) : false;
                stream.SendNext(isLocked);

                if (lid != null && lid.animatedLid != null)
                {
                    var tr = lid.animatedLid.transform;
                    stream.SendNext(tr.localPosition);
                    stream.SendNext(tr.localRotation);
                }
                else
                {
                    stream.SendNext(Vector3.zero);
                    stream.SendNext(Quaternion.identity);
                }
            }
        }
        else 
        {
            solved = (bool)stream.ReceiveNext();

            int n = (int)stream.ReceiveNext();
            for (int i = 0; i < n; i++)
            {
                bool isLocked = (bool)stream.ReceiveNext();
                Vector3 pos = (Vector3)stream.ReceiveNext();
                Quaternion rot = (Quaternion)stream.ReceiveNext();

                if (lids == null || i >= lids.Length || lids[i] == null) continue;

                var lid = lids[i];
                if (isLocked) lid.Lock(); else lid.Unlock();

                var ai = lid.animatedLid;
                if (ai != null && !AnimatedManager.Instance.IsAnimating(ai))
                {
                    ai.transform.localPosition = pos;
                    ai.transform.localRotation = rot;
                }
            }


        }
    }

    private bool Invalid(int i) => (lids == null || i < 0 || i >= lids.Length || lids[i] == null);
    private bool IsLockedAuthoritative(int i) => reserved.Contains(i) || (attemptOrder.Contains(i));
}
