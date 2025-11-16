using System.Text;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class TeamNumpadController : MonoBehaviourPun
{
    [Header("Config")]
    [SerializeField] private string teamFilter = "Blue";
    [SerializeField] private string correctCode = "1234";
    [SerializeField] private int maxLength = 4;
    [SerializeField] private bool lockAfterSolve = true;

    [Header("UI")]
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private string hiddenChar = "•";
    [SerializeField] private bool hideDigits = false;

    [Header("Lobby Spawns (zona de lobby)")]
    [SerializeField] private Transform lobbyBlueSpawn;
    [SerializeField] private Transform lobbyRedSpawn;

    [Header("Eventos")]
    public UnityEvent onDigit;
    public UnityEvent onClear;
    public UnityEvent onBackspace;
    public UnityEvent onSubmit;
    public UnityEvent onCorrectCode;
    public UnityEvent onWrongCode;

    private readonly StringBuilder _buffer = new StringBuilder(8);
    private bool _solved = false;

    private const string TeamKey = "team";
    private const string ReadyKey = "ready";
    private const string MatchStartedKey = "matchStarted";
    private const string SecondRoundKey = "secondRound";

    private void Start()
    {
        RefreshDisplay();
    }

    public bool IsSolved => _solved;

   

    public void RequestDigit(int d)
    {
        if (d < 0 || d > 9) return;
        if (RejectInput()) return;

        string senderTeam = GetLocalTeam();
        photonView.RPC(nameof(RPC_PressDigit), RpcTarget.All, d, senderTeam);
    }

    public void RequestClear()
    {
        if (RejectInput()) return;
        string senderTeam = GetLocalTeam();
        photonView.RPC(nameof(RPC_Clear), RpcTarget.All, senderTeam);
    }

    public void RequestBackspace()
    {
        if (RejectInput()) return;
        string senderTeam = GetLocalTeam();
        photonView.RPC(nameof(RPC_Backspace), RpcTarget.All, senderTeam);
    }

    public void RequestSubmit()
    {
        if (RejectInput()) return;
        string senderTeam = GetLocalTeam();
        photonView.RPC(nameof(RPC_Submit), RpcTarget.All, senderTeam);
    }

    
    [PunRPC]
    private void RPC_PressDigit(int d, string senderTeam, PhotonMessageInfo _mi)
    {
        if (!AcceptsFromTeam(senderTeam)) return;
        if (_solved && lockAfterSolve) return;

        if (_buffer.Length < Mathf.Max(1, maxLength))
        {
            _buffer.Append((char)('0' + d));
            onDigit?.Invoke();
            RefreshDisplay();
        }
    }

    [PunRPC]
    private void RPC_Clear(string senderTeam, PhotonMessageInfo _mi)
    {
        if (!AcceptsFromTeam(senderTeam)) return;
        if (_solved && lockAfterSolve) return;

        _buffer.Clear();
        onClear?.Invoke();
        RefreshDisplay();
    }

    [PunRPC]
    private void RPC_Backspace(string senderTeam, PhotonMessageInfo _mi)
    {
        if (!AcceptsFromTeam(senderTeam)) return;
        if (_solved && lockAfterSolve) return;

        if (_buffer.Length > 0)
        {
            _buffer.Length = _buffer.Length - 1;
            onBackspace?.Invoke();
            RefreshDisplay();
        }
    }

    [PunRPC]
    private void RPC_Submit(string senderTeam, PhotonMessageInfo _mi)
    {
        if (!AcceptsFromTeam(senderTeam)) return;
        if (_solved && lockAfterSolve) return;

        onSubmit?.Invoke();
        bool ok = string.Equals(_buffer.ToString(), correctCode);
        if (ok)
        {
            _solved = true;
            onCorrectCode?.Invoke();

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log($"[Numpad] Código correcto ({correctCode}) por {senderTeam}. Punto + volver a lobby + habilitar segundo puzzle.");

               
                if (ScoreManager.Instance != null)
                    ScoreManager.Instance.AddPoint(senderTeam);

              
                TeleportAllPlayersToLobby();

               
                ResetAllReadyFlags();

                
                var roomProps = new PhotonHashtable
                {
                    { MatchStartedKey, false },
                    { SecondRoundKey, true }
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

                Debug.Log("[Numpad] SetCustomProperties → matchStarted=false, secondRound=true");
            }
        }
        else
        {
            onWrongCode?.Invoke();
            Debug.Log($"[Numpad] Código incorrecto ingresado por {senderTeam}. Reset del buffer.");
            _buffer.Clear();
        }

        RefreshDisplay();
    }

    

    private void TeleportAllPlayersToLobby()
    {
        foreach (var p in PhotonNetwork.PlayerList)
        {
            string team = GetTeamOf(p);
            if (string.IsNullOrEmpty(team)) continue;

            Transform targetSpawn = null;
            if (team == "Blue")
                targetSpawn = lobbyBlueSpawn;
            else if (team == "Red")
                targetSpawn = lobbyRedSpawn;

            if (targetSpawn == null) continue;

            if (p.TagObject is GameObject go)
            {
                go.transform.position = targetSpawn.position;
                go.transform.rotation = targetSpawn.rotation;
            }
        }

        Debug.Log("[Numpad] Todos los jugadores teletransportados a la lobby.");
    }

    private void ResetAllReadyFlags()
    {
        foreach (var p in PhotonNetwork.PlayerList)
        {
            var props = new PhotonHashtable
            {
                { ReadyKey, false }
            };
            p.SetCustomProperties(props);
        }

        Debug.Log("[Numpad] Flags de Ready reseteados a false para todos.");
    }

   

    private string GetTeamOf(Player p)
    {
        if (p?.CustomProperties == null) return "";
        return p.CustomProperties.TryGetValue(TeamKey, out object t) ? (t as string ?? "") : "";
    }

    private bool RejectInput()
    {
        if (!EnabledForLocal()) return true;
        if (_solved && lockAfterSolve) return true;
        return false;
    }

    private bool EnabledForLocal()
    {
        if (string.IsNullOrEmpty(teamFilter)) return true;
        return string.Equals(GetLocalTeam(), teamFilter);
    }

    private bool AcceptsFromTeam(string senderTeam)
    {
        if (string.IsNullOrEmpty(teamFilter)) return true;
        return string.Equals(senderTeam, teamFilter);
    }

    private string GetLocalTeam()
    {
        var p = PhotonNetwork.LocalPlayer;
        if (p?.CustomProperties == null) return "";
        return p.CustomProperties.TryGetValue(TeamKey, out object t) ? (t as string ?? "") : "";
    }

    private void RefreshDisplay()
    {
        if (!displayText) return;

        if (_buffer.Length == 0)
        {
            displayText.text = "----";
            return;
        }

        if (hideDigits)
        {
            displayText.text = new string(hiddenChar[0], _buffer.Length)
                .PadRight(Mathf.Max(1, maxLength), '-');
        }
        else
        {
            string s = _buffer.ToString();
            if (s.Length < maxLength) s = s.PadRight(maxLength, '-');
            displayText.text = s;
        }
    }

    public void SetCorrectCode(string code)
    {
        correctCode = code ?? "";
        _buffer.Clear();
        _solved = false;
        RefreshDisplay();
    }
}
