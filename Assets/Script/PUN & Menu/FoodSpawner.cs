using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;



public class FoodSpawner : MonoBehaviourPunCallbacks
{
    public string pelletPrefabName = "FoodPellet";
    public int targetCount = 200;
    public Vector2 areaXZ = new Vector2(60, 60);
    public float checkInterval = 1f;

    float nextCheck;
    readonly List<GameObject> spawned = new();
    bool IsMaster => PhotonNetwork.IsMasterClient;

    void Start()
    {
        if (IsMaster) EnsurePellets();
    }

    void Update()
    {
        if (!IsMaster) return;
        if (Time.time >= nextCheck)
        {
            nextCheck = Time.time + checkInterval;
            EnsurePellets();
        }
    }

    void EnsurePellets()
    {
        int active = 0;
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] == null) { spawned.RemoveAt(i); continue; }
            if (spawned[i].activeInHierarchy) active++;
        }
        int need = targetCount - active;
        for (int i = 0; i < need; i++)
        {
            Vector3 pos = RandomPos();
            var go = PhotonNetwork.InstantiateRoomObject(pelletPrefabName, pos, Quaternion.identity);
            spawned.Add(go);
        }
    }

    Vector3 RandomPos()
    {
        float x = Random.Range(-areaXZ.x / 2, areaXZ.x / 2);
        float z = Random.Range(-areaXZ.y / 2, areaXZ.y / 2);
        return new Vector3(x, 0, z);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(areaXZ.x, 0.1f, areaXZ.y));
    }
}
