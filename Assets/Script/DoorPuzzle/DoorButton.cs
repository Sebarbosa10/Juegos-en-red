using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class DoorButton : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private string doorId = "Door_A";
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [Tooltip("Distancia máxima para poder pulsar")]
    [SerializeField] private float useDistance = 2f;

    private const string TeamKey = "team";
    private const byte EVT_BUTTON_PRESSED = 101;

    void Update()
    {
        if (!PhotonNetwork.InRoom) return;
        if (!Input.GetKeyDown(interactKey)) return;

        
        var me = FindMyLocalPlayer();
        if (me == null) return;
        if (Vector3.Distance(me.transform.position, transform.position) > useDistance) return;

        
        string team = "";
        var lp = PhotonNetwork.LocalPlayer;
        if (lp.CustomProperties != null && lp.CustomProperties.ContainsKey(TeamKey))
            team = lp.CustomProperties[TeamKey] as string;

        
        object[] data = new object[]
        {
            doorId,
            team,
            PhotonNetwork.Time,       
            lp.ActorNumber
        };

        var raise = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // todos la escuchan; el Master decide
        PhotonNetwork.RaiseEvent(EVT_BUTTON_PRESSED, data, raise, SendOptions.SendReliable);

        Debug.Log($"[Button] {lp.NickName} pressed {doorId} ({team})");
    }

    private GameObject FindMyLocalPlayer()
    {
        foreach (var pv in GameObject.FindObjectsOfType<PhotonView>())
            if (pv && pv.IsMine && pv.gameObject.CompareTag("Player"))
                return pv.gameObject;
        return null;
    }
}
