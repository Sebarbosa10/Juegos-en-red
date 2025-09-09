using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class Punconnect : MonoBehaviourPunCallbacks
{
    [Header("Spawn")]
    [SerializeField] private string playerPrefabName = "Player"; 
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private Transform fallbackSpawn;

    private const string TEAM_KEY = "team";

    public override void OnJoinedRoom()
    {
        Debug.Log("[Photon] OnJoinedRoom");
        StartCoroutine(WaitTeamAndSpawn());
    }

    private IEnumerator WaitTeamAndSpawn()
    {
        // Esperar hasta que el Master nos asigne el equipo
        while (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(TEAM_KEY))
            yield return null;

        int teamInt = (int)PhotonNetwork.LocalPlayer.CustomProperties[TEAM_KEY];
        Team team = (Team)teamInt;

        Transform p = GetSpawnFor(team);
        Vector3 pos = p ? p.position : Vector3.zero;
        Quaternion rot = p ? p.rotation : Quaternion.identity;

        // Pasamos el team en instantiationData[0]
        object[] data = new object[] { teamInt };

        GameObject go = PhotonNetwork.Instantiate(playerPrefabName, pos, rot, 0, data);
        if (go == null)
        {
            Debug.LogError("[Spawn] No se pudo instanciar Player (revisá Resources/nombre).");
            yield break;
        }

        Debug.Log($"[Spawn] Player local instanciado en {team}.");
    }

    private Transform GetSpawnFor(Team team)
    {
        // Opción simple: usar índices distintos según equipo
        // p.ej. primeros N puntos para Yellow, segundos N para Purple
        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            // reparto estable por ActorNumber para que no se apilen
            int idx = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Count;
            return spawnPoints[idx];
        }
        return fallbackSpawn;
    }
}

