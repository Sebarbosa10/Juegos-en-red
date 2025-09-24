using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManagerPhoton : MonoBehaviourPunCallbacks
{
    [SerializeField] private CardDataBase cardDatabase;
    private const string CardKey = "cardID";

    // 🔹 Llamar desde el Master cuando todos están ready
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

            // Guardamos carta en customProperties
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable { { CardKey, cardId } };
            rival.SetCustomProperties(props);

            if (rival.TagObject is GameObject rivalGO)
            {
                var playerCard = rivalGO.GetComponent<PlayerCard>();
                if (playerCard != null)
                {
                    // 🔹 Llamamos al RPC en todos los clientes
                    playerCard.photonView.RPC("RPC_AssignCard", RpcTarget.All, cardId, player.NickName);

                    Debug.Log($"[CardManager] {player.NickName} robó {cardId}, aplicado a {rival.NickName}");
                }
                else
                {
                    Debug.LogWarning($"[CardManager] {rival.NickName} tiene GO pero no PlayerCard.");
                }
            }
            else
            {
                Debug.LogWarning($"[CardManager] {rival.NickName} aún no tiene TagObject asignado.");
            }
        }
    }


    public void ResetCards()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable { { CardKey, -1 } };
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
