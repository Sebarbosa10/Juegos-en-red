using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;



public class TeamMapSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private string playerPrefabName = "Player"; 
    [SerializeField] private string spawnTag = "Spawn";

    void Start()
    {
        if (!PhotonNetwork.InRoom) return;
        if (PhotonNetwork.LocalPlayer.TagObject != null) return;

        var spawns = GameObject.FindGameObjectsWithTag(spawnTag);
        Transform spawn = null;
        if (spawns != null && spawns.Length > 0)
        {
            int idx = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawns.Length;
            spawn = spawns[idx].transform;
        }

        Vector3 pos = spawn ? spawn.position : Vector3.zero;
        Quaternion rot = spawn ? spawn.rotation : Quaternion.identity;

        var go = PhotonNetwork.Instantiate(playerPrefabName, pos, rot);
        PhotonNetwork.LocalPlayer.TagObject = go;

        Debug.Log($"[Spawner] Player instanciado en {pos}");
    }
}
