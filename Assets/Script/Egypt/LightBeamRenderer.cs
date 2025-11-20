using UnityEngine;
using System.Collections.Generic;

public class LightBeamRenderer : MonoBehaviour
{
  
    public GameObject beamPrefab; 
    public int poolSize = 32;

   
    public float thickness = 0.05f;

    private readonly List<Transform> pool = new();
    private int used = 0;

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var go = Instantiate(beamPrefab, transform);
            go.SetActive(false);
            pool.Add(go.transform);
        }
    }

    public void Begin() { used = 0; }

    public void AddSegment(Vector3 a, Vector3 b)
    {
        if (used >= pool.Count) return;

        var t = pool[used++];
        Vector3 dir = b - a;
        float len = dir.magnitude;

        if (len < 1e-4f) { t.gameObject.SetActive(false); return; }

        t.gameObject.SetActive(true);
        t.position = a + dir * 0.5f; 
        t.up = dir.normalized;      
        t.localScale = new Vector3(thickness, len * 0.5f, thickness);
    }

    public void End()
    {
        for (int i = used; i < pool.Count; i++)
            pool[i].gameObject.SetActive(false);
    }
}
