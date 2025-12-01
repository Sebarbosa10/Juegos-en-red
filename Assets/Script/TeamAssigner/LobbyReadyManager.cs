using System.Linq;
using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyReadyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private byte maxPlayers = 4;

    [Header("UI (opcional)")]
    [SerializeField] private TMPro.TMP_Text readyCountText;

    [Header("Card Manager")]
    [SerializeField] private CardManagerPhoton cardManager;

    private const string ReadyKey = "ready";
    private const string TeamKey = "team";
    private const string MatchStartedKey = "matchStarted";
    private const string RoundIndexKey = "roundIndex";
    private const string TeamBlue = "Blue";
    private const string TeamRed = "Red";

    void Start()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
        {
            var roomProps = PhotonNetwork.CurrentRoom.CustomProperties;
            if (roomProps == null || !roomProps.ContainsKey(RoundIndexKey))
            {
                var props = new PhotonHashtable
                {
                    { RoundIndexKey, 1 },
                    { MatchStartedKey, false }
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
                Debug.Log("[LobbyReadyManager] roundIndex inicializado a 1.");
            }
        }

        if (cardManager == null)
        {
            cardManager = FindObjectOfType<CardManagerPhoton>();
        }

        UpdateReadyUI();
        TryStartIfAllReady();
    }

    public override void OnJoinedRoom()
    {
        UpdateReadyUI();
        TryStartIfAllReady();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateReadyUI();
        TryStartIfAllReady();
    }

    public override void OnPlayerPropertiesUpdate(Player target, PhotonHashtable changedProps)
    {
        if (changedProps != null && changedProps.ContainsKey(ReadyKey))
        {
            UpdateReadyUI();
            TryStartIfAllReady();
        }
    }

    private void UpdateReadyUI()
    {
        if (readyCountText == null || !PhotonNetwork.InRoom) return;

        int readyCount = PhotonNetwork.PlayerList.Count(p =>
            p.CustomProperties != null &&
            p.CustomProperties.ContainsKey(ReadyKey) &&
            (bool)p.CustomProperties[ReadyKey]);

        readyCountText.text = $"Ready: {readyCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
    }

    private void TryStartIfAllReady()
    {
        if (!PhotonNetwork.InRoom) return;
        if (!PhotonNetwork.IsMasterClient) return;
        if (PhotonNetwork.CurrentRoom.PlayerCount < maxPlayers) return;

        bool allReady = PhotonNetwork.PlayerList.All(p =>
            p.CustomProperties != null &&
            p.CustomProperties.ContainsKey(ReadyKey) &&
            (bool)p.CustomProperties[ReadyKey]);

        if (!allReady) return;

        bool alreadyStarted =
            PhotonNetwork.CurrentRoom.CustomProperties != null &&
            PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(MatchStartedKey) &&
            (bool)PhotonNetwork.CurrentRoom.CustomProperties[MatchStartedKey];

        if (alreadyStarted) return;

        Debug.Log("[LobbyReadyManager] ¡Todos listos! Iniciando partida...");

        
        var players = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToArray();
        for (int i = 0; i < players.Length; i++)
        {
            
            if (!players[i].CustomProperties.ContainsKey(TeamKey) ||
                string.IsNullOrEmpty(players[i].CustomProperties[TeamKey] as string))
            {
                string team = (i < 2) ? TeamBlue : TeamRed;
                var props = new PhotonHashtable { { TeamKey, team } };
                players[i].SetCustomProperties(props);
                Debug.Log($"[LobbyReadyManager] {players[i].NickName} asignado a {team}");
            }
        }

        
        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new PhotonHashtable { { MatchStartedKey, true } }
        );

        Debug.Log("[LobbyReadyManager] matchStarted = true");

       
        StartCoroutine(DealCardsAfterDelay());
    }

    private IEnumerator DealCardsAfterDelay()
    {
        
        yield return new WaitForSeconds(0.5f);

        
        if (TeamManager.Instance != null)
        {
            TeamManager.Instance.RefreshTeams();
            Debug.Log("[LobbyReadyManager] TeamManager.RefreshTeams() llamado");
        }
        else
        {
            Debug.LogError("[LobbyReadyManager] TeamManager.Instance es NULL!");
            yield break;
        }

        
        yield return new WaitForSeconds(0.3f);

        
        if (cardManager != null)
        {
            Debug.Log("[LobbyReadyManager] Repartiendo cartas...");
            cardManager.DealCards();
        }
        else
        {
            Debug.LogError("[LobbyReadyManager] cardManager es NULL!");
        }
    }
}
