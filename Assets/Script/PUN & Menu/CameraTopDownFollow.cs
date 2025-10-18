using Photon.Pun;
using UnityEngine;



public class CameraTopDownFollow : MonoBehaviour
{
    public Transform target;          
    public float baseHeight = 25f;
    public float heightPerSqrtMass = 1.2f;
    public float followLerp = 10f;
    public Vector3 planarOffset = Vector3.zero;

    
    private PLayerBlob blob;

    void Start()
    {
        
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        TryCacheBlob();
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (blob == null) TryCacheBlob();

        float m = (blob != null) ? Mathf.Max(1f, blob.mass) : 1f;
        float height = baseHeight + Mathf.Sqrt(m) * heightPerSqrtMass;

        Vector3 wanted = new Vector3(
            target.position.x + planarOffset.x,
            height,
            target.position.z + planarOffset.z
        );

        transform.position = Vector3.Lerp(transform.position, wanted, Time.deltaTime * followLerp);
    }

    void TryCacheBlob()
    {
        if (target == null) return;
        
        blob = target.GetComponent<PLayerBlob>();
        if (blob == null) blob = target.GetComponentInParent<PLayerBlob>();
    }
}
