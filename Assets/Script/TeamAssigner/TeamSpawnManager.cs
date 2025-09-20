using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class TeamSpawnManager : MonoBehaviourPunCallbacks
{
    [Header("Prefab & Tags")]
    [SerializeField] private string playerPrefabName = "Player"; // Resources/Player.prefab
    [SerializeField] private string blueSpawnTag = "BlueSpawn";
    [SerializeField] private string redSpawnTag = "RedSpawn";

    private const string TeamKey = "team";
    private const string MatchStartedKey = "matchStarted";
    private const string SpawnedKey = "spawned";
    private const string TeamBlue = "Blue";
    private const string TeamRed = "Red";

    private bool _triedToClaim = false; // evita spamear claims locales
    private bool _spawnedLocal = false; // guard local

    void Start()
    {
        TryProceed(); // por si ya estaba todo listo al cargar la escena
    }

    public override void OnRoomPropertiesUpdate(PhotonHashtable changedProps)
    {
        TryProceed();
    }

    public override void OnPlayerPropertiesUpdate(Player target, PhotonHashtable changedProps)
    {
        // Nos importa especialmente CUANDO:
        // - el local recibe su Team
        // - el local recibe SpawnedKey=true (tras claim atómico)
        if (target.IsLocal)
            TryProceed();
    }

    private void TryProceed()
    {
        if (!PhotonNetwork.InRoom) return;
        if (_spawnedLocal) return;

        // 1) ¿arrancó el match?
        bool started = PhotonNetwork.CurrentRoom.CustomProperties != null &&
                       PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(MatchStartedKey) &&
                       (bool)PhotonNetwork.CurrentRoom.CustomProperties[MatchStartedKey];
        if (!started) return;

        // 2) ¿tengo mi team?
        if (PhotonNetwork.LocalPlayer.CustomProperties == null ||
            !PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(TeamKey))
            return;

        // 3) ¿ya tomé el token "spawned"? Si no, intentarlo una sola vez
        bool alreadySpawnedFlag = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(SpawnedKey) &&
                                  PhotonNetwork.LocalPlayer.CustomProperties[SpawnedKey] is bool b && b;
        if (!alreadySpawnedFlag && !_triedToClaim)
        {
            _triedToClaim = true;
            ClaimSpawnToken(); // intento atómico
            return; // esperamos el OnPlayerPropertiesUpdate con SpawnedKey=true
        }

        // 4) Si ya tengo spawned=true, hacer el spawn SOLO si aún no tengo TagObject
        if (alreadySpawnedFlag && PhotonNetwork.LocalPlayer.TagObject == null)
        {
            DoSpawnAtTeamPoint();
        }
    }

    private void ClaimSpawnToken()
    {
        // Claim atómico: set SpawnedKey=true SOLO si el valor actual coincide con expected
        // Si no existe, esperamos null. Si existe y es false, esperamos false.
        object current = null;
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(SpawnedKey))
            current = PhotonNetwork.LocalPlayer.CustomProperties[SpawnedKey];

        var props = new PhotonHashtable { { SpawnedKey, true } };
        var expected = new PhotonHashtable { { SpawnedKey, current } };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props, expected);
        // Nota: si falla por condición de carrera, otro claim ganó → nos llegará el cambio igualmente.
    }

    private void DoSpawnAtTeamPoint()
    {
        string team = PhotonNetwork.LocalPlayer.CustomProperties[TeamKey] as string;
        string tag = (team == TeamBlue) ? blueSpawnTag : redSpawnTag;

        var spawns = GameObject.FindGameObjectsWithTag(tag);
        Transform spawn = null;
        if (spawns != null && spawns.Length > 0)
        {
            int idx = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawns.Length;
            spawn = spawns[idx].transform;
        }

        Vector3 pos = spawn ? spawn.position : Vector3.zero;
        Quaternion rot = spawn ? spawn.rotation : Quaternion.identity;

        // Si ya tenía una instancia mía (ej: la del lobby), moverla en vez de crear otra
        var existing = FindMyLocalPlayer();
        if (existing != null)
        {
            existing.transform.SetPositionAndRotation(pos, rot);
            PhotonNetwork.LocalPlayer.TagObject = existing;
            CleanupExtraLocalPlayers(existing);
            _spawnedLocal = true;
            Debug.Log($"[Spawn] Reubicado player existente a {pos}");
            return;
        }

        // Crear de red
        var go = PhotonNetwork.Instantiate(playerPrefabName, pos, rot);
        PhotonNetwork.LocalPlayer.TagObject = go;

        // Limpieza por si acaso
        CleanupExtraLocalPlayers(go);

        _spawnedLocal = true;
        Debug.Log($"[Spawn] {PhotonNetwork.NickName} ({team}) instanciado en {pos}");
    }

    private GameObject FindMyLocalPlayer()
    {
        // 1) Si ya lo guardamos en TagObject
        if (PhotonNetwork.LocalPlayer.TagObject is GameObject go && go != null)
            return go;

        // 2) Buscar un objeto mío con tag "Player"
        var allPV = FindObjectsOfType<PhotonView>();
        foreach (var pv in allPV)
        {
            if (pv && pv.IsMine && pv.gameObject.CompareTag("Player"))
                return pv.gameObject;
        }
        return null;
    }

    private void CleanupExtraLocalPlayers(GameObject keep)
    {
        var allPV = FindObjectsOfType<PhotonView>();
        foreach (var pv in allPV)
        {
            if (pv && pv.IsMine && pv.gameObject.CompareTag("Player") && pv.gameObject != keep)
            {
                Debug.LogWarning("[Spawn] Había un clon local extra. Lo destruyo.");
                PhotonNetwork.Destroy(pv.gameObject);
            }
        }
    }
}
