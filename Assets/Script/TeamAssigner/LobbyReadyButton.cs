using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyReadyButton : MonoBehaviourPunCallbacks
{
    [SerializeField] private KeyCode readyKey = KeyCode.R;
    private const string ReadyKey = "ready";

    void Update()
    {
        if (Input.GetKeyDown(readyKey))
        {
            var me = PhotonNetwork.LocalPlayer;
            bool current = false;
            if (me.CustomProperties != null && me.CustomProperties.ContainsKey(ReadyKey))
                current = (bool)me.CustomProperties[ReadyKey];

            bool next = !current;
            me.SetCustomProperties(new PhotonHashtable { { ReadyKey, next } });
            Debug.Log($"[Ready] {(next ? "ON" : "OFF")} for {me.NickName} (Actor {me.ActorNumber})");
        }
    }
}
