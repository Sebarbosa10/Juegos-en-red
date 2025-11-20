using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;

[RequireComponent(typeof(PhotonView))]
public class DisconnectPauseManager : MonoBehaviourPunCallbacks
{
   
    [SerializeField] private Transform lobbyBlueSpawn;
    [SerializeField] private Transform lobbyRedSpawn;

    private const string TeamKey = "team";
    private const string TeamBlue = "Blue";
    private const string TeamRed = "Red";

    private const string ReadyKey = "ready";
    private const string MatchStartedKey = "matchStarted";
    private const string SecondRoundKey = "secondRound";

    public static bool IsPaused => false;

    
    public void ResetCardsAndEffects()
    {
        
        photonView.RPC(nameof(RPC_ResetCardEffects), RpcTarget.All);

        
        if (PhotonNetwork.IsMasterClient)
        {
            var cardManager = FindObjectOfType<CardManagerPhoton>();
            if (cardManager != null)
            {
                cardManager.ResetCards();
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        

        if (!PhotonNetwork.IsMasterClient) return;

        if (!IsMatchStarted())
        {
            
            return;
        }

        
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
        
        ResetCardsAndEffects();

        
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

        
    }

    [PunRPC]
    private void RPC_ResetScores()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScores();
        }
    }

    [PunRPC]
    private void RPC_ResetCardEffects()
    {
        
        var managers = FindObjectsOfType<CardEffectManager>();
        foreach (var mgr in managers)
        {
            mgr.ResetAllEffects();
        }

        
        if (CardEffectUI.Instance != null)
        {
            CardEffectUI.Instance.Clear();
        }

        
    }

    private string GetTeamOf(Player p)
    {
        if (p.CustomProperties == null) return "";
        return p.CustomProperties.TryGetValue(TeamKey, out object t) ? (t as string ?? "") : "";
    }
}
