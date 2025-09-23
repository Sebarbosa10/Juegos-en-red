using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;

public class CardManagerPhoton : MonoBehaviourPunCallbacks
{
    [SerializeField] private CardDataBase cardDatabase;
    private const string CardKey = "cardID";

    public void DealCards()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // Copy IDs
        List<int> availableCards = new List<int>();
        for (int i = 0; i < cardDatabase.allCards.Count; i++)
        {
            availableCards.Add(i);
        }

        // Shuffle
        ShuffleCards(availableCards);

        // Asignar
        int playerIndex = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            int cardId = availableCards[playerIndex];
            playerIndex++;

            //  Look for rival
            var rival = TeamManager.Instance.GetRival(player);
            if (rival == null)
            {
                UnityEngine.Debug.LogWarning($"[CardManager] {player.NickName} no tiene rival, se salta.");
                continue;
            }

            Hashtable props = new Hashtable { { CardKey, cardId } };
            rival.SetCustomProperties(props);

            photonView.RPC(nameof(RPC_AssignCard), rival, cardId, player.NickName);
            UnityEngine.Debug.Log($"[CardManager] {player.NickName} robó {cardId}, aplicado a {rival.NickName}");
        }
    }


    [PunRPC]
    private void RPC_AssignCard(int cardId, string fromPlayer, PhotonMessageInfo info)
    {
        var playerCard = FindObjectOfType<PlayerCard>();
        if (playerCard != null)
        {
            playerCard.ApplyCard(cardId);
            UnityEngine.Debug.Log($"[PlayerCard] Me aplicaron la carta {playerCard.CurrentCard.cardName} desde {fromPlayer}");
        }
    }

    public void ResetCards()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Hashtable props = new Hashtable { { CardKey, -1 } };
            player.SetCustomProperties(props);

            if (player == PhotonNetwork.LocalPlayer)
            {
                var playerCard = FindObjectOfType<PlayerCard>();
                if (playerCard != null)
                {
                    playerCard.ApplyCard(-1);
                }
            }
        }
    }

    private void ShuffleCards(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(0, i + 1);
            int temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }
}
