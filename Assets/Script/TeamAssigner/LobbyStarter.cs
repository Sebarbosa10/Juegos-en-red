using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using System;


public class LobbyStarter : MonoBehaviourPunCallbacks
{
    private const string TeamKey = "team";
    private const string TeamBlue = "Blue";
    private const string TeamRed = "Red";
    private const string TeamRoomKey = "teamRoom";
    private const string MatchStartedKey = "matchStarted";

    [SerializeField] private byte maxPlayers = 4;

    public override void OnJoinedRoom()
    {
        MaybeStartIfFull();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        MaybeStartIfFull();
    }

    private void MaybeStartIfFull()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (PhotonNetwork.CurrentRoom.PlayerCount < maxPlayers) return;

        StartMatchSplitRooms();
    }

    private void StartMatchSplitRooms()
    {
        
        string matchId = Guid.NewGuid().ToString("N").Substring(0, 8); 

        
        var players = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToArray();
        for (int i = 0; i < players.Length; i++)
        {
            string team = (i < 2) ? TeamBlue : TeamRed;
            string teamRoomName = $"Match{matchId}-{team}";

            var props = new PhotonHashtable {
                { TeamKey, team },
                { TeamRoomKey, teamRoomName }
            };
            players[i].SetCustomProperties(props);
        }

        
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        
        PhotonNetwork.CurrentRoom.SetCustomProperties(new PhotonHashtable { { MatchStartedKey, true } });

        Debug.Log($"[Lobby] Partida iniciada. Rooms: Match{matchId}-Blue / Match{matchId}-Red");
    }
}
