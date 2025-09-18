using UnityEngine;
public class RedBook : MonoBehaviour, IInteractive
{
    [SerializeField] private PlayerController cameraController;

    public void Interact()
    {
        if (!UIManager.Instance.IsBookOpen)
            UIManager.Instance.OpenBook(cameraController);   
        else
            UIManager.Instance.CloseBook(cameraController);  
    }
}

