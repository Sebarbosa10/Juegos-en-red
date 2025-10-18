using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;


public class PLayerBlob : MonoBehaviour, IPunObservable
{
    public float baseSpeed = 10f;
    public float accel = 30f;
    public float decel = 25f;
    public float groundY = 0f;
    public float mass = 10f;
    public float radiusScale = 0.35f;
    public float speedMassExponent = 0.35f;
    public Transform visualRoot;
    public float visualHeight = 0.2f;
    public LayerMask eatScanMask = ~0;

    PhotonView pv;
    Rigidbody rb;
    SphereCollider col;
    Vector3 velocity;
    Vector3 netPos;
    float netMass;
    float lerpPosSpeed = 12f;
    float lerpMassSpeed = 10f;
    float nextScan;
    const float scanInterval = 0.1f;
    const float requestCooldown = 0.2f;
    readonly Dictionary<int, float> recentRequests = new();

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<SphereCollider>();
        rb.isKinematic = true;
        rb.useGravity = false;
        col.isTrigger = true;
        gameObject.tag = "Player";
        netPos = transform.position;
        netMass = mass;
        ApplyVisuals(true);
    }

    void Update()
    {
        if (pv.IsMine)
        {
            LocalUpdate();
            if (Time.time >= nextScan)
            {
                nextScan = Time.time + scanInterval;
                ScanAndRequestEats();
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, netPos, Time.deltaTime * lerpPosSpeed);
            mass = Mathf.Lerp(mass, netMass, Time.deltaTime * lerpMassSpeed);
            ApplyVisuals();
        }
    }

    void LocalUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0, v);
        if (input.sqrMagnitude > 1f) input.Normalize();
        float maxSpeed = baseSpeed / Mathf.Pow(Mathf.Max(1f, mass), speedMassExponent);
        Vector3 targetVel = input * maxSpeed;
        float blend = (input.sqrMagnitude > 0.0001f) ? accel : decel;
        velocity = Vector3.MoveTowards(velocity, targetVel, blend * Time.deltaTime);
        transform.position += velocity * Time.deltaTime;
        if (visualRoot && velocity.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(velocity.normalized, Vector3.up);
            visualRoot.rotation = Quaternion.Slerp(visualRoot.rotation, look, Time.deltaTime * 12f);
        }
        Vector3 p = transform.position; p.y = groundY; transform.position = p;
        ApplyVisuals();
    }

    void ApplyVisuals(bool force = false)
    {
        float radius = Mathf.Sqrt(Mathf.Max(1f, mass)) * radiusScale;
        float d = radius * 2f;
        if (visualRoot)
        {
            Vector3 t = new Vector3(d, visualHeight, d);
            visualRoot.localScale = force ? t : Vector3.Lerp(visualRoot.localScale, t, Time.deltaTime * 10f);
        }
        else
        {
            Vector3 s = transform.localScale; s.x = s.z = d; transform.localScale = s;
        }
        col.radius = radius;
    }

    void ScanAndRequestEats()
    {
        float radius = Mathf.Sqrt(Mathf.Max(1f, mass)) * radiusScale;
        var hits = Physics.OverlapSphere(transform.position, radius, eatScanMask, QueryTriggerInteraction.Collide);
        PhotonView mgr = BlobEatManager.InstancePV;
        if (!mgr) return;
        foreach (var c in hits)
        {
            if (!c) continue;
            Transform t = c.transform;
            Transform root = t.root != null ? t.root : t;
            if (root == transform.root) continue;
            PhotonView anyPV = root.GetComponent<PhotonView>() ?? t.GetComponent<PhotonView>();
            if (anyPV == null) continue;
            if (root.CompareTag("Pellet") || t.CompareTag("Pellet"))
            {
                if (CanRequest(anyPV.ViewID))
                {
                    recentRequests[anyPV.ViewID] = Time.time;
                    mgr.RPC(nameof(BlobEatManager.RPC_RequestEatPellet), RpcTarget.MasterClient, pv.ViewID, anyPV.ViewID);
                }
                continue;
            }
            if ((root.CompareTag("Player") || t.CompareTag("Player")) && anyPV != pv)
            {
                if (CanRequest(anyPV.ViewID))
                {
                    recentRequests[anyPV.ViewID] = Time.time;
                    mgr.RPC(nameof(BlobEatManager.RPC_RequestEatPlayer), RpcTarget.MasterClient, pv.ViewID, anyPV.ViewID);
                }
            }
        }
    }

    bool CanRequest(int id) => !recentRequests.TryGetValue(id, out var t) || Time.time - t > requestCooldown;

    [PunRPC]
    public void RPC_AddMass(float delta)
    {
        mass = Mathf.Max(1f, mass + delta);
        ApplyVisuals(true);
    }

    [PunRPC]
    public void RPC_RespawnAsSmall(float newMass, Vector3 newPos)
    {
        mass = Mathf.Max(1f, newMass);
        transform.position = new Vector3(newPos.x, groundY, newPos.z);
        velocity = Vector3.zero;
        ApplyVisuals(true);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(mass);
        }
        else
        {
            netPos = (Vector3)stream.ReceiveNext();
            netMass = (float)stream.ReceiveNext();
        }
    }
}
