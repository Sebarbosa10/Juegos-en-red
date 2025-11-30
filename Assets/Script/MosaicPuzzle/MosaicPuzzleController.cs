using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MosaicPuzzleController : MonoBehaviourPun, IPunObservable
{
    
    [SerializeField] private int rows = 3;
    [SerializeField] private int cols = 3;
    [SerializeField] private float cellSize = 1.2f;

    
    [SerializeField] private MosaicTile[] tiles;

    
    [SerializeField] private Transform origin;   

    
    [SerializeField] private string teamFilter = "Blue"; 

   
    [SerializeField] private UnityEngine.Events.UnityEvent onSolved;

    private int[] _slots;               
    private int _selectedTileIndex = -1;
    private bool _solved = false;

    private const string TeamKey = "team";

    private void Awake()
    {
       
        if (tiles == null || tiles.Length == 0)
        {
            tiles = GetComponentsInChildren<MosaicTile>();
        }

        
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == null) continue;
            tiles[i].SetControllerAndIndex(this, i);
        }

        if (origin == null)
        {
            origin = transform; 
        }

        if (photonView == null)
        {
            Debug.LogError("[MosaicPuzzle] No hay PhotonView en este GameObject. Añádelo o no podrás usar RPC.");
        }
    }

    private void Start()
    {
        int total = rows * cols;
        if (tiles == null || tiles.Length != total)
        {
            
            return;
        }

        _slots = new int[total];

        if (PhotonNetwork.IsMasterClient)
        {
            
            for (int i = 0; i < total; i++)
                _slots[i] = i;

            Shuffle(_slots);
            Debug.Log("[MosaicPuzzle] Estado inicial (desordenado): " + string.Join(",", _slots));
            Debug.Log($"[MosaicPuzzle] origin at world pos = {origin.position}");
            UpdateTilePositions();
        }
    }

    public void OnTileClicked(MosaicTile tile)
    {
        if (tile == null)
        {
            
            return;
        }

        if (_solved) return;

        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            
            return;
        }

        if (photonView == null)
        {
            
            return;
        }

        string senderTeam = GetLocalTeam();
        if (!AcceptsFromTeam(senderTeam))
        {
            Debug.Log($"[MosaicPuzzle] Equipo {senderTeam} no puede interactuar con este puzzle (filtro={teamFilter}).");
            return;
        }

        Debug.Log($"[MosaicPuzzle] OnTileClicked desde cliente local → tileIndex={tile.TileIndex}");
        photonView.RPC(nameof(RPC_ClickTile), RpcTarget.MasterClient, tile.TileIndex, senderTeam);
    }

    [PunRPC]
    private void RPC_ClickTile(int tileIndex, string senderTeam, PhotonMessageInfo info)
    {
        Debug.Log($"[MosaicPuzzle] RPC_ClickTile en MASTER → tileIndex={tileIndex}, senderTeam={senderTeam}, solved={_solved}");

        if (!PhotonNetwork.IsMasterClient) return;
        if (_solved) return;
        if (!AcceptsFromTeam(senderTeam)) return;
        if (_slots == null || _slots.Length == 0) return;

        
        if (_selectedTileIndex < 0)
        {
            _selectedTileIndex = tileIndex;
            Debug.Log($"[MosaicPuzzle] Seleccionando tile {tileIndex}");
        }
        else
        {
           
            if (_selectedTileIndex != tileIndex)
            {
                Debug.Log($"[MosaicPuzzle] Swapping tileA={_selectedTileIndex} con tileB={tileIndex}");
                SwapTilesByTileIndex(_selectedTileIndex, tileIndex);
                Debug.Log("[MosaicPuzzle] Nuevo estado slots: " + string.Join(",", _slots));
                CheckSolved();
            }
            else
            {
                Debug.Log("[MosaicPuzzle] Segundo click en la misma tile, no swap");
            }

            _selectedTileIndex = -1;
        }

        UpdateTilePositions();
    }

    private void SwapTilesByTileIndex(int tileA, int tileB)
    {
        if (_slots == null) return;

        int indexA = System.Array.IndexOf(_slots, tileA);
        int indexB = System.Array.IndexOf(_slots, tileB);

        if (indexA < 0 || indexB < 0)
        {
            Debug.LogWarning($"[MosaicPuzzle] No encontré indices para tiles A={tileA}, B={tileB} en slots.");
            return;
        }

        int temp = _slots[indexA];
        _slots[indexA] = _slots[indexB];
        _slots[indexB] = temp;
    }

    private void CheckSolved()
    {
        if (_slots == null) return;

        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] != i)
            {
                return; 
            }
        }

        _solved = true;
        Debug.Log("[MosaicPuzzle] Puzzle resuelto!");

        onSolved?.Invoke();

        
        if (PhotonNetwork.IsMasterClient && ScoreManager.Instance != null)
        {
            string team = string.IsNullOrEmpty(teamFilter) ? GetLocalTeam() : teamFilter;
            if (!string.IsNullOrEmpty(team))
            {
                ScoreManager.Instance.AddPoint(team);
                Debug.Log($"[MosaicPuzzle] Punto otorgado al equipo {team} (Puzzle 3).");
            }
        }
    }

   

    private void UpdateTilePositions()
    {
        if (_slots == null || tiles == null) return;
        if (origin == null) origin = transform;

        for (int slotIndex = 0; slotIndex < _slots.Length; slotIndex++)
        {
            int tileIndex = _slots[slotIndex];
            if (tileIndex < 0 || tileIndex >= tiles.Length) continue;
            var tile = tiles[tileIndex];
            if (tile == null) continue;

            int r = slotIndex / cols;
            int c = slotIndex % cols;

            
            Vector3 localPos = new Vector3(
                c * cellSize,
                0f,
                -r * cellSize
            );

            
            Vector3 worldPos = origin.position + origin.rotation * localPos;

            tile.transform.position = worldPos;
        }
    }

    private void Shuffle(int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int rnd = Random.Range(0, i + 1);
            int tmp = array[i];
            array[i] = array[rnd];
            array[rnd] = tmp;
        }

        
        bool alreadySolved = true;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] != i)
            {
                alreadySolved = false;
                break;
            }
        }
        if (alreadySolved && array.Length > 1)
        {
            int tmp = array[0];
            array[0] = array[1];
            array[1] = tmp;
        }
    }

    private string GetLocalTeam()
    {
        var p = PhotonNetwork.LocalPlayer;
        if (p?.CustomProperties == null) return "";
        return p.CustomProperties.TryGetValue(TeamKey, out object t) ? (t as string ?? "") : "";
    }

    private bool AcceptsFromTeam(string senderTeam)
    {
        if (string.IsNullOrEmpty(teamFilter)) return true;
        return string.Equals(senderTeam, teamFilter);
    }

  

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            
            stream.SendNext(_solved);
            stream.SendNext(_selectedTileIndex);

            if (_slots == null)
            {
                stream.SendNext(0);
            }
            else
            {
                stream.SendNext(_slots.Length);
                for (int i = 0; i < _slots.Length; i++)
                    stream.SendNext(_slots[i]);
            }
        }
        else
        {
            
            _solved = (bool)stream.ReceiveNext();
            _selectedTileIndex = (int)stream.ReceiveNext();

            int len = (int)stream.ReceiveNext();
            if (_slots == null || _slots.Length != len)
                _slots = new int[len];

            for (int i = 0; i < len; i++)
                _slots[i] = (int)stream.ReceiveNext();

            UpdateTilePositions();
        }
    }
}
