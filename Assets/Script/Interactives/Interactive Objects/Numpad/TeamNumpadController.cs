using System.Text;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

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

    [SerializeField] private string progressFlagOnSolved = "Blue_Numpad_Solved";

    [Header("Eventos")]
    public UnityEvent onDigit;
    public UnityEvent onClear;
    public UnityEvent onBackspace;
    public UnityEvent onSubmit;
    public UnityEvent onCorrectCode;
    public UnityEvent onWrongCode;

    private readonly StringBuilder _buffer = new StringBuilder(8);
    private bool _solved = false;

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
                Debug.Log($"[Numpad] Código correcto ingresado por {senderTeam}. Sumando punto solo en el Master.");
                ScoreManager.Instance.AddPoint(senderTeam); // 
            }

            if (!string.IsNullOrEmpty(progressFlagOnSolved) && GameProgressManager.Instance != null)
                GameProgressManager.Instance.SetProgressFlag(progressFlagOnSolved, true);
        }

        else
        {
            onWrongCode?.Invoke();
            Debug.Log($"[Numpad] Código incorrecto ingresado por {senderTeam}. Reset del buffer.");
            _buffer.Clear();
        }
        RefreshDisplay();
    }

    private bool RejectInput()
    {
        if (!_enabledForLocal()) return true;
        if (_solved && lockAfterSolve) return true;
        return false;
    }

    private bool _enabledForLocal()
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
        return p.CustomProperties.TryGetValue("team", out object t) ? (t as string ?? "") : "";
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
            displayText.text = new string(hiddenChar[0], _buffer.Length).PadRight(Mathf.Max(1, maxLength), '-');
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
