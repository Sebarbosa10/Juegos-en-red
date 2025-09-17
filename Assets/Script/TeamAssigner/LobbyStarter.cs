using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyStarter : MonoBehaviourPunCallbacks
{
    private const string TeamKey = "team";
    private const string TeamBlue = "Blue";
    private const string TeamRed = "Red";

    [SerializeField] private byte maxPlayers = 4;
    [SerializeField] private string _playerPrefabName = "Player";
    [SerializeField] private Transform _spawnPointBlue;
    [SerializeField] private Transform _spawnPointRed;

    [SerializeField] private Material blueMat;
    [SerializeField] private Material redMat;

    void Start()
    {
        if (!PhotonNetwork.InRoom)
        {
            var opts = new RoomOptions { MaxPlayers = maxPlayers, IsOpen = true, IsVisible = true };
            PhotonNetwork.JoinOrCreateRoom("EgyptLobby", opts, TypedLobby.Default);
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[Lobby] Entré a la sala, spawneando jugador...");

        string team = UnityEngine.Random.Range(0, 2) == 0 ? TeamBlue : TeamRed;

        var props = new PhotonHashtable { { TeamKey, team } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Vector3 spawnPos = team == TeamBlue ? _spawnPointBlue.position : _spawnPointRed.position;

        GameObject playerObj = PhotonNetwork.Instantiate(_playerPrefabName, spawnPos, Quaternion.identity);

        var rend = playerObj.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            rend.material = (team == TeamBlue) ? blueMat : redMat;
        }

        Debug.Log($"[Lobby] Jugador {PhotonNetwork.LocalPlayer.NickName} asignado al equipo {team}.");
    }
}

