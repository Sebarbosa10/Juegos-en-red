using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class PlayCube : MonoBehaviour, IInteractive
{
    public void Interact()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[PlayCube] Solo el Master puede iniciar la partida.");
            return;
        }

        Debug.Log("[PlayCube] Master inició la partida.");

        // Marca el inicio de la partida en las propiedades de la sala
        var props = new Hashtable { { "matchStarted", true } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }
}

