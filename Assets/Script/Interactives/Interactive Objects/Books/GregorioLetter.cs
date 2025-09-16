using UnityEngine;

public class GregorioLetter : MonoBehaviour, IInteractive
{
    [SerializeField] private PlayerController cameraController;

    public void Interact()
    {
        if (!UIManager.Instance.IsBookOpen)
        {
            OpenLetter();
        }
        else
        {
            CloseLetter();
        }
    }

    public void OpenLetter()
    {
        UIManager.Instance.OpenLetter(cameraController);
    }

    public void CloseLetter()
    {
        UIManager.Instance.CloseLetter(cameraController);
        GameProgressManager.Instance.SetProgressFlag("Recova_Questions_Unlocked", true);


    }
}
