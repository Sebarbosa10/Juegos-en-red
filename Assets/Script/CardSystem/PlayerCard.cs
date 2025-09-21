using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerCard : MonoBehaviourPunCallbacks
{
    [SerializeField] private CardDataBase cardDatabase;

    private const string CardKey = "cardID";
    public CardData CurrentCard { get; private set; }

    private void OnEnable()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(CardKey))
        {
            int cardId = (int)PhotonNetwork.LocalPlayer.CustomProperties[CardKey];
            ApplyCard(cardId);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer != PhotonNetwork.LocalPlayer) return;

        if (changedProps.ContainsKey(CardKey))
        {
            int cardId = (int)changedProps[CardKey];
            ApplyCard(cardId);
        }
    }

    private void ApplyCard(int cardId)
    {
        if (cardId >= 0)
        {
            CurrentCard = cardDatabase.GetCardById(cardId);
            Debug.Log($"[PlayerCard] {PhotonNetwork.NickName} me tocó: {CurrentCard.cardName}");
        }
        else
        {
            CurrentCard = null;
            Debug.Log("[PlayerCard] Carta reseteada");
        }
    }
}
