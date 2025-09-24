using TMPro;
using UnityEngine;

public class CardEffectUI : MonoBehaviour
{
    public static CardEffectUI Instance;

    [SerializeField] private TextMeshProUGUI cardText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (cardText != null)
            cardText.text = "";
    }

    public void ShowCard(string cardName, string fromPlayer)
    {
        if (cardText != null)
        {
            cardText.text = $"Te aplicaron: {cardName}\n(from {fromPlayer})";
        }
    }

    public void Clear()
    {
        if (cardText != null)
            cardText.text = "";
    }
}
