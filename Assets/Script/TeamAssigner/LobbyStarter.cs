using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;

public class LobbyStarter : MonoBehaviourPunCallbacks
{
    private const string TeamKey = "team";
    private const string TeamBlue = "Blue";
    private const string TeamRed = "Red";
    private const string MatchStartedKey = "matchStarted";

    [SerializeField] private string _playerPrefabName = "Player";
    [SerializeField] private Transform _spawnPointBlue;
    [SerializeField] private Transform _spawnPointRed;

    [SerializeField] private Material blueMat;
    [SerializeField] private Material redMat;

    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            OnJoinedRoom();
        }
    }

    public override void OnJoinedRoom()
    {
        int blueCount = PhotonNetwork.PlayerList.Count(p =>
            p.CustomProperties != null &&
            p.CustomProperties.ContainsKey(TeamKey) &&
            (string)p.CustomProperties[TeamKey] == TeamBlue);

        int redCount = PhotonNetwork.PlayerList.Count(p =>
            p.CustomProperties != null &&
            p.CustomProperties.ContainsKey(TeamKey) &&
            (string)p.CustomProperties[TeamKey] == TeamRed);

        string team;
        if (blueCount < 2) team = TeamBlue;
        else if (redCount < 2) team = TeamRed;
        else return;

        var props = new PhotonHashtable { { TeamKey, team } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Vector3 spawnPos = team == TeamBlue ? _spawnPointBlue.position : _spawnPointRed.position;
        Quaternion spawnRot = team == TeamBlue ? _spawnPointBlue.rotation : _spawnPointRed.rotation;

        GameObject playerObj = PhotonNetwork.Instantiate(_playerPrefabName, spawnPos, spawnRot);

        PhotonNetwork.LocalPlayer.TagObject = playerObj;

      
        Transform coinTransform = playerObj.transform.Find("VISUALS/Coin");
        if (coinTransform != null)
        {
            Renderer coinRenderer = coinTransform.GetComponent<Renderer>();
            if (coinRenderer != null)
            {
                coinRenderer.material = (team == TeamBlue) ? blueMat : redMat;
            }
        }
        else
        {
            Debug.LogWarning("Coin object not found under Player → Empty/Coin");
        }

        Debug.Log($"[Lobby] Player {PhotonNetwork.LocalPlayer.NickName} assigned to team {team}.");

        var roomProps = PhotonNetwork.CurrentRoom?.CustomProperties;
        if (roomProps != null &&
            roomProps.ContainsKey(MatchStartedKey) &&
            (bool)roomProps[MatchStartedKey])
        {
            Debug.Log("[Lobby] matchStarted already true → sending player directly to puzzle zone.");

            TeamMapSpawner spawner = FindObjectOfType<TeamMapSpawner>();
            if (spawner != null)
            {
                spawner.RespawnLocalPlayerFromPause();
            }
            else
            {
                Debug.LogWarning("[Lobby] TeamMapSpawner not found in scene.");
            }
        }
    }
}
