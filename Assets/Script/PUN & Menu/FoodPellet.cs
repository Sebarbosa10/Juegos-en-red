using Photon.Pun;
using UnityEngine;


public class FoodPellet : MonoBehaviour, IPunObservable
{
    public float mass = 1f;
    [HideInInspector] public bool alive = true;
    public float visualRadiusScale = 0.2f;
    public bool randomizeColor = true;

    PhotonView pv;
    SphereCollider col;
    Renderer rend;
    Vector3 netPos;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        gameObject.tag = "Pellet";
        rend = GetComponentInChildren<Renderer>();
        ApplyVisuals(true);
        netPos = transform.position;
    }

    void OnEnable()
    {
        alive = true;
        if (randomizeColor && rend != null)
        {
            Color c = Color.HSVToRGB(Random.value, 0.8f, 1f);
            rend.material.color = c;
        }
    }

    public void MasterRespawnAt(Vector3 newPos)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        pv.RPC(nameof(RPC_RespawnAtAll), RpcTarget.All, newPos);
    }

    [PunRPC]
    void RPC_RespawnAtAll(Vector3 newPos)
    {
        transform.position = newPos;
        netPos = newPos;
        alive = true;
        ApplyVisuals(true);
    }

    void ApplyVisuals(bool force = false)
    {
        float r = Mathf.Sqrt(Mathf.Max(0.01f, mass)) * visualRadiusScale;
        Vector3 t = new Vector3(r * 2f, r * 2f, r * 2f);
        transform.localScale = force ? t : Vector3.Lerp(transform.localScale, t, Time.deltaTime * 10f);
        col.radius = 0.5f;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) stream.SendNext(transform.position);
        else
        {
            netPos = (Vector3)stream.ReceiveNext();
            if (!pv.IsMine)
                transform.position = Vector3.Lerp(transform.position, netPos, Time.deltaTime * 15f);
        }
    }
}
