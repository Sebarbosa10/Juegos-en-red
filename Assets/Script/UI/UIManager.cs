using TMPro;
using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private PlayerController _playerController;








    [Header("Book UI")]
    [SerializeField] private GameObject NoteUI;   
    private bool _isBookOpen = false;
    public bool IsBookOpen => _isBookOpen;

    private PlayerController _bookOpener;
    private PlayerController _noteOpener;




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

   


    public void OpenNote(PlayerController controller)
    {
       

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

    private void SetCursorVisible(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    
}