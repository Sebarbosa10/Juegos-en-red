using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class PuzzleGoalTrigger : MonoBehaviourPunCallbacks
{
    [Header("Lobby Spawns")]
    [SerializeField] private Transform lobbyBlueSpawn;
    [SerializeField] private Transform lobbyRedSpawn;

    [Header("Settings")]
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

        // Bloquear inmediatamente para este cliente
        alreadyTriggered = true;

        Debug.Log($"[PuzzleGoalTrigger] {pv.Owner.NickName} ({team}) llegó a la meta");

        // Si SOY el MasterClient, proceso directamente
        if (PhotonNetwork.IsMasterClient)
        {
            ProcessGoalReached(team);
        }
        else
        {
            // Si NO soy MasterClient, le aviso al MasterClient via RPC
            // Pero necesitamos un PhotonView para esto...
            // Usamos propiedades de room como alternativa
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
        // Resetear cuando empieza nueva ronda
        if (propertiesThatChanged.ContainsKey(MatchStartedKey))
        {
            bool matchStarted = (bool)propertiesThatChanged[MatchStartedKey];
            if (matchStarted)
            {
                alreadyTriggered = false;
                Debug.Log("[PuzzleGoalTrigger] Nueva ronda, trigger reseteado");
            }
        }

        // MasterClient procesa cuando alguien llega a la meta
        if (PhotonNetwork.IsMasterClient && propertiesThatChanged.ContainsKey("goalReachedBy"))
        {
            if (alreadyTriggered) return; // Ya procesado

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
        Debug.Log($"[PuzzleGoalTrigger] Procesando victoria de {team}");

        // 1. Agregar punto
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPoint(team);
            Debug.Log($"[PuzzleGoalTrigger] {team} +1 punto");
        }

        // 2. Resetear cartas
        ResetAllCards();

        // 3. Resetear ready flags
        ResetAllReadyFlags();

        // 4. Teletransportar a lobby
        TeleportAllPlayersToLobby();

        // 5. Actualizar propiedades de room (esto notifica a todos)
        var props = new PhotonHashtable
        {
            { MatchStartedKey, false },
            { RoundIndexKey, nextRoundIndex },
            { "goalReachedBy", null } // Limpiar
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        Debug.Log($"[PuzzleGoalTrigger] matchStarted=false, roundIndex={nextRoundIndex}");

        // 6. Resetear efectos locales del MasterClient
        ResetLocalPlayerCardEffects();
    }

    // Este método se llama en todos los clientes cuando matchStarted cambia a false
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