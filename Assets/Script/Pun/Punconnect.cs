using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class Punconnect : MonoBehaviourPunCallbacks
{
    [Header("Photon")]
    [SerializeField] private string gameVersion = "dev-0.1";
    [Tooltip("Vacío = BestRegion. Para test local pongan TODOS la misma, ej: \"sa\", \"usw\", \"eu\".")]
    [SerializeField] private string fixedRegion = "sa";
    [Tooltip("Si está en true, todos entran a la MISMA sala de test.")]
    [SerializeField] private bool useFixedRoomName = true;
    [SerializeField] private string roomName = "ROOM_DEV";
    [SerializeField] private byte maxPlayers = 8;
    [SerializeField] private bool respawnOnSceneLoaded = true;

    [Header("Spawn")]
    [SerializeField] private string playerPrefabName = "Player";
    [SerializeField] private Transform fallbackSpawn;
    [SerializeField] private List<Transform> spawnPoints = new();

    private void OnEnable()
    {
        if (respawnOnSceneLoaded) SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        if (respawnOnSceneLoaded) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;

        if (!string.IsNullOrEmpty(fixedRegion))
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = fixedRegion;

        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("[Photon] Conectando...");
            PhotonNetwork.ConnectUsingSettings();
            return;
        }

        TryJoinRoom();
        TrySpawnIfInRoom("[Start]");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Hola connected to master");
        TryJoinRoom();
    }

    private void TryJoinRoom()
    {
        var opts = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            IsOpen = true,
            IsVisible = true,
            CleanupCacheOnLeave = true,
            PublishUserId = true
        };

        if (useFixedRoomName)
        {
            Debug.Log($"[Photon] JoinOrCreateRoom('{roomName}')...");
            PhotonNetwork.JoinOrCreateRoom(roomName, opts, TypedLobby.Default);
        }
        else
        {
            Debug.Log("[Photon] Intentando unirse a una sala aleatoria...");
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[Photon] JoinRandom falló ({returnCode}): {message}. Creando sala...");
        string rnd = $"ROOM_{Random.Range(1000, 9999)}";
        var opts = new RoomOptions { MaxPlayers = maxPlayers, IsOpen = true, IsVisible = true, CleanupCacheOnLeave = true, PublishUserId = true };
        PhotonNetwork.CreateRoom(rnd, opts, TypedLobby.Default);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[Photon] CreateRoom falló ({returnCode}): {message}. Reintentando con otro nombre...");
        string rnd = $"ROOM_{Random.Range(1000, 9999)}";
        var opts = new RoomOptions { MaxPlayers = maxPlayers, IsOpen = true, IsVisible = true, CleanupCacheOnLeave = true, PublishUserId = true };
        PhotonNetwork.CreateRoom(rnd, opts, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[Photon] ¡Entré a '{PhotonNetwork.CurrentRoom.Name}' ({PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers})!");
        TrySpawnIfInRoom("[OnJoinedRoom]");
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LocalPlayer.TagObject = null;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[Photon] JoinRoomFailed ({returnCode}): {message}");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"[Photon] Desconectado: {cause}");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (respawnOnSceneLoaded) TrySpawnIfInRoom($"[OnSceneLoaded:{scene.name}]");
    }

    // ---------- SPAWN ----------
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
            Debug.Log($"{from} [Spawn] Ya existe TagObject local ({existing.name}).");
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
            Debug.LogError($"{from} [Spawn] Instantiate devolvió null. Revisá Resources/{playerPrefabName}.prefab y su PhotonView.");
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
