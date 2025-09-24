using Photon.Pun;
using UnityEngine;

public class PlayerCard : MonoBehaviourPunCallbacks
{
    [SerializeField] private CardDataBase cardDatabase;
    private CardEffectManager _effectManager;

    private const string CardKey = "cardID";
    public CardData CurrentCard { get; private set; }

    private void Awake()
    {
        _effectManager = GetComponent<CardEffectManager>();
        //photonView.Owner.TagObject = photonView;
    }

    public void ApplyCard(int cardId)
    {
        if (cardId >= 0)
        {
            CurrentCard = cardDatabase.GetCardById(cardId);
            Debug.Log($"[PlayerCard] {PhotonNetwork.NickName} me tocó: {CurrentCard.cardName}");

            if (_effectManager != null && CurrentCard != null)
                _effectManager.ActivateEffects(CurrentCard);
        }
        else
        {
            CurrentCard = null;
            Debug.Log("[PlayerCard] Carta reseteada");
        }
    }

    [PunRPC]
    public void RPC_AssignCard(int cardId, string fromPlayer)
    {
        ApplyCard(cardId);

        Debug.Log($"[PlayerCard] Me aplicaron la carta {CurrentCard.cardName} desde {fromPlayer}");

        if (CardEffectUI.Instance != null)
            CardEffectUI.Instance.ShowCard(CurrentCard.cardName, fromPlayer);

        if (_effectManager != null && CurrentCard != null)
            _effectManager.ActivateEffects(CurrentCard);
    }
}
