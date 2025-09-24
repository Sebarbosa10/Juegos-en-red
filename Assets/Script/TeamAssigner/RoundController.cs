using UnityEngine;
using Photon.Pun;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class RoundController : MonoBehaviourPunCallbacks
{
    private const string MatchStartedKey = "matchStarted";

    [SerializeField] private KeyCode endRoundKey = KeyCode.F;

    void Update()
    {
        if (!PhotonNetwork.InRoom) return;
        if (!PhotonNetwork.IsMasterClient) return;

        if (Input.GetKeyDown(endRoundKey))
        {
            
            PhotonNetwork.CurrentRoom.SetCustomProperties(
                new PhotonHashtable { { MatchStartedKey, false } });

            Debug.Log("[Round] Master marcó fin de ronda → matchStarted=false (volver a Lobby).");
        }
    }
}
