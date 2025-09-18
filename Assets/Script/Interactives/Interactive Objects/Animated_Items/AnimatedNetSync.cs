using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class AnimatedNetSync : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private AnimatedInteractable target;
    [SerializeField] private bool useBufferedRPCs = true;

    private void Reset()
    {
        if (target == null) target = GetComponent<AnimatedInteractable>();
    }

    public void InteractNetworked()
    {
        if (target == null) return;

        if (AnimatedManager.Instance.IsAnimating(target)) return;


        if (useBufferedRPCs)
        {
            photonView.RPC(nameof(RPC_HandleInteraction),
                RpcTarget.AllBuffered,
                (int)target.mode,
                (int)target.type,
                target.axis,
                target.amount,
                target.speed,
                target.isActive,
                target.stepCount);
        }
        else
        {
            photonView.RPC(nameof(RPC_HandleInteraction),
                RpcTarget.AllViaServer,
                (int)target.mode,
                (int)target.type,
                target.axis,
                target.amount,
                target.speed,
                target.isActive,
                target.stepCount);
        }
    }

    [PunRPC]
    private void RPC_HandleInteraction(int mode, int type, Vector3 axis, float amount, float speed, bool isActive, int stepCount, PhotonMessageInfo _info)
    {
        if (target == null) return;

        target.mode = (AnimatedMode)mode;
        target.type = (AnimatedType)type;
        target.axis = axis;
        target.amount = amount;
        target.speed = speed;
        target.isActive = isActive;
        target.stepCount = stepCount;


        AnimatedManager.Instance.HandleInteraction(target);
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (target == null) return;

        if (stream.IsWriting)
        {
            stream.SendNext(target.isActive);
            stream.SendNext(target.stepCount);
            stream.SendNext(target.transform.localPosition);
            stream.SendNext(target.transform.localRotation);
        }
        else
        {
            target.isActive = (bool)stream.ReceiveNext();
            target.stepCount = (int)stream.ReceiveNext();
            var pos = (Vector3)stream.ReceiveNext();
            var rot = (Quaternion)stream.ReceiveNext();


            if (!AnimatedManager.Instance.IsAnimating(target))
            {
                target.transform.localPosition = pos;
                target.transform.localRotation = rot;
            }
        }
    }
}
