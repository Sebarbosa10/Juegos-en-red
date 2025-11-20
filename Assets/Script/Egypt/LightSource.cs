using UnityEngine;

public class LightSource : MonoBehaviour
{
    
    public float maxDistance = 100f;
    public int maxBounces = 8;
    public LayerMask mask = ~0;

    
    public bool stopOnReceiver = true;
    public float separationEpsilon = 0.02f;

    
    public LightBeamRenderer beamRenderer;

    
    public bool debugDraw = true;
    public Color colIncident = Color.cyan;
    public Color colNormal = Color.yellow;
    public Color colReflected = Color.magenta;

    void LateUpdate()
    {
        if (!beamRenderer) return;

        var list = LightReceiverRegister.All;
        for (int i = 0; i < list.Count; i++) if (list[i]) list[i].SetLit(false);

        beamRenderer.Begin();

        Vector3 origin = transform.position;
        Vector3 dir = transform.forward.normalized;

        for (int b = 0; b <= maxBounces; b++)
        {
            if (!Physics.Raycast(origin, dir, out var hit, maxDistance, mask, QueryTriggerInteraction.Ignore))
            {
                beamRenderer.AddSegment(origin, origin + dir * maxDistance);
                break;
            }

            beamRenderer.AddSegment(origin, hit.point);

            if (debugDraw) Debug.DrawRay(origin, dir * hit.distance, colIncident, 0f, false);

            if (hit.collider.GetComponentInParent<LightReceiver>() is { } recv)
            {
                recv.SetLit(true);
                if (stopOnReceiver) break;
                origin = hit.point + dir * separationEpsilon;
                continue;
            }

            var refl = hit.collider.GetComponentInParent<MirrorSurface>();
            if (refl != null)
            {
                Vector3 n = refl.GetWorldNormal();  
                if (debugDraw)
                {
                    Debug.DrawRay(hit.point, n * 0.4f, colNormal, 0f, false);
                    Debug.DrawRay(hit.point, n * 0.4f, colReflected, 0f, false);
                }
                dir = n;                         
                origin = hit.point + dir * separationEpsilon;  
                continue;
            }

            break;
        }

        beamRenderer.End();
    }
}
