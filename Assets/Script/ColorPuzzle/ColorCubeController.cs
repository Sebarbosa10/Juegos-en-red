using UnityEngine;
using Photon.Pun;

public class ColorCubeController : MonoBehaviourPunCallbacks, IPunObservable
{
    public enum CubeColor
    {
        Blue = 0,
        Yellow = 1,
        Red = 2
    }

    public static ColorCubeController Instance;

    
    [SerializeField] private Material blueMat;
    [SerializeField] private Material yellowMat;
    [SerializeField] private Material redMat;


    [SerializeField] private float changeInterval;  

    private Renderer _renderer;
    private float _timer;

    
    [SerializeField] private int currentColorIndex = 0; 

    public int CurrentColorIndex => currentColorIndex;

    private void Awake()
    {
        Instance = this;
        _renderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        
        if (PhotonNetwork.IsMasterClient)
        {
            SetRandomColor();
        }
        else
        {
            
            ApplyColor((CubeColor)currentColorIndex);
        }
    }

    private void Update()
    {
        
        if (!PhotonNetwork.IsMasterClient) return;

        _timer += Time.deltaTime;
        if (_timer >= changeInterval)
        {
            _timer = 0f;
            SetRandomColor();
        }
    }

    private void SetRandomColor()
    {
        int newColor = Random.Range(0, 3); 
        currentColorIndex = newColor;
        ApplyColor((CubeColor)currentColorIndex);

        Debug.Log($"[ColorCube] Master cambió color a {(CubeColor)currentColorIndex}");
        
    }

    private void ApplyColor(CubeColor color)
    {
        if (_renderer == null) return;

        switch (color)
        {
            case CubeColor.Blue:
                _renderer.material = blueMat;
                break;
            case CubeColor.Yellow:
                _renderer.material = yellowMat;
                break;
            case CubeColor.Red:
                _renderer.material = redMat;
                break;
        }
    }

  
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            
            stream.SendNext(currentColorIndex);
        }
        else
        {
            
            currentColorIndex = (int)stream.ReceiveNext();
            ApplyColor((CubeColor)currentColorIndex);
        }
    }
}
