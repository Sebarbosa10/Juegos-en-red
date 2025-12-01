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
        {
            Debug.LogError($"[PlayerCard] CardEffectManager NO encontrado en {gameObject.name}!");
        }

        if (cardDatabase == null)
        {
            Debug.LogError($"[PlayerCard] cardDatabase NO asignado en {gameObject.name}!");
        }
    }

    public void ApplyCard(int cardId, string fromPlayer = null)
    {
        Debug.Log($"[PlayerCard] ApplyCard({cardId}, {fromPlayer}) en {photonView.Owner.NickName}");

        if (cardDatabase == null)
        {
            Debug.LogError("[PlayerCard] cardDatabase es NULL!");
            return;
        }

        
        if (cardId < 0)
        {
            CurrentCard = null;

            if (photonView.IsMine && _effectManager != null)
            {
                _effectManager.ResetAllEffects();
            }

            if (photonView.IsMine && CardEffectUI.Instance != null)
            {
                CardEffectUI.Instance.Clear();
            }

            Debug.Log($"[PlayerCard] Carta reseteada para {photonView.Owner.NickName}");
            return;
        }

       
        CurrentCard = cardDatabase.GetCardById(cardId);

        if (CurrentCard == null)
        {
            Debug.LogError($"[PlayerCard] No se encontró carta con ID {cardId}!");
            return;
        }

        Debug.Log($"[PlayerCard] {photonView.Owner.NickName} recibió carta: {CurrentCard.cardName} (Effect: {CurrentCard.cardEffectType})");

        
        if (photonView.IsMine)
        {
            
            if (fromPlayer != null && CardEffectUI.Instance != null)
            {
                CardEffectUI.Instance.ShowCard(CurrentCard, fromPlayer);
            }

            
            if (_effectManager != null)
            {
                _effectManager.ActivateEffects(CurrentCard);
            }
            else
            {
                Debug.LogError("[PlayerCard] _effectManager es NULL!");
            }
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


