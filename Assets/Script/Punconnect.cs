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

    void Start()
    {
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
     

       
        GameObject player = PhotonNetwork.Instantiate(
            playerPrefab.name,
            GetPlayerSpawnPosition().position,
            playerSpawn.rotation,
            0);

        
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

