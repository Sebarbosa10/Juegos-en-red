using UnityEngine;

public class MosaicTile : MonoBehaviour, IInteractive
{
    [SerializeField] private int tileIndex;
    private MosaicPuzzleController _controller;

    
    [SerializeField] private TextMesh indexLabel;   

    public int TileIndex => tileIndex;

    
    public void SetControllerAndIndex(MosaicPuzzleController controller, int index)
    {
        _controller = controller;
        tileIndex = index;

        if (indexLabel != null)
        {
            indexLabel.text = index.ToString();
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
            Debug.LogWarning("[MosaicTile] No tengo controller asignado, revisa MosaicPuzzleController.Awake()");
        }
    }
}
