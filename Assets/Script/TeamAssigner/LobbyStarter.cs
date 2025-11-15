using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyStarter : MonoBehaviourPunCallbacks
{
    private const string TeamKey = "team";
    private const string TeamBlue = "Blue";
    private const string TeamRed = "Red";

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
            p.CustomProperties.ContainsKey(TeamKey) &&
            (string)p.CustomProperties[TeamKey] == TeamBlue);

        int redCount = PhotonNetwork.PlayerList.Count(p =>
            p.CustomProperties.ContainsKey(TeamKey) &&
            (string)p.CustomProperties[TeamKey] == TeamRed);

        string team;
        if (blueCount < 2) team = TeamBlue;
        else if (redCount < 2) team = TeamRed;
        else return;

        PhotonHashtable props = new PhotonHashtable { { TeamKey, team } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Vector3 spawnPos = team == TeamBlue ? _spawnPointBlue.position : _spawnPointRed.position;

        GameObject playerObj = PhotonNetwork.Instantiate(_playerPrefabName, spawnPos, Quaternion.identity);

        Renderer coinRenderer = FindChildRendererByName(playerObj, "Coin");
        if (coinRenderer != null)
        {
            coinRenderer.material = (team == TeamBlue) ? blueMat : redMat;
        }

        Debug.Log("[Lobby] Player assigned to: " + team);
    }

    private Renderer FindChildRendererByName(GameObject root, string childName)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == childName)
            {
                return child.GetComponent<Renderer>();
            }
        }
        return null;
    }
}
