using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class GoToLobbyOnSolved : MonoBehaviourPun
{
    [SerializeField] private string lobbySceneName = "Lobby";

    // Enganchá este método al UnityEvent "onCorrectCode" de cada TeamNumpadController
    public void GoToLobby()
    {
        // Opción A (recomendada): Master carga y PUN sincroniza al resto.
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && PhotonNetwork.AutomaticallySyncScene)
        {
            PhotonNetwork.LoadLevel(lobbySceneName);
            return;
        }

        // Opción B (fallback): forzar carga por RPC si no tenés AutomaticallySyncScene
        if (photonView != null)
        {
            photonView.RPC(nameof(RPC_LoadSceneAll), RpcTarget.AllBuffered, lobbySceneName);
        }
        else
        {
            // Single player / editor tests
            SceneManager.LoadScene(lobbySceneName);
        }
    }

    [PunRPC]
    private void RPC_LoadSceneAll(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}