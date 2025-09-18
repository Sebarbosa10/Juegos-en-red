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
        Debug.Log("[Lobby] Entré a la sala, spawneando jugador...");

        int blueCount = PhotonNetwork.PlayerList.Count(p =>
            p.CustomProperties.ContainsKey("team") && (string)p.CustomProperties["team"] == "Blue");

        int redCount = PhotonNetwork.PlayerList.Count(p =>
            p.CustomProperties.ContainsKey("team") && (string)p.CustomProperties["team"] == "Red");

        string team;
        if (blueCount < 2) team = "Blue";
        else if (redCount < 2) team = "Red";
        else
        {
            Debug.LogWarning("[Lobby] No hay lugar en ningún equipo!");
            return;
        }

        var props = new ExitGames.Client.Photon.Hashtable { { "team", team } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Vector3 spawnPos = team == "Blue" ? _spawnPointBlue.position : _spawnPointRed.position;

        GameObject playerObj = PhotonNetwork.Instantiate(_playerPrefabName, spawnPos, Quaternion.identity);

        var rend = playerObj.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            rend.material = (team == "Blue") ? blueMat : redMat;
        }

        Debug.Log($"[Lobby] Jugador {PhotonNetwork.LocalPlayer.NickName} asignado al equipo {team}.");
    }

}
