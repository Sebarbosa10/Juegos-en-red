using UnityEngine;
using UnityEngine.Events;

public class WireNumpadsToLobby : MonoBehaviour
{
    [SerializeField] private TeamNumpadController[] numpads;
    [SerializeField] private GoToLobbyOnSolved goToLobby;

    private void Awake()
    {
        if (!goToLobby) goToLobby = GetComponent<GoToLobbyOnSolved>();
        if (goToLobby == null) return;

        foreach (var pad in numpads)
            if (pad) pad.onCorrectCode.AddListener(goToLobby.GoToLobby);
    }
}

