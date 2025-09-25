using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumpadKey : MonoBehaviour, IInteractive
{
    public enum KeyType { Digit, Clear, Backspace, Submit }

    [Header("Key")]
    public KeyType type = KeyType.Digit;
    [Range(0, 9)] public int digitValue = 0;

    [Header("Target")]
    [Tooltip("Arrastrá el TeamNumpadController de ESTE keypad/equipo")]
    public TeamNumpadController controller;

    public void Interact()
    {
        if (!controller) return;

        var anim = GetComponent<AnimatedInteractable>();
        if (anim != null) anim.Interact();

        switch (type)
        {
            case KeyType.Digit: controller.RequestDigit(digitValue); break;
            case KeyType.Clear: controller.RequestClear(); break;
            case KeyType.Backspace: controller.RequestBackspace(); break;
            case KeyType.Submit: controller.RequestSubmit(); break;
        }
    }
}
