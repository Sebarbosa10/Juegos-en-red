using System.Collections;
using UnityEngine;
using Photon.Pun;

public class MosaicPuzzleController : MonoBehaviourPun, IPunObservable
{
    
    [SerializeField] private int rows = 3;
    [SerializeField] private int cols = 3;

    
    [SerializeField] private MosaicTile[] tiles;

    
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float arcHeight = 0.3f;

    
    [SerializeField] private string teamFilter = "Blue";

    
    [SerializeField] private UnityEngine.Events.UnityEvent onSolved;
    [SerializeField] private UnityEngine.Events.UnityEvent onTileSwapped;

    private int[] _slots;
    private Vector3[] _slotPositions;
    private int _selectedTileIndex = -1;
    private bool _solved = false;
    private bool _isAnimating = false;

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

        _slotPositions = new Vector3[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] != null)
            {
                _slotPositions[i] = tiles[i].transform.position;
            }
        }

        if (photonView == null)
        {
            Debug.LogError("[MosaicPuzzle] No hay PhotonView en este GameObject.");
        }
    }

    private void Start()
    {
        int total = rows * cols;
        if (tiles == null || tiles.Length != total)
        {
            Debug.LogError($"[MosaicPuzzle] Se esperaban {total} tiles pero hay {tiles?.Length ?? 0}");
            return;
        }

        _slots = new int[total];

        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < total; i++)
                _slots[i] = i;

            Shuffle(_slots);
            Debug.Log("[MosaicPuzzle] Estado inicial (mezclado): " + string.Join(",", _slots));

            ApplySlotPositions();
        }
    }

    private void ApplySlotPositions()
    {
        if (_slots == null || tiles == null || _slotPositions == null) return;

        for (int slotIndex = 0; slotIndex < _slots.Length; slotIndex++)
        {
            int tileIndex = _slots[slotIndex];
            if (tileIndex < 0 || tileIndex >= tiles.Length) continue;

            var tile = tiles[tileIndex];
            if (tile == null) continue;

            tile.transform.position = _slotPositions[slotIndex];
        }
    }

    public void OnTileClicked(MosaicTile tile)
    {
        if (tile == null) return;
        if (_solved) return;
        if (_isAnimating) return;

        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom) return;
        if (photonView == null) return;

        string senderTeam = GetLocalTeam();
        if (!AcceptsFromTeam(senderTeam))
        {
            Debug.Log($"[MosaicPuzzle] Equipo {senderTeam} no puede interactuar.");
            return;
        }

        photonView.RPC(nameof(RPC_ClickTile), RpcTarget.MasterClient, tile.TileIndex, senderTeam);
    }

    [PunRPC]
    private void RPC_ClickTile(int tileIndex, string senderTeam, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (_solved) return;
        if (!AcceptsFromTeam(senderTeam)) return;
        if (_slots == null || _slots.Length == 0) return;

        if (_selectedTileIndex < 0)
        {
            _selectedTileIndex = tileIndex;
            photonView.RPC(nameof(RPC_TileSelected), RpcTarget.All, tileIndex);
        }
        else
        {
            if (_selectedTileIndex != tileIndex)
            {
                int tileA = _selectedTileIndex;
                int tileB = tileIndex;

                
                SwapTilesInSlots(tileA, tileB);

                
                int[] currentSlots = (int[])_slots.Clone();

                
                photonView.RPC(nameof(RPC_AnimateSwapAndCheck), RpcTarget.All, tileA, tileB, currentSlots);
            }
            else
            {
                photonView.RPC(nameof(RPC_TileDeselected), RpcTarget.All);
            }

            _selectedTileIndex = -1;
        }
    }

    private void SwapTilesInSlots(int tileA, int tileB)
    {
        if (_slots == null) return;

        int slotOfA = System.Array.IndexOf(_slots, tileA);
        int slotOfB = System.Array.IndexOf(_slots, tileB);

        if (slotOfA < 0 || slotOfB < 0)
        {
            Debug.LogWarning($"[MosaicPuzzle] No encontré slots para tiles A={tileA}, B={tileB}");
            return;
        }

        (_slots[slotOfA], _slots[slotOfB]) = (_slots[slotOfB], _slots[slotOfA]);

        Debug.Log($"[MosaicPuzzle] Swap: tile {tileA} <-> tile {tileB}");
        Debug.Log($"[MosaicPuzzle] Nuevo estado: [{string.Join(",", _slots)}]");
    }

    [PunRPC]
    private void RPC_TileSelected(int tileIndex)
    {
        if (tileIndex >= 0 && tileIndex < tiles.Length && tiles[tileIndex] != null)
        {
            tiles[tileIndex].SetSelected(true);
        }
    }

    [PunRPC]
    private void RPC_TileDeselected()
    {
        foreach (var tile in tiles)
        {
            if (tile != null)
                tile.SetSelected(false);
        }
    }

    [PunRPC]
    private void RPC_AnimateSwapAndCheck(int tileIndexA, int tileIndexB, int[] newSlots)
    {
        
        _slots = newSlots;

        StartCoroutine(AnimateSwapAndCheckCoroutine(tileIndexA, tileIndexB));
    }

    private IEnumerator AnimateSwapAndCheckCoroutine(int tileIndexA, int tileIndexB)
    {
        _isAnimating = true;

       
        foreach (var tile in tiles)
        {
            if (tile != null)
                tile.SetSelected(false);
        }

        MosaicTile tileA = tiles[tileIndexA];
        MosaicTile tileB = tiles[tileIndexB];

        if (tileA == null || tileB == null)
        {
            _isAnimating = false;
            yield break;
        }

        Vector3 startPosA = tileA.transform.position;
        Vector3 startPosB = tileB.transform.position;
        Vector3 endPosA = startPosB;
        Vector3 endPosB = startPosA;

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            float curveT = movementCurve.Evaluate(t);

            float arcT = Mathf.Sin(t * Mathf.PI);

            Vector3 posA = Vector3.Lerp(startPosA, endPosA, curveT);
            posA.y += arcHeight * arcT;

            Vector3 posB = Vector3.Lerp(startPosB, endPosB, curveT);
            posB.y += arcHeight * arcT * 0.5f;

            tileA.transform.position = posA;
            tileB.transform.position = posB;

            yield return null;
        }

        
        tileA.transform.position = endPosA;
        tileB.transform.position = endPosB;

        _isAnimating = false;

        onTileSwapped?.Invoke();

        
        if (PhotonNetwork.IsMasterClient)
        {
            CheckSolvedAndAwardPoint();
        }
    }

    private void CheckSolvedAndAwardPoint()
    {
        if (_slots == null) return;
        if (_solved) return;

        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] != i)
            {
                Debug.Log($"[MosaicPuzzle] No resuelto: slot {i} tiene tile {_slots[i]}");
                return;
            }
        }

        _solved = true;
        Debug.Log("[MosaicPuzzle] ========== ¡¡¡PUZZLE RESUELTO!!! ==========");

        photonView.RPC(nameof(RPC_PuzzleSolved), RpcTarget.All);

        // Dar el punto - ScoreManager se encarga de verificar victoria
        if (ScoreManager.Instance != null)
        {
            string team = string.IsNullOrEmpty(teamFilter) ? GetLocalTeam() : teamFilter;
            if (!string.IsNullOrEmpty(team))
            {
                Debug.Log($"[MosaicPuzzle] Otorgando punto al equipo: {team}");
                ScoreManager.Instance.AddPoint(team);
            }
            else
            {
                Debug.LogError("[MosaicPuzzle] No se pudo determinar el equipo!");
            }
        }
        else
        {
            Debug.LogError("[MosaicPuzzle] ScoreManager.Instance es NULL!");
        }
    }

    [PunRPC]
    private void RPC_PuzzleSolved()
    {
        _solved = true;
        Debug.Log("[MosaicPuzzle] RPC_PuzzleSolved recibido");
        onSolved?.Invoke();
        StartCoroutine(SolvedCelebrationAnimation());
    }

    private IEnumerator SolvedCelebrationAnimation()
    {
        foreach (var tile in tiles)
        {
            if (tile != null)
            {
                StartCoroutine(BounceAnimation(tile.transform));
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    private IEnumerator BounceAnimation(Transform t)
    {
        Vector3 originalPos = t.position;
        float bounceHeight = 0.2f;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float bounce = Mathf.Sin((elapsed / duration) * Mathf.PI) * bounceHeight;
            t.position = originalPos + Vector3.up * bounce;
            yield return null;
        }

        t.position = originalPos;
    }

    private void Shuffle(int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            (array[i], array[rnd]) = (array[rnd], array[i]);
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
            (array[0], array[1]) = (array[1], array[0]);
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

            stream.SendNext(_slots?.Length ?? 0);
            if (_slots != null)
            {
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

            if (!_isAnimating)
            {
                ApplySlotPositions();
            }
        }
    }
}