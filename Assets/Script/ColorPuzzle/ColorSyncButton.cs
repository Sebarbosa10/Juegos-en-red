using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ColorSyncButton : MonoBehaviour, IInteractive
{
    // 0 = Blue, 1 = Yellow, 2 = Red
    [SerializeField] private int colorIndex = 0;

    public void Interact()
    {
        if (DisconnectPauseManager.IsPaused) return;

        if (TeamColorSyncPuzzle.Instance != null)
        {
            TeamColorSyncPuzzle.Instance.RegisterLocalPress(colorIndex);
        }
    }
}
