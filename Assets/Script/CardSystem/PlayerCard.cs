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
            Debug.Log($"[PlayerCard] {PhotonNetwork.NickName} me tocó: {CurrentCard.cardName}");

            // Mostrar UI si vino con atacante
            if (fromPlayer != null && CardEffectUI.Instance != null)
                CardEffectUI.Instance.ShowCard(CurrentCard.cardName, fromPlayer);

            if (_effectManager != null && CurrentCard != null)
                _effectManager.ActivateEffects(CurrentCard);
        }
        else
        {
            CurrentCard = null;
            Debug.Log("[PlayerCard] Carta reseteada");
        }
    }

    // 🔹 Detecta cuando alguien le setea la carta
    public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (target != photonView.Owner) return;

        if (changedProps.ContainsKey(CardKey))
        {
            int cardId = (int)changedProps[CardKey];

            //  el "attacker" es quien me eligió como rival
            Player attacker = TeamManager.Instance.GetRival(target);

            string fromName = attacker != null ? attacker.NickName : "???";
            ApplyCard(cardId, fromName);
        }
    }
}
