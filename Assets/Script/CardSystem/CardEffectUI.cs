using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardEffectUI : MonoBehaviour
{
    public static CardEffectUI Instance;

   
    [SerializeField] private Image cardImage;     
    [SerializeField] private Image effectImage;   
    [SerializeField] private TextMeshProUGUI cardSourceText;

    private Sprite _currentEffectSprite; 

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (cardImage != null)
            cardImage.enabled = false;

        if (effectImage != null)
            effectImage.enabled = false;

        if (cardSourceText != null)
            cardSourceText.text = "";
    }

    public void ShowCard(CardData cardData, string fromPlayer)
    {
        if (cardData == null)
            return;

        
        if (cardImage != null)
        {
            cardImage.sprite = cardData.icon;
            cardImage.enabled = true;
        }

        
        _currentEffectSprite = cardData.F_UI;

        if (cardSourceText != null)
            cardSourceText.text = "From " + fromPlayer;
    }

    
    public void ShowEffectActive()
    {
        if (effectImage != null && _currentEffectSprite != null)
        {
            effectImage.sprite = _currentEffectSprite;
            effectImage.enabled = true;
        }
    }

   
    public void HideEffectActive()
    {
        if (effectImage != null)
        {
            effectImage.enabled = false;
        }
    }

    public void Clear()
    {
        if (cardImage != null)
            cardImage.enabled = false;

        if (effectImage != null)
            effectImage.enabled = false;

        if (cardSourceText != null)
            cardSourceText.text = "";

        _currentEffectSprite = null;
    }
}