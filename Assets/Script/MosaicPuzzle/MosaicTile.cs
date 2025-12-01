using UnityEngine;

public class MosaicTile : MonoBehaviour, IInteractive
{
    [SerializeField] private int tileIndex;
    private MosaicPuzzleController _controller;

    [Header("Visual Feedback")]
    [SerializeField] private TextMesh indexLabel;
    [SerializeField] private Renderer tileRenderer;

    [Header("Selection Settings")]
    [SerializeField] private bool useColorFeedback = true;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private float selectedScale = 1.1f;
    [SerializeField] private float scaleAnimationSpeed = 8f;

    private bool _isSelected = false;
    private Vector3 _originalScale;
    private Color _originalColor;
    private Material _material;
    private bool _initialized = false;

    public int TileIndex => tileIndex;

    private void Awake()
    {
        _originalScale = transform.localScale;

        if (tileRenderer == null)
        {
            tileRenderer = GetComponent<Renderer>();
        }

        
        if (tileRenderer != null)
        {
            
            _material = tileRenderer.material;
            _originalColor = _material.color;
            _initialized = true;
        }
    }

    private void Update()
    {
        
        Vector3 targetScale = _isSelected ? _originalScale * selectedScale : _originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleAnimationSpeed);
    }

    public void SetControllerAndIndex(MosaicPuzzleController controller, int index)
    {
        _controller = controller;
        tileIndex = index;

        if (indexLabel != null)
        {
            indexLabel.text = index.ToString();
        }
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;

        if (!useColorFeedback) return;
        if (!_initialized || _material == null) return;

        
        if (selected)
        {
            _material.color = selectedColor;
        }
        else
        {
            _material.color = _originalColor;
        }
    }

    public void Interact()
    {
        if (_controller != null)
        {
            _controller.OnTileClicked(this);
        }
        else
        {
            Debug.LogWarning("[MosaicTile] No tengo controller asignado.");
        }
    }

    private void OnDestroy()
    {
        
        if (_material != null)
        {
            Destroy(_material);
        }
    }
}