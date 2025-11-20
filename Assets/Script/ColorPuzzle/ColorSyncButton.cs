using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ColorSyncButton : MonoBehaviour, IInteractive
{
   
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
