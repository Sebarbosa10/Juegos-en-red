using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardShuffleAnimation : MonoBehaviour
{
    public static CardShuffleAnimation Instance;

    [Header("UI References")]
    [SerializeField] private GameObject shufflePanel;
    [SerializeField] private Image[] cardImages;  // Las imágenes de tus cartas
    [SerializeField] private Sprite[] cardSprites; // Los sprites de todas las cartas

    [Header("Animation Settings")]
    [SerializeField] private float shuffleSpeed = 0.15f;
    [SerializeField] private float moveDistance = 50f;
    [SerializeField] private float rotationAmount = 10f;

    private Vector3[] _originalPositions;
    private Coroutine _shuffleCoroutine;
    private bool _isShuffling = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (shufflePanel != null)
            shufflePanel.SetActive(false);

        // Guardar posiciones originales
        if (cardImages != null && cardImages.Length > 0)
        {
            _originalPositions = new Vector3[cardImages.Length];
            for (int i = 0; i < cardImages.Length; i++)
            {
                if (cardImages[i] != null)
                    _originalPositions[i] = cardImages[i].rectTransform.anchoredPosition;
            }
        }
    }

    /// <summary>
    /// Inicia la animación de barajeo
    /// </summary>
    public void StartShuffle()
    {
        if (_isShuffling) return;

        if (shufflePanel != null)
            shufflePanel.SetActive(true);

        // Asignar sprites aleatorios a las cartas
        AssignRandomSprites();

        _shuffleCoroutine = StartCoroutine(ShuffleCoroutine());
    }

    /// <summary>
    /// Detiene la animación y muestra la carta asignada
    /// </summary>
    public void StopAndReveal(CardData card, string fromPlayer)
    {
        if (_shuffleCoroutine != null)
        {
            StopCoroutine(_shuffleCoroutine);
            _shuffleCoroutine = null;
        }

        _isShuffling = false;

        // Resetear posiciones
        ResetCardPositions();

        // Ocultar panel de barajeo
        if (shufflePanel != null)
            shufflePanel.SetActive(false);

        // Mostrar carta en el UI normal
        if (CardEffectUI.Instance != null)
        {
            CardEffectUI.Instance.ShowCard(card, fromPlayer);
        }
    }

    private void AssignRandomSprites()
    {
        if (cardSprites == null || cardSprites.Length == 0) return;

        foreach (var cardImage in cardImages)
        {
            if (cardImage != null)
            {
                int randomIndex = Random.Range(0, cardSprites.Length);
                cardImage.sprite = cardSprites[randomIndex];
                cardImage.gameObject.SetActive(true);
            }
        }
    }

    private IEnumerator ShuffleCoroutine()
    {
        _isShuffling = true;

        while (_isShuffling)
        {
            // Mover cada carta aleatoriamente
            for (int i = 0; i < cardImages.Length; i++)
            {
                if (cardImages[i] != null)
                {
                    // Posición aleatoria
                    Vector3 randomOffset = new Vector3(
                        Random.Range(-moveDistance, moveDistance),
                        Random.Range(-moveDistance, moveDistance),
                        0f
                    );
                    cardImages[i].rectTransform.anchoredPosition = _originalPositions[i] + randomOffset;

                    // Rotación aleatoria
                    float randomRotation = Random.Range(-rotationAmount, rotationAmount);
                    cardImages[i].rectTransform.localRotation = Quaternion.Euler(0, 0, randomRotation);

                    // Cambiar sprite aleatoriamente
                    if (cardSprites != null && cardSprites.Length > 0)
                    {
                        int randomIndex = Random.Range(0, cardSprites.Length);
                        cardImages[i].sprite = cardSprites[randomIndex];
                    }
                }
            }

            yield return new WaitForSeconds(shuffleSpeed);
        }
    }

    private void ResetCardPositions()
    {
        for (int i = 0; i < cardImages.Length; i++)
        {
            if (cardImages[i] != null)
            {
                cardImages[i].rectTransform.anchoredPosition = _originalPositions[i];
                cardImages[i].rectTransform.localRotation = Quaternion.identity;
            }
        }
    }

    public bool IsShuffling => _isShuffling;
}