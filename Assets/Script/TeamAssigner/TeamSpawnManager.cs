using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;


public class TeamSpawnManager : MonoBehaviourPunCallbacks
{
    [Header("Prefab & Tags")]
    [SerializeField] private string playerPrefabName = "Player"; 
    [SerializeField] private string blueSpawnTag = "BlueSpawn";
    [SerializeField] private string redSpawnTag = "RedSpawn";
    [SerializeField] private string lobbySpawnTag = "LobbySpawn";

    private const string TeamKey = "team";
    private const string MatchStartedKey = "matchStarted";
    private const string SpawnedKey = "spawned";
    private const string TeamBlue = "Blue";
    private const string TeamRed = "Red";

    private bool _triedToClaim = false;
    private bool _spawnedLocal = false;
    private bool? _lastMatchStarted = null; 

    void Start()
    {
        
        TrySpawnIfInMatch();
    }

    
    public override void OnRoomPropertiesUpdate(PhotonHashtable changedProps)
    {
        if (changedProps == null || !changedProps.ContainsKey(MatchStartedKey)) return;

        bool started = (bool)changedProps[MatchStartedKey];
        bool previous = _lastMatchStarted ?? started; 
        _lastMatchStarted = started;

        if (!previous && started)
        {
            
            ResetLocalFlagsForMatch();
            TrySpawnIfInMatch();
        }
        else if (previous && !started)
        {
           
            MoveToLobbySpawn();
        }
    }

    public override void OnPlayerPropertiesUpdate(Player target, PhotonHashtable changedProps)
    {
        if (!target.IsLocal || changedProps == null) return;

        
        if (changedProps.ContainsKey(TeamKey) || changedProps.ContainsKey(SpawnedKey))
            TrySpawnIfInMatch();
    }

    
    private void TrySpawnIfInMatch()
    {
        if (!PhotonNetwork.InRoom) return;

        
        bool started = PhotonNetwork.CurrentRoom.CustomProperties != null &&
                       PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(MatchStartedKey) &&
                       (bool)PhotonNetwork.CurrentRoom.CustomProperties[MatchStartedKey];
        _lastMatchStarted ??= started;
        if (!started) return;

        if (PhotonNetwork.LocalPlayer.CustomProperties == null ||
            !PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(TeamKey))
            return;

        bool alreadySpawnedFlag = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(SpawnedKey) &&
                                  PhotonNetwork.LocalPlayer.CustomProperties[SpawnedKey] is bool b && b;

        if (!alreadySpawnedFlag && !_triedToClaim)
        {
            _triedToClaim = true;
            ClaimSpawnToken();       
            return;                  
        }

        if (alreadySpawnedFlag && PhotonNetwork.LocalPlayer.TagObject == null)
            DoSpawnAtTeamPoint();
    }

    private void ClaimSpawnToken()
    {
        object current = null;
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(SpawnedKey))
            current = PhotonNetwork.LocalPlayer.CustomProperties[SpawnedKey];

        var props = new PhotonHashtable { { SpawnedKey, true } };
        var expected = new PhotonHashtable { { SpawnedKey, current } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props, expected);
    }

    private void DoSpawnAtTeamPoint()
    {
        string team = PhotonNetwork.LocalPlayer.CustomProperties[TeamKey] as string;
        string tag = (team == TeamBlue) ? blueSpawnTag : redSpawnTag;

        GetSpawnTransform(tag, out Vector3 pos, out Quaternion rot);

        var existing = FindMyLocalPlayer();
        if (existing != null)
        {
            existing.transform.SetPositionAndRotation(pos, rot);
            PhotonNetwork.LocalPlayer.TagObject = existing;
            CleanupExtraLocalPlayers(existing);
            _spawnedLocal = true;
            Debug.Log($"[Spawn] Reubicado a {team} en {pos}");
            return;
        }

        var go = PhotonNetwork.Instantiate(playerPrefabName, pos, rot);
        PhotonNetwork.LocalPlayer.TagObject = go;
        CleanupExtraLocalPlayers(go);
        _spawnedLocal = true;
        Debug.Log($"[Spawn] Instanciado {team} en {pos}");
    }

    
    private void MoveToLobbySpawn()
    {
        var mine = FindMyLocalPlayer();
        if (mine == null) return;

        GetSpawnTransform(lobbySpawnTag, out Vector3 pos, out Quaternion rot);
        mine.transform.SetPositionAndRotation(pos, rot);
        PhotonNetwork.LocalPlayer.TagObject = mine;

        Debug.Log($"[Spawn] Movido a Lobby en {pos}");
    }

    private void ResetLocalFlagsForMatch()
    {
       
        _triedToClaim = false;
        _spawnedLocal = PhotonNetwork.LocalPlayer.TagObject != null; 
    }

    
    private void GetSpawnTransform(string tag, out Vector3 pos, out Quaternion rot)
    {
        var spawns = GameObject.FindGameObjectsWithTag(tag);
        Transform spawn = null;
        if (spawns != null && spawns.Length > 0)
        {
            int idx = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawns.Length;
            spawn = spawns[idx].transform;
        }
        pos = spawn ? spawn.position : Vector3.zero;
        rot = spawn ? spawn.rotation : Quaternion.identity;
    }

    private GameObject FindMyLocalPlayer()
    {
        if (PhotonNetwork.LocalPlayer.TagObject is GameObject go && go != null)
            return go;

        foreach (var pv in FindObjectsOfType<PhotonView>())
            if (pv && pv.IsMine && pv.gameObject.CompareTag("Player"))
                return pv.gameObject;

        return null;
    }

    private void CleanupExtraLocalPlayers(GameObject keep)
    {
        foreach (var pv in FindObjectsOfType<PhotonView>())
            if (pv && pv.IsMine && pv.gameObject.CompareTag("Player") && pv.gameObject != keep)
                PhotonNetwork.Destroy(pv.gameObject);
    }
}
