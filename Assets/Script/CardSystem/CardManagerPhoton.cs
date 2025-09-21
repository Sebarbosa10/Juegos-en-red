using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;

public class CardManagerPhoton : MonoBehaviourPunCallbacks
{
    [SerializeField] private CardDataBase cardDataBase;
    private const string CardKey = "cardID";

    public void DealCards()
    {
        if (!PhotonNetwork.IsMasterClient) return;


        //Copy List
        List<int> availableCards = new List<int>();
        for (int i = 0; i < cardDataBase.allCards.Count; i++)
        {
            availableCards.Add(i);
        }

        //Shuffle Cards
        ShuffleCards(availableCards);

        //Draw Card
        int playerIndex = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            int cardId = availableCards[playerIndex];
            playerIndex++;

            //Save in CustomProperties
            Hashtable props = new Hashtable { { CardKey, cardId } };
            player.SetCustomProperties(props);

            if (player == PhotonNetwork.LocalPlayer)
            {
                var playerCard = FindObjectOfType<PlayerCard>();
                if (playerCard != null)
                {
                    playerCard.ApplyCard(cardId);
                }
            }

        }

    }

    public void ResetCards()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Hashtable props = new Hashtable { { CardKey, null } };
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
