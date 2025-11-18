using Photon.Pun;
using UnityEngine;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyReadyButton : MonoBehaviourPunCallbacks
{
    [SerializeField] private KeyCode readyKey = KeyCode.R;

    private const string LobbyCycleKey = "lobbyCycle";
    private const string ReadyCycleKey = "readyCycle";

    void Update()
    {
        if (Input.GetKeyDown(readyKey))
        {
            if (!PhotonNetwork.InRoom) return;

            int lobbyCycle = 0;
            var rp = PhotonNetwork.CurrentRoom.CustomProperties;
            if (rp != null && rp.ContainsKey(LobbyCycleKey))
                lobbyCycle = (int)rp[LobbyCycleKey];

            PhotonNetwork.LocalPlayer.SetCustomProperties(
                new PhotonHashtable { { ReadyCycleKey, lobbyCycle } });

            Debug.Log($"[Ready] {PhotonNetwork.NickName} listo para ciclo {lobbyCycle}");
        }
    }
}
