using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardEffectUI : MonoBehaviour
{
    public static CardEffectUI Instance;

    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI cardSourceText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (cardImage != null)
            cardImage.enabled = false;

        if (cardSourceText != null)
            cardSourceText.text = "";
    }

    public void ShowCard(CardData cardData, string fromPlayer)
    {
        if (cardData == null || cardImage == null)
            return;

        // show icon
        cardImage.sprite = cardData.icon;
        cardImage.enabled = true;

        // show who applied the card
        if (cardSourceText != null)
            cardSourceText.text = "From " + fromPlayer;
    }

    public void Clear()
    {
        if (cardImage != null)
            cardImage.enabled = false;

        if (cardSourceText != null)
            cardSourceText.text = "";
    }
}
