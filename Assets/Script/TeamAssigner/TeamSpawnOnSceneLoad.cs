using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class TeamSpawnOnSceneLoad : MonoBehaviourPunCallbacks
{
    [Header("Prefab & Tags")]
    [SerializeField] private string playerPrefabName = "Player";   // en Resources/
    [SerializeField] private string blueSpawnTag = "BlueSpawn";
    [SerializeField] private string redSpawnTag = "RedSpawn";

    private const string TeamKey = "team";
    private const string Blue = "Blue";
    private const string Red = "Red";

    private bool _spawned = false;

    IEnumerator Start()
    {
        // Esperar a estar realmente dentro de la room y con team disponible
        while (!PhotonNetwork.InRoom ||
               PhotonNetwork.LocalPlayer.CustomProperties == null ||
               !PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(TeamKey))
        {
            yield return null;
        }

        DoSpawn();
    }

    private void DoSpawn()
    {
        if (_spawned) return;

        string team = PhotonNetwork.LocalPlayer.CustomProperties[TeamKey] as string;
        string tag = (team == Blue) ? blueSpawnTag : redSpawnTag;

        GetSpawnTransform(tag, out Vector3 pos, out Quaternion rot);

        // Instanciar solo si aún no tengo Player local en escena
        var mine = FindMyLocalPlayer();
        if (mine == null)
        {
            var go = PhotonNetwork.Instantiate(playerPrefabName, pos, rot);
            PhotonNetwork.LocalPlayer.TagObject = go;
        }
        else
        {
            mine.transform.SetPositionAndRotation(pos, rot);
            PhotonNetwork.LocalPlayer.TagObject = mine;
        }

        _spawned = true;
        Debug.Log($"[Egypt] Spawned as {team} at {pos}");
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
        foreach (var pv in FindObjectsOfType<PhotonView>())
            if (pv != null && pv.IsMine && pv.gameObject.CompareTag("Player"))
                return pv.gameObject;
        return null;
    }

    public override void OnPlayerPropertiesUpdate(Player target, PhotonHashtable changedProps)
    {
        // Si el team nos llegó tarde por alguna razón y todavía no spawneamos
        if (!_spawned && target.IsLocal && changedProps != null && changedProps.ContainsKey(TeamKey))
            DoSpawn();
    }
}
