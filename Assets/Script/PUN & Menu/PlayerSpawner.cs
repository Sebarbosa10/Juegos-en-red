using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    
    public string playerPrefabName = "PlayerBlob"; 
    
    public Vector2 spawnArea = new Vector2(40, 40);

    private bool spawned;

    void Start()
    {
        
        if (PhotonNetwork.InRoom && !spawned)
        {
            SpawnPlayer();
        }
    }

    public override void OnJoinedRoom()
    {
        
        if (!spawned)
        {
            SpawnPlayer();
        }
    }

    void SpawnPlayer()
    {
        spawned = true;
        Vector3 pos = RandomPosXZ();
        var go = PhotonNetwork.Instantiate(playerPrefabName, pos, Quaternion.identity);
        Debug.Log($"[Spawn] Player instanciado en {pos}. Prefab='{playerPrefabName}'");
    }

    Vector3 RandomPosXZ()
    {
        float x = Random.Range(-spawnArea.x * 0.5f, spawnArea.x * 0.5f);
        float z = Random.Range(-spawnArea.y * 0.5f, spawnArea.y * 0.5f);
        return new Vector3(x, -30f, z); 
    }
}

