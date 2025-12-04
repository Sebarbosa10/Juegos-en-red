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

        if (_effectManager == null)
            Debug.LogError($"[PlayerCard] CardEffectManager NO encontrado en {gameObject.name}!");

        if (cardDatabase == null)
            Debug.LogError($"[PlayerCard] cardDatabase NO asignado en {gameObject.name}!");
    }

    public void ApplyCard(int cardId, string fromPlayer = null)
    {
        if (cardDatabase == null) return;

        // Resetear carta
        if (cardId < 0)
        {
            CurrentCard = null;

            if (photonView.IsMine)
            {
                _effectManager?.ResetAllEffects();
                CardEffectUI.Instance?.Clear();
            }
            return;
        }

        CurrentCard = cardDatabase.GetCardById(cardId);
        if (CurrentCard == null) return;

        Debug.Log($"[PlayerCard] {photonView.Owner.NickName} recibió carta: {CurrentCard.cardName}");

        if (photonView.IsMine)
        {
            // Detener animación y mostrar carta
            if (CardShuffleAnimation.Instance != null)
            {
                CardShuffleAnimation.Instance.StopAndReveal(CurrentCard, fromPlayer);
            }
            else
            {
                CardEffectUI.Instance?.ShowCard(CurrentCard, fromPlayer);
            }

            // Activar efecto
            _effectManager?.ActivateEffects(CurrentCard);
        }
    }

    public void ClearCard()
    {
        ApplyCard(-1, null);
    }

    public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (target != photonView.Owner) return;

        if (changedProps.ContainsKey(CardKey))
        {
            int cardId = (int)changedProps[CardKey];
            string fromName = changedProps.ContainsKey("cardFrom")
                ? changedProps["cardFrom"].ToString()
                : "???";

            ApplyCard(cardId, fromName);
        }
    }
}