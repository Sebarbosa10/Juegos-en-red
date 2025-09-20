using System;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;


public class LobbyReadyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private byte maxPlayers = 4;
    [Header("UI (opcional)")]
    [SerializeField] private TMPro.TMP_Text readyCountText;

    private const string ReadyKey = "ready";
    private const string TeamKey = "team";
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
        if (PhotonNetwork.CurrentRoom.PlayerCount < 1) return;

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
        for (int i = 0; i < players.Length; i++)
        {
            string team = (i < 2) ? TeamBlue : TeamRed;
            var props = new PhotonHashtable { { TeamKey, team } };
            players[i].SetCustomProperties(props);
        }

        // 2) Esperar propagación y recién ahí iniciar
        StartCoroutine(WaitTeamsPropsAndStart());
    }

    private System.Collections.IEnumerator WaitTeamsPropsAndStart()
    {
        float t = 0f, timeout = 5f;
        while (t < timeout)
        {
            bool allHaveTeam = PhotonNetwork.PlayerList.All(p =>
                p.CustomProperties != null &&
                p.CustomProperties.ContainsKey(TeamKey));
            if (allHaveTeam) break;

            t += Time.deltaTime;
            yield return null;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(new PhotonHashtable { { MatchStartedKey, true } });
        Debug.Log("[Lobby] matchStarted = true (misma escena, a spawnear).");
    }
}
