using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class PuzzleGoalTrigger : MonoBehaviourPunCallbacks, IOnEventCallback
{
    
    [SerializeField] private Transform lobbyBlueSpawn;
    [SerializeField] private Transform lobbyRedSpawn;

    [SerializeField] private int nextRoundIndex = 3;

    private bool alreadyScored = false;

    private const string TeamKey = "team";
    private const string ReadyKey = "ready";
    private const string MatchStartedKey = "matchStarted";
    private const string RoundIndexKey = "roundIndex";
    private const string CardKey = "cardID";

    private const byte PuzzleGoalEventCode = 50;


    private void OnTriggerEnter(Collider other)
    {
        if (alreadyScored) return;

        var pv = other.GetComponentInParent<PhotonView>();
        if (pv == null) return;
        if (!pv.IsMine) return;

        string team = GetTeamOf(pv.Owner);
        if (string.IsNullOrEmpty(team)) return;

        Debug.Log($"[PuzzleGoalTrigger] Jugador {pv.Owner.NickName} (Equipo {team}) llegó a la meta");

        alreadyScored = true;

        object[] content = new object[] { team, nextRoundIndex };
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(PuzzleGoalEventCode, content, options, SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != PuzzleGoalEventCode) return;

        object[] data = (object[])photonEvent.CustomData;
        string team = (string)data[0];
        int roundIndex = (int)data[1];

        Debug.Log($"[PuzzleGoalTrigger] Evento recibido: Equipo {team} completó puzzle");

        alreadyScored = true;

        ResetLocalPlayerCardEffects();

        if (PhotonNetwork.IsMasterClient)
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddPoint(team);
                Debug.Log($"[PuzzleGoalTrigger] Equipo {team} +1 punto");
            }

            TeleportAllPlayersToLobby();
            ResetAllReadyFlags();
            ResetAllCards();

            var props = new PhotonHashtable
            {
                { MatchStartedKey, false },
                { RoundIndexKey, roundIndex }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);

            Debug.Log($"[PuzzleGoalTrigger] matchStarted=false, roundIndex={roundIndex}");
        }
    }

    private void ResetLocalPlayerCardEffects()
    {
        if (PhotonNetwork.LocalPlayer.TagObject is GameObject playerObj)
        {
            var effectManager = playerObj.GetComponent<CardEffectManager>();
            if (effectManager != null)
            {
                effectManager.ResetAllEffects();
                Debug.Log("[PuzzleGoalTrigger] Efectos de carta reseteados");
            }
        }

        if (CardEffectUI.Instance != null)
        {
            CardEffectUI.Instance.Clear();
        }
    }

    private void ResetAllCards()
    {
        Debug.Log("[PuzzleGoalTrigger] Reseteando cartas de todos...");

        foreach (var p in PhotonNetwork.PlayerList)
        {
            var props = new PhotonHashtable
            {
                { CardKey, -1 },
                { "cardFrom", "" }
            };
            p.SetCustomProperties(props);
        }
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

        Debug.Log("[PuzzleGoalTrigger] Todos teletransportados a lobby");
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

    public override void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(MatchStartedKey))
        {
            bool matchStarted = (bool)propertiesThatChanged[MatchStartedKey];
            if (matchStarted)
            {
                alreadyScored = false;
                Debug.Log("[PuzzleGoalTrigger] Nueva ronda, alreadyScored reseteado");
            }
        }
    }
}