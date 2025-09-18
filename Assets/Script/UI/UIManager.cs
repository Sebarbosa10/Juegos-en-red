using TMPro;
using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private PlayerController _playerController;

    public bool IsDialogueActive { get; set; } = false;

    #region Comm Variables

    [Header("Common UI Elements")]
    [SerializeField] private GameObject pauseMenuUI;

    private bool _isGamePaused = false;
    public bool IsGamePaused => _isGamePaused;

    //[SerializeField] private GameObject optionsMenuUI;
    public bool IsInOptionsMenu { get; set; } = false;

    #endregion

    #region Book Variables
    [Header("Book UI")]
    [SerializeField] private GameObject newsPaperUI;
    [SerializeField] private GameObject LetterUI;
    [SerializeField] private GameObject NoteUI;
    private bool _isBookOpen = false;
    public bool IsBookOpen => _isBookOpen;

    private PlayerController _bookOpener;
    private PlayerController _noteOpener;
    #endregion

    #region Clues Variables
    [Header("Clue Note UI")]
    [SerializeField] private GameObject notePanelUI;
    [SerializeField] private TextMeshProUGUI noteTitleText;
    [SerializeField] private TextMeshProUGUI noteBodyText;
    private bool _isClueOpen = false;
    public bool IsClueOpen => _isClueOpen;
    #endregion

    #region Journal Variables
    [Header("Journal")]
    [SerializeField] private GameObject journalUI;
    private bool _journalOpen = false;
    public bool canOpenJournal { get; set; } = true;
    #endregion

    #region Player Dialogue Variables
    [Header("Player Dialogue")]
    [SerializeField] private GameObject playerDialogueUI;
    [SerializeField] private TextMeshProUGUI playerDialogueText;
    [SerializeField] private float dialogueDisplayTime = 2.5f;
    private Coroutine dialogueCoroutine;
    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        _playerController = Object.FindObjectOfType<PlayerController>();
    }

    #region Player Dialogue System

    public void ShowMessage(string message)
    {
        if (dialogueCoroutine != null)
            StopCoroutine(dialogueCoroutine);
        dialogueCoroutine = StartCoroutine(ShowMessageRoutine(message));
    }

    private IEnumerator ShowMessageRoutine(string message)
    {
        playerDialogueText.text = message;
        playerDialogueUI.SetActive(true);
        yield return new WaitForSecondsRealtime(dialogueDisplayTime);
        playerDialogueUI.SetActive(false);
    }

    #endregion

    #region Journal

    public void ToggleJournal()
    {
        if (!canOpenJournal) return;
        if (_isBookOpen || _isClueOpen) return;

        if (_journalOpen) CloseJournal();
        else OpenJournal();
    }

    public bool IsJournalOpen()
    {
        return _journalOpen;
    }

    private void OpenJournal()
    {
        _journalOpen = true;
        journalUI.SetActive(true);
        //Time.timeScale = 0f;

        SetCursorVisible(true);

        if (_playerController != null)
            _playerController.SetCanMove(false);
    }

    private void CloseJournal()
    {
        _journalOpen = false;
        journalUI.SetActive(false);
        //Time.timeScale = 1f;

        SetCursorVisible(false);

        if (_playerController != null)
            _playerController.SetCanMove(true);
    }

    #endregion

    #region Pause Menu

    public void TogglePause()
    {
        if (IsInOptionsMenu)
        {
            ShowPauseMenu();
        }
        else if (_isGamePaused)
        {
            HideAllUI();
        }
        else
        {
            ShowPauseMenu();
        }
    }

    public void ShowPauseMenu()
    {
        IsInOptionsMenu = false;
        ShowUI(pauseMenuUI);
        //HideUI(optionsMenuUI);
        _isGamePaused = true;
        //Time.timeScale = 0f;
        SetCursorVisible(true);
        if (_playerController != null)
            _playerController.SetPaused(true);
    }

    public void ShowOptionsMenu()
    {
        //ShowUI(optionsMenuUI);
        HideUI(pauseMenuUI);
        IsInOptionsMenu = true;
    }

    #endregion

    #region Common UI

    public void OpenBook(PlayerController controller)
    {
        if (_journalOpen || _isClueOpen) return;

        ShowUI(newsPaperUI);
        _isBookOpen = true;
        _bookOpener = controller;

        if (_bookOpener != null)
            _bookOpener.enabled = false;

        SetCursorVisible(true);
    }

    public void CloseBook(PlayerController controller)
    {
        HideUI(newsPaperUI);
        _isBookOpen = false;

        if (_bookOpener != null)
            _bookOpener.enabled = true;

        SetCursorVisible(false);
    }

    public void OpenLetter(PlayerController controller)
    {
        if (_journalOpen || _isClueOpen) return;

        ShowUI(LetterUI);
        _isBookOpen = true;
        _bookOpener = controller;

        if (_bookOpener != null)
            _bookOpener.enabled = false;

        SetCursorVisible(true);
    }

    public void CloseLetter(PlayerController controller)
    {
        HideUI(LetterUI);
        _isBookOpen = false;

        if (_bookOpener != null)
            _bookOpener.enabled = true;

        SetCursorVisible(false);
        GameProgressManager.Instance.SetProgressFlag("Recova_Questions_Unlocked", true);

    }


    public void OpenNote(PlayerController controller)
    {
        if (_journalOpen || _isClueOpen) return;

        ShowUI(NoteUI);
        _isBookOpen = true;
        _bookOpener = controller;

        if (_bookOpener != null)
            _bookOpener.enabled = false;

        SetCursorVisible(true);
    }

    public void CloseNote(PlayerController controller)
    {
        HideUI(NoteUI);
        _isBookOpen = false;

        if (_bookOpener != null)
            _bookOpener.enabled = true;

        SetCursorVisible(false);
        GameProgressManager.Instance.SetProgressFlag("MainZonesUnlocked", true);
    }


    public void ShowUI(GameObject uiPanel)
    {
        if (uiPanel != null)
            uiPanel.SetActive(true);
    }

    public void HideUI(GameObject uiPanel)
    {
        if (uiPanel != null)
            uiPanel.SetActive(false);
    }

    public void HideAllUI()
    {
        // Oculta menús
        HideUI(pauseMenuUI);
        //HideUI(optionsMenuUI);

        if (_isBookOpen)
        {
            if (newsPaperUI != null && newsPaperUI.activeSelf)
                CloseBook(_bookOpener);
            else if (LetterUI != null && LetterUI.activeSelf)
                CloseLetter(_bookOpener);
            else if (NoteUI != null && NoteUI.activeSelf)
                CloseNote(_bookOpener);
            else
                _isBookOpen = false; // fallback de seguridad
        }


        if (_isClueOpen)
            CloseClueNote();

        _isGamePaused = false;
        IsInOptionsMenu = false;

        //Time.timeScale = 1f;
        SetCursorVisible(false);

        if (_playerController != null)
            _playerController.SetPaused(false);
    }

    private void SetCursorVisible(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    #endregion

    #region Clue UI

    public void ShowClueNote(string title, string body, PlayerController opener)
    {
        if (_journalOpen) return;

        if (noteTitleText != null) noteTitleText.text = title;
        if (noteBodyText != null) noteBodyText.text = body;
        if (notePanelUI != null) notePanelUI.SetActive(true);

        _isClueOpen = true;
        _noteOpener = opener;

        if (_noteOpener != null)
            _noteOpener.enabled = false;

        SetCursorVisible(true);
    }

    public void CloseClueNote()
    {
        if (notePanelUI != null)
            notePanelUI.SetActive(false);

        _isClueOpen = false;

        if (_noteOpener != null)
            _noteOpener.enabled = true;

        SetCursorVisible(false);
    }

    #endregion
}
