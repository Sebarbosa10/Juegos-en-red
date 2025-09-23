using UnityEngine;
using Photon.Pun;

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

    private void OnEnable()
    {

        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(CardKey))
        {
            int cardId = (int)PhotonNetwork.LocalPlayer.CustomProperties[CardKey];
            ApplyCard(cardId);
        }
    }

    public void ApplyCard(int cardId)
    {
        if (cardId >= 0)
        {
            CurrentCard = cardDatabase.GetCardById(cardId);
            Debug.Log($"[PlayerCard] {PhotonNetwork.NickName} me tocó: {CurrentCard.cardName}");

            //  Apenas asignamos, activamos el efecto
            if (_effectManager != null && CurrentCard != null)
            {
                _effectManager.ActivateEffects(CurrentCard);
            }
        }
        else
        {
            CurrentCard = null;
            Debug.Log("[PlayerCard] Carta reseteada");
        }
    }
}
