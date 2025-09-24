using Photon.Pun;
using UnityEngine;
using Photon.Realtime;

public class PlayerCard : MonoBehaviourPunCallbacks
{
    [SerializeField] private CardDataBase cardDatabase;
    private CardEffectManager _effectManager;

    private const string CardKey = "cardID";
    public CardData CurrentCard { get; private set; }

    private void Awake()
    {
        _effectManager = GetComponent<CardEffectManager>();
    }

    public void ApplyCard(int cardId, string fromPlayer = null)
    {
        if (cardId >= 0)
        {
            CurrentCard = cardDatabase.GetCardById(cardId);
            Debug.Log($"[PlayerCard] {photonView.Owner.NickName} me tocó: {CurrentCard.cardName}");

            // Solo mostrar la UI si soy el jugador local
            if (photonView.IsMine && fromPlayer != null && CardEffectUI.Instance != null)
                CardEffectUI.Instance.ShowCard(CurrentCard.cardName, fromPlayer);

            // Efectos se aplican solo en el dueño local
            if (photonView.IsMine)
                _effectManager?.ActivateEffects(CurrentCard);
        }
        else
        {
            CurrentCard = null;
            if (photonView.IsMine)
                Debug.Log("[PlayerCard] Carta reseteada");
        }
    }

    public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {
        // Solo reacciono si soy yo
        if (target != photonView.Owner) return;

        if (changedProps.ContainsKey(CardKey))
        {
            int cardId = (int)changedProps[CardKey];
            string fromName = changedProps.ContainsKey("cardFrom") ? changedProps["cardFrom"].ToString() : "???";

            ApplyCard(cardId, fromName);
        }
    }

}
