using Photon.Pun;
using UnityEngine;

 
public class SplitController : MonoBehaviour
{
    public int ownerActor;
    public int parentViewID = -1;
    public float mergeDelay = 1.5f;
    public float mergeDistance = 1.0f;

    float bornTime;
    PLayerBlob blob;
    PhotonView pv;

    Vector3 inertialVel;
    public float decel = 10f; // desaceleración del split

    public void SetInitialVelocity(Vector3 v) => inertialVel = v;

    public void Begin()
    {
        if (blob == null) blob = GetComponent<PLayerBlob>();
        if (pv == null) pv = GetComponent<PhotonView>();
        bornTime = Time.time;
    }

    void Awake()
    {
        blob = GetComponent<PLayerBlob>();
        pv = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (!pv.IsMine) return;

        // mover por inercia (el blob principal no usa 'velocity' en no-controlable)
        if (inertialVel.sqrMagnitude > 0.0001f)
        {
            transform.position += inertialVel * Time.deltaTime;
            inertialVel = Vector3.MoveTowards(inertialVel, Vector3.zero, decel * Time.deltaTime);
        }

        // fusionar con el padre si pasó el delay y está cerca
        if (Time.time - bornTime >= mergeDelay && parentViewID > 0)
        {
            var parentPV = PhotonView.Find(parentViewID);
            if (parentPV != null)
            {
                float dist = Vector3.Distance(parentPV.transform.position, transform.position);
                if (dist <= mergeDistance)
                {
                    // sumar masa al padre y destruir este split
                    parentPV.RPC(nameof(PLayerBlob.RPC_AddMass), parentPV.Owner, blob.mass);
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }
    }

    public bool IsGracePeriod() => (Time.time - bornTime) < mergeDelay;
}
