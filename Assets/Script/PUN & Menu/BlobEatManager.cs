using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;


public class BlobEatManager : MonoBehaviourPunCallbacks
{
    public static PhotonView InstancePV { get; private set; }
    public float playerEatMargin = 1.1f;
    public float preyMassRetention = 0.95f;
    public float respawnMass = 10f;
    public Vector2 respawnAreaXZ = new(40, 40);

    void Awake() => InstancePV = GetComponent<PhotonView>();

    Vector3 RandomRespawn()
    {
        float x = Random.Range(-respawnAreaXZ.x / 2, respawnAreaXZ.x / 2);
        float z = Random.Range(-respawnAreaXZ.y / 2, respawnAreaXZ.y / 2);
        return new Vector3(x, 0, z);
    }

    [PunRPC]
    public void RPC_RequestEatPellet(int eaterViewID, int pelletViewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        var eaterPV = PhotonView.Find(eaterViewID);
        var pelletPV = PhotonView.Find(pelletViewID);
        if (!eaterPV || !pelletPV) return;
        var eater = eaterPV.GetComponent<PLayerBlob>();
        var pellet = pelletPV.GetComponent<FoodPellet>();
        if (!eater || !pellet || !pellet.alive) return;
        float eaterRadius = Mathf.Sqrt(Mathf.Max(1f, eater.mass)) * eater.radiusScale;
        float dist = Vector3.Distance(eater.transform.position, pellet.transform.position);
        if (dist <= eaterRadius)
        {
            pellet.alive = false;
            eaterPV.RPC(nameof(PLayerBlob.RPC_AddMass), eaterPV.Owner, pellet.mass);
            Vector3 np = RandomRespawn();
            pellet.MasterRespawnAt(np);
        }
    }

    [PunRPC]
    public void RPC_RequestEatPlayer(int eaterViewID, int preyViewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        var eaterPV = PhotonView.Find(eaterViewID);
        var preyPV = PhotonView.Find(preyViewID);
        if (!eaterPV || !preyPV) return;
        var eater = eaterPV.GetComponent<PLayerBlob>();
        var prey = preyPV.GetComponent<PLayerBlob>();
        if (!eater || !prey) return;
        float eaterRadius = Mathf.Sqrt(Mathf.Max(1f, eater.mass)) * eater.radiusScale;
        float dist = Vector3.Distance(eater.transform.position, prey.transform.position);
        if (dist <= eaterRadius && eater.mass >= prey.mass * playerEatMargin)
        {
            float gain = prey.mass * preyMassRetention;
            eaterPV.RPC(nameof(PLayerBlob.RPC_AddMass), eaterPV.Owner, gain);
            Vector3 np = RandomRespawn();
            preyPV.RPC(nameof(PLayerBlob.RPC_RespawnAsSmall), preyPV.Owner, respawnMass, np);
        }
    }
}
