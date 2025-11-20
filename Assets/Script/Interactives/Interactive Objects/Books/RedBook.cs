using TMPro;
using UnityEngine;
public class RedBook : MonoBehaviour, IInteractive
{
   
    [SerializeField] private GameObject notePanel;  

    
    private bool _isOpen = false;
    private CursorLockMode _prevLockMode;
    private bool _prevCursorVisible;

    private void Awake()
    {
        if (notePanel != null) notePanel.SetActive(false);
    }

    private void Update()
    {
        if (_isOpen && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    public void Interact()
    {
        if (_isOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (notePanel == null) return;

        notePanel.SetActive(true);
        _isOpen = true;

        _prevLockMode = Cursor.lockState;
        _prevCursorVisible = Cursor.visible;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Close()
    {
        if (notePanel == null) return;

        notePanel.SetActive(false);
        _isOpen = false;
        Cursor.visible = _prevCursorVisible;
        Cursor.lockState = _prevLockMode;
    }
}

