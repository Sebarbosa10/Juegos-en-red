using Photon.Pun;
using Photon.Realtime;
using UnityEngine;


public class GameStarter : MonoBehaviourPun, IInteractive
{
    [SerializeField] private string blueScene = "EgyptBlue";
    [SerializeField] private string redScene = "EgyptRed";

    private const string TeamKey = "team";
    private const string TeamBlue = "Blue";
    private const string TeamRed = "Red";

    public void Interact()
    {
        // Solo el host (MasterClient) puede iniciar
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[GameStarter] Solo el host puede iniciar la partida.");
            return;
        }

        Debug.Log("[GameStarter] Host tocó el cubo. Iniciando partida...");
        photonView.RPC(nameof(RPC_StartMatch), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_StartMatch()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(TeamKey, out object teamObj))
        {
            string team = teamObj as string;

            if (team == TeamBlue)
            {
                Debug.Log("[GameStarter] Soy Azul, cargo escena " + blueScene);
                PhotonNetwork.LoadLevel(blueScene);
            }
            else if (team == TeamRed)
            {
                Debug.Log("[GameStarter] Soy Rojo, cargo escena " + redScene);
                PhotonNetwork.LoadLevel(redScene);
            }
        }
        else
        {
            Debug.LogWarning("[GameStarter] Este jugador no tiene equipo asignado.");
        }
    }
}
