using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class PuzzleGoalTrigger : MonoBehaviourPunCallbacks
{
    
    [SerializeField] private Transform lobbyBlueSpawn;
    [SerializeField] private Transform lobbyRedSpawn;

    
    [SerializeField] private int nextRoundIndex = 3;

    private bool alreadyTriggered = false;

    private const string TeamKey = "team";
    private const string ReadyKey = "ready";
    private const string MatchStartedKey = "matchStarted";
    private const string RoundIndexKey = "roundIndex";
    private const string CardKey = "cardID";

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyTriggered) return;

        var pv = other.GetComponentInParent<PhotonView>();
        if (pv == null) return;
        if (!pv.IsMine) return;

        string team = GetTeamOf(pv.Owner);
        if (string.IsNullOrEmpty(team)) return;

       
        alreadyTriggered = true;

        Debug.Log($"[PuzzleGoalTrigger] {pv.Owner.NickName} ({team}) llegó a la meta");

        
        if (PhotonNetwork.IsMasterClient)
        {
            ProcessGoalReached(team);
        }
        else
        {
            
            var props = new PhotonHashtable
            {
                { "goalReachedBy", team },
                { "goalTimestamp", PhotonNetwork.Time }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
    }

    public override void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
    {
        
        if (propertiesThatChanged.ContainsKey(MatchStartedKey))
        {
            bool matchStarted = (bool)propertiesThatChanged[MatchStartedKey];
            if (matchStarted)
            {
                alreadyTriggered = false;
                Debug.Log("[PuzzleGoalTrigger] Nueva ronda, trigger reseteado");
            }
        }

        
        if (PhotonNetwork.IsMasterClient && propertiesThatChanged.ContainsKey("goalReachedBy"))
        {
            if (alreadyTriggered) return; 

            string team = propertiesThatChanged["goalReachedBy"] as string;
            if (!string.IsNullOrEmpty(team))
            {
                alreadyTriggered = true;
                ProcessGoalReached(team);
            }
        }
    }
    private void ProcessGoalReached(string team)
    {
        Debug.LogWarning($"[PuzzleGoalTrigger] ========== GOAL REACHED ==========");

        
        if (ScoreManager.Instance != null)
        {
            int blueAntes = ScoreManager.Instance.GetBlueScore();
            int redAntes = ScoreManager.Instance.GetRedScore();
            Debug.LogWarning($"[PuzzleGoalTrigger] Marcador ANTES: Blue={blueAntes}, Red={redAntes}");

            ScoreManager.Instance.AddPoint(team);
            Debug.LogWarning($"[PuzzleGoalTrigger] Punto agregado a {team}");
        }

        ResetAllCards();
        ResetAllReadyFlags();
        TeleportAllPlayersToLobby();

        var props = new PhotonHashtable
    {
        { MatchStartedKey, false },
        { RoundIndexKey, nextRoundIndex },
        { "goalReachedBy", null }
    };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        Debug.LogWarning($"[PuzzleGoalTrigger] matchStarted=false, roundIndex={nextRoundIndex}");

        ResetLocalPlayerCardEffects();
    }

    private void OnMatchEnded()
    {
        ResetLocalPlayerCardEffects();
    }

    private void ResetLocalPlayerCardEffects()
    {
        if (PhotonNetwork.LocalPlayer.TagObject is GameObject playerObj)
        {
            var effectManager = playerObj.GetComponent<CardEffectManager>();
            if (effectManager != null)
            {
                effectManager.ResetAllEffects();
            }
        }

        if (CardEffectUI.Instance != null)
        {
            CardEffectUI.Instance.Clear();
        }
    }

    private void ResetAllCards()
    {
        foreach (var p in PhotonNetwork.PlayerList)
        {
            var props = new PhotonHashtable
            {
                { CardKey, -1 },
                { "cardFrom", "" }
            };
            p.SetCustomProperties(props);
        }
        Debug.Log("[PuzzleGoalTrigger] Cartas reseteadas");
    }

    private void TeleportAllPlayersToLobby()
    {
        foreach (var p in PhotonNetwork.PlayerList)
        {
            string team = GetTeamOf(p);
            if (string.IsNullOrEmpty(team)) continue;

            Transform targetSpawn = (team == "Blue") ? lobbyBlueSpawn : lobbyRedSpawn;
            if (targetSpawn == null) continue;

            if (p.TagObject is GameObject go)
            {
                go.transform.position = targetSpawn.position;
                go.transform.rotation = targetSpawn.rotation;
            }
        }
        Debug.Log("[PuzzleGoalTrigger] Jugadores teletransportados");
    }

    private void ResetAllReadyFlags()
    {
        foreach (var p in PhotonNetwork.PlayerList)
        {
            var props = new PhotonHashtable { { ReadyKey, false } };
            p.SetCustomProperties(props);
        }
        Debug.Log("[PuzzleGoalTrigger] Ready flags reseteados");
    }

    private string GetTeamOf(Player p)
    {
        if (p?.CustomProperties == null) return "";
        return p.CustomProperties.TryGetValue(TeamKey, out object value)
            ? (value as string ?? "")
            : "";
    }
}