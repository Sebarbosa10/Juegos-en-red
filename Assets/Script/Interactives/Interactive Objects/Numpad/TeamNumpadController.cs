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
    [Tooltip("Blue o Red (de CustomProperties['team']). Si queda vacío, acepta de todos.")]
    [SerializeField] private string teamFilter = "Blue";

    [Tooltip("Código correcto de 4 dígitos")]
    [SerializeField] private string correctCode = "1234";

    [Tooltip("Longitud máxima de entrada")]
    [SerializeField] private int maxLength = 4;

    [Tooltip("Bloquear input luego de resolver")]
    [SerializeField] private bool lockAfterSolve = true;

    [Header("UI")]
    [SerializeField] private TMP_Text displayText;          // opcional (pantalla)
    [SerializeField] private string hiddenChar = "•";       // si querés ocultar dígitos
    [SerializeField] private bool hideDigits = false;

    [Header("Progreso (opcional)")]
    [Tooltip("Flag a setear en tu GameProgressManager al resolver")]
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

    // ---- API local (la llaman los botones) ----------------------------

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

    // ---- RPCs ---------------------------------------------------------

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

            if (!string.IsNullOrEmpty(progressFlagOnSolved) && GameProgressManager.Instance != null)
                GameProgressManager.Instance.SetProgressFlag(progressFlagOnSolved, true);

            if (lockAfterSolve == false)
            {
                // si no se bloquea, dejamos buffer lleno para que quede visible
            }
        }
        else
        {
            onWrongCode?.Invoke();
            // limpiar tras error (opcional):
            _buffer.Clear();
        }
        RefreshDisplay();
    }

    // ---- Helpers ------------------------------------------------------

    private bool RejectInput()
    {
        if (!_enabledForLocal()) return true;
        if (_solved && lockAfterSolve) return true;
        return false;
    }

    private bool _enabledForLocal()
    {
        // permite que cualquiera apriete si teamFilter vacío
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
            // mostrar dígitos a izquierda y rellenar con -
            string s = _buffer.ToString();
            if (s.Length < maxLength) s = s.PadRight(maxLength, '-');
            displayText.text = s;
        }
    }

    // Utilidad si querés setear el código por script
    public void SetCorrectCode(string code)
    {
        correctCode = code ?? "";
        _buffer.Clear();
        _solved = false;
        RefreshDisplay();
    }
}
