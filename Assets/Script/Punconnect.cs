using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Punconnect : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView playerPrefab;
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private List<Transform> playerSpawnPositions = new List<Transform>();

    private int currentSpawnIndex = 0;


    public override void OnJoinedRoom()
    {
        Debug.Log("[Photon] OnJoinedRoom");
        var prefabName = "Player"; 
        var pos = Vector3.zero;
        var rot = Quaternion.identity;

        
        if (playerSpawn != null) { pos = playerSpawn.position; rot = playerSpawn.rotation; }

        GameObject go = PhotonNetwork.Instantiate(prefabName, pos, rot);
        if (go == null) Debug.LogError("[Photon] No se pudo instanciar el Player (revisá Resources y nombre).");
    }


    private Transform GetPlayerSpawnPosition()
    {
        if (playerSpawnPositions.Count == 0)
            return playerSpawn;

        Transform spawnPoint = playerSpawnPositions[currentSpawnIndex];
        currentSpawnIndex = (currentSpawnIndex + 1) % playerSpawnPositions.Count;
        return spawnPoint;
    }
}

