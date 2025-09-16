using UnityEngine;

public class PlayerView : MonoBehaviour
{
    public Camera playerCamera;
    public GameObject handIcon;


    public void Move(Vector3 movement)
    {
        transform.Translate(movement);
    }

    public void Rotate(float mouseX)
    {
        transform.Rotate(Vector3.up * mouseX);
    }

    public void RotateCamera(float xRotation)
    {
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    public void ShowHandIcon(bool show)
    {
        handIcon.SetActive(show);
    }
}
