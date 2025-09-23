using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using UnityEngine;

public class CardManagerPhoton : MonoBehaviourPunCallbacks
{
    [SerializeField] private CardDataBase cardDatabase;
    private const string CardKey = "cardID";

    public void DealCards()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // Copiar IDs
        List<int> availableCards = new List<int>();
        for (int i = 0; i < cardDatabase.allCards.Count; i++)
        {
            availableCards.Add(i);
        }

        // Barajar
        ShuffleCards(availableCards);

        // Asignar
        int playerIndex = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            int cardId = availableCards[playerIndex];
            playerIndex++;

            // Buscar rival
            var rival = TeamManager.Instance.GetRival(player);
            if (rival == null)
            {
                Debug.LogWarning($"[CardManager] {player.NickName} no tiene rival, se salta.");
                continue;
            }

            // Guardar propiedad (persistencia/debug)
            Hashtable props = new Hashtable { { CardKey, cardId } };
            rival.SetCustomProperties(props);

            // ✅ Enviar RPC al PhotonView del rival
            if (rival.TagObject is PhotonView rivalView)
            {
                rivalView.RPC("RPC_AssignCard", rival, cardId, player.NickName);
                Debug.Log($"[CardManager] {player.NickName} robó {cardId}, aplicado a {rival.NickName}");
            }
            else
            {
                Debug.LogWarning($"[CardManager] {rival.NickName} no tiene PhotonView asignado en TagObject.");
            }
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
                    playerCard.ApplyCard(-1);
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
