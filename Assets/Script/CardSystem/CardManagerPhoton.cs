using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class CardManagerPhoton : MonoBehaviourPunCallbacks
{
    [SerializeField] private CardDataBase cardDatabase;
    private const string CardKey = "cardID";

    //  Llamar desde el Master cuando todos están ready
    public void DealCards()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log("[CardManager] Repartiendo cartas...");

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
                Debug.LogWarning($"[CardManager] {player.NickName} no tiene rival, se salta.");
                continue;
            }

            // Guardamos carta en customProperties del rival
            var props = new ExitGames.Client.Photon.Hashtable {
                { "cardID", cardId },
                { "cardFrom", player.NickName }
            };
            rival.SetCustomProperties(props);


            Debug.Log($"[CardManager] {player.NickName} robó {cardId}, aplicado a {rival.NickName}");
        }
    }

    public void ResetCards()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            var props = new ExitGames.Client.Photon.Hashtable { { CardKey, -1 } };
            player.SetCustomProperties(props);
        }

        // Solo limpiar en local
        if (CardEffectUI.Instance != null)
            CardEffectUI.Instance.Clear();
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
