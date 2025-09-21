using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerCard : MonoBehaviourPunCallbacks
{
    [SerializeField] private CardDataBase cardDatabase;
    public CardData currentCard { get; private set; }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if(targetPlayer != PhotonNetwork.LocalPlayer) return;

        if (changedProps.ContainsKey("cardID"))
        {
            int cardId = (int)changedProps["cardID"];
            if (cardId >= 0)
            {
                currentCard = cardDatabase.GetCardById(cardId);
                Debug.Log($"[PlayerCard] Me tocó: {currentCard.cardName}");
            }
            else
            {
                currentCard = null;
                Debug.Log("[PlayerCard] Carta reseteada");
            }
        }

    }
}
