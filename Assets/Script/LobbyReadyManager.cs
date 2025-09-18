using System;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyReadyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private byte maxPlayers = 4;

    // Opcional: UI para mostrar progreso (ej. "Ready: 3/4")
    [SerializeField] private TMPro.TMP_Text readyCountText;

    private const string ReadyKey = "ready";
    private const string TeamKey = "team";
    private const string TeamRoomKey = "teamRoom";
    private const string MatchStartedKey = "matchStarted";
    private const string TeamBlue = "Blue";
    private const string TeamRed = "Red";

    void Start()
    {
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

        // ¿Están los 4 en ready?
        bool allReady = PhotonNetwork.PlayerList.All(p =>
            p.CustomProperties != null &&
            p.CustomProperties.ContainsKey(ReadyKey) &&
            (bool)p.CustomProperties[ReadyKey]);

        if (!allReady) return;

        // Ya inició?
        bool alreadyStarted = PhotonNetwork.CurrentRoom.CustomProperties != null &&
                              PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(MatchStartedKey) &&
                              (bool)PhotonNetwork.CurrentRoom.CustomProperties[MatchStartedKey];
        if (alreadyStarted) return;

        // 1) Asignar equipos 2/2 de forma determinística (por ActorNumber)
        var players = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToArray();
        string matchId = Guid.NewGuid().ToString("N").Substring(0, 8);

        for (int i = 0; i < players.Length; i++)
        {
            string team = (i < 2) ? TeamBlue : TeamRed; // 2 y 2
            string teamRoom = $"Match{matchId}-{team}";

            var props = new PhotonHashtable
            {
                { TeamKey, team },
                { TeamRoomKey, teamRoom }
            };
            players[i].SetCustomProperties(props);
        }

        // (Opcional) cerrar el lobby
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        // 2) Señal: ¡arrancó! — hará que el TeamRoomSwitcher salga del lobby y entre al team-room
        PhotonNetwork.CurrentRoom.SetCustomProperties(new PhotonHashtable { { MatchStartedKey, true } });

        Debug.Log($"[Lobby] Todos READY. Inicio → Rooms: Match{matchId}-Blue / Match{matchId}-Red");
    }
}
