using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;



public class Punconnect : MonoBehaviourPunCallbacks
{
    [Header("Spawn")]
    [SerializeField] private string playerPrefabName = "Player"; 
    [SerializeField] private Transform fallbackSpawn;            
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    



    void Start()
    {
    
        TrySpawnIfInRoom();
    }

    
    public override void OnJoinedRoom()
    {
       
        TrySpawnIfInRoom();
    }

    private void TrySpawnIfInRoom()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[Spawn] No estoy en una sala aún, no spawneo.");
            return;
        }

        if (PhotonNetwork.LocalPlayer.TagObject != null)
        {
            Debug.Log("[Spawn] Ya existe TagObject local (ya spawneado). No hago nada.");
            return;
        }

        SpawnLocalPlayer();
    }

    private void SpawnLocalPlayer()
    {
        if (string.IsNullOrEmpty(playerPrefabName))
        {
            Debug.LogError("[Spawn] Nombre de prefab vacío.");
            return;
        }

        // Elegir punto de spawn estable por jugador (evita superposición)
        Transform p = GetPlayerSpawnPosition();
        Vector3 pos = p ? p.position : Vector3.zero;
        Quaternion rot = p ? p.rotation : Quaternion.identity;

        Debug.Log($"[Spawn] Instanciando '{playerPrefabName}' en {pos} rot {rot.eulerAngles} (ActorNumber={PhotonNetwork.LocalPlayer.ActorNumber})");

        GameObject go = PhotonNetwork.Instantiate(playerPrefabName, pos, rot);
        if (go == null)
        {
            Debug.LogError("[Spawn] PhotonNetwork.Instantiate devolvió null. Revisá que Resources/" + playerPrefabName + ".prefab exista y tenga PhotonView.");
            return;
        }

        
        PhotonNetwork.LocalPlayer.TagObject = go;

        Debug.Log("[Spawn] Player local instanciado correctamente.");
    }

    private Transform GetPlayerSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            int idx = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Count;
            var t = spawnPoints[idx];
            Debug.Log($"[Spawn] Usando spawnPoints[{idx}] -> {t.name}");
            return t;
        }
        Debug.Log("[Spawn] Usando fallbackSpawn (o Vector3.zero si es null).");
        return fallbackSpawn;
    }

  
}
