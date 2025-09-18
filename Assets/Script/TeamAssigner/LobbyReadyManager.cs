using System;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyReadyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private byte maxPlayers = 4;

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

        bool allReady = PhotonNetwork.PlayerList.All(p =>
            p.CustomProperties != null &&
            p.CustomProperties.ContainsKey(ReadyKey) &&
            (bool)p.CustomProperties[ReadyKey]);

        if (!allReady) return;

        bool alreadyStarted = PhotonNetwork.CurrentRoom.CustomProperties != null &&
                              PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(MatchStartedKey) &&
                              (bool)PhotonNetwork.CurrentRoom.CustomProperties[MatchStartedKey];
        if (alreadyStarted) return;


        var players = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToArray();
        string matchId = Guid.NewGuid().ToString("N").Substring(0, 8);

        for (int i = 0; i < players.Length; i++)
        {
            string team = (i < 2) ? TeamBlue : TeamRed;
            string teamRoom = $"Match{matchId}-{team}";

            var props = new PhotonHashtable
            {
                { TeamKey, team },
                { TeamRoomKey, teamRoom }
            };
            players[i].SetCustomProperties(props);
        }

        StartCoroutine(WaitTeamsPropsAndStart());
    }

    private System.Collections.IEnumerator WaitTeamsPropsAndStart()
    {
        float t = 0f;
        const float timeout = 5f;

        while (t < timeout)
        {
            bool allHaveProps = PhotonNetwork.PlayerList.All(p =>
                p.CustomProperties != null &&
                p.CustomProperties.ContainsKey(TeamKey) &&
                p.CustomProperties.ContainsKey(TeamRoomKey));

            if (allHaveProps) break;

            t += Time.deltaTime;
            yield return null;
        }

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        PhotonNetwork.CurrentRoom.SetCustomProperties(new PhotonHashtable { { MatchStartedKey, true } });
        Debug.Log("[Lobby] Props propagadas → matchStarted = true");
    }
}
