using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyReadyButton : MonoBehaviourPunCallbacks
{
    [SerializeField] private KeyCode readyKey = KeyCode.R;

    private const string ReadyCycleKey = "readyCycle";
    private const string LobbyCycleKey = "lobbyCycle";

    void Update()
    {
        if (Input.GetKeyDown(readyKey))
        {
            if (!PhotonNetwork.InRoom) return;

            int lobbyCycle = 0;
            var rp = PhotonNetwork.CurrentRoom.CustomProperties;
            if (rp != null && rp.ContainsKey(LobbyCycleKey))
                lobbyCycle = (int)rp[LobbyCycleKey];

            // ✅ Marco listo para ESTE ciclo
            PhotonNetwork.LocalPlayer.SetCustomProperties(
                new PhotonHashtable { { ReadyCycleKey, lobbyCycle } });

            Debug.Log($"[Ready] {PhotonNetwork.NickName} listo para ciclo {lobbyCycle}");
        }
    }
}
