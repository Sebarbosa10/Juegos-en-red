using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RedBook : MonoBehaviour, IInteractive
{
    [SerializeField] private PlayerController cameraController;

    public void Interact()
    {
        if (!UIManager.Instance.IsBookOpen)
        {
            OpenBook();
        }
        else
        {
            CloseBook();
        }
    }

    public void OpenBook()
    {
        UIManager.Instance.OpenBook(cameraController);
    }

    public void CloseBook()
    {
        UIManager.Instance.CloseBook(cameraController);
    }
}
