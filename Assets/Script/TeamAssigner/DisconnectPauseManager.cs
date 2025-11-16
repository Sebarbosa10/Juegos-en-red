using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;

[RequireComponent(typeof(PhotonView))]
public class DisconnectPauseManager : MonoBehaviourPunCallbacks
{
    [Header("Lobby Spawns")]
    [SerializeField] private Transform lobbyBlueSpawn;
    [SerializeField] private Transform lobbyRedSpawn;

    private const string TeamKey = "team";
    private const string TeamBlue = "Blue";
    private const string TeamRed = "Red";

    private const string ReadyKey = "ready";
    private const string MatchStartedKey = "matchStarted";
    private const string SecondRoundKey = "secondRound";

    
    public static bool IsPaused => false;

    

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"[DisconnectReset] Player left: {otherPlayer.NickName}");

       
        if (!PhotonNetwork.IsMasterClient) return;

        if (!IsMatchStarted())
        {
            Debug.Log("[DisconnectReset] La partida no estaba empezada, no hago nada especial.");
            return;
        }

        Debug.Log("[DisconnectReset] Partida en curso y alguien se desconectó → reseteando y volviendo a lobby.");
        ResetMatchAndReturnToLobby();

        var clearProps = new PhotonHashtable
        {
            { MatchStartedKey, false },
            { SecondRoundKey, false }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(clearProps);
    }


    private bool IsMatchStarted()
    {
        var props = PhotonNetwork.CurrentRoom?.CustomProperties;
        if (props == null) return false;

        return props.ContainsKey(MatchStartedKey) && (bool)props[MatchStartedKey];
    }

    private void ResetMatchAndReturnToLobby()
    {
        
        if (ScoreManager.Instance != null)
        {
            photonView.RPC(nameof(RPC_ResetScores), RpcTarget.All);
        }

       
        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (!(p.TagObject is GameObject go)) continue;

            string team = GetTeamOf(p);
            Transform lobbySpawn = null;

            if (team == TeamBlue) lobbySpawn = lobbyBlueSpawn;
            else if (team == TeamRed) lobbySpawn = lobbyRedSpawn;

            if (lobbySpawn == null) continue;

            go.transform.position = lobbySpawn.position;
            go.transform.rotation = lobbySpawn.rotation;

            var props = new PhotonHashtable { { ReadyKey, false } };
            p.SetCustomProperties(props);
        }

        Debug.Log("[DisconnectReset] Todos teletransportados a lobby, ready=false, scores reseteados.");
    }

    [PunRPC]
    private void RPC_ResetScores()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScores();
        }
    }

    private string GetTeamOf(Player p)
    {
        if (p.CustomProperties == null) return "";
        return p.CustomProperties.TryGetValue(TeamKey, out object t) ? (t as string ?? "") : "";
    }
}
