using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;


public class Punconnect : MonoBehaviourPunCallbacks
{
    [Header("Spawn")]
    [SerializeField] private string playerPrefabName = "Player";
    [SerializeField] private Transform fallbackSpawn;
    [SerializeField] private List<Transform> spawnPoints = new();

    [Header("Escenas")]
    [Tooltip("Si usás PhotonNetwork.AutomaticallySyncScene = true, habilitá esto para respawnear al cargar la escena sincronizada.")]
    [SerializeField] private bool respawnOnSceneLoaded = true;

    private void OnEnable()
    {
        if (respawnOnSceneLoaded)
            SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (respawnOnSceneLoaded)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        TrySpawnIfInRoom("[Start]");
    }

    public override void OnJoinedRoom()
    {
        TrySpawnIfInRoom("[OnJoinedRoom]");
    }

    public override void OnLeftRoom()
    {
   
        PhotonNetwork.LocalPlayer.TagObject = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!respawnOnSceneLoaded) return;
        TrySpawnIfInRoom($"[OnSceneLoaded:{scene.name}]");
    }

    private void TrySpawnIfInRoom(string from)
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning($"{from} [Spawn] No estoy en una sala aún, no spawneo.");
            return;
        }

        var existing = PhotonNetwork.LocalPlayer.TagObject as GameObject;
        if (existing != null) 
        {
            Debug.Log($"{from} [Spawn] Ya existe TagObject local (ya spawneado): {existing.name}. No hago nada.");
            return;
        }

        SpawnLocalPlayer(from);
    }

    private void SpawnLocalPlayer(string from)
    {
        if (string.IsNullOrEmpty(playerPrefabName))
        {
            Debug.LogError($"{from} [Spawn] Nombre de prefab vacío.");
            return;
        }

        Transform p = GetPlayerSpawnPosition();
        Vector3 pos = p ? p.position : Vector3.zero;
        Quaternion rot = p ? p.rotation : Quaternion.identity;

        Debug.Log($"{from} [Spawn] Instanciando '{playerPrefabName}' en {pos} rot {rot.eulerAngles} (ActorNumber={PhotonNetwork.LocalPlayer.ActorNumber})");

        GameObject go = PhotonNetwork.Instantiate(playerPrefabName, pos, rot);
        if (go == null)
        {
            Debug.LogError($"{from} [Spawn] PhotonNetwork.Instantiate devolvió null. Revisá Resources/{playerPrefabName}.prefab y su PhotonView.");
            return;
        }

        PhotonNetwork.LocalPlayer.TagObject = go;

        Debug.Log($"{from} [Spawn] Player local instanciado correctamente: {go.name}");
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

        if (fallbackSpawn != null)
        {
            Debug.Log("[Spawn] Usando fallbackSpawn.");
            return fallbackSpawn;
        }

        Debug.Log("[Spawn] Sin puntos definidos: usando Vector3.zero.");
        return null;
    }
}
