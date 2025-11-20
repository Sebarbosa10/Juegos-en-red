using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class CardManagerPhoton : MonoBehaviourPunCallbacks
{
    [SerializeField] private CardDataBase cardDatabase;
    private const string CardKey = "cardID";

    public void DealCards()
    {
        if (!PhotonNetwork.IsMasterClient) return;

      

        List<int> availableCards = new List<int>();
        for (int i = 0; i < cardDatabase.allCards.Count; i++)
            availableCards.Add(i);

        ShuffleCards(availableCards);

        int playerIndex = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            int cardId = availableCards[playerIndex++];
            var rival = TeamManager.Instance.GetRival(player);
            if (rival == null)
            {
               
                continue;
            }

            var props = new ExitGames.Client.Photon.Hashtable {
                { CardKey, cardId },
                { "cardFrom", player.NickName }
            };
            rival.SetCustomProperties(props);

            
        }
    }

    public void ResetCards()
    {
        
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            var props = new ExitGames.Client.Photon.Hashtable { { CardKey, -1 } };
            player.SetCustomProperties(props);
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
