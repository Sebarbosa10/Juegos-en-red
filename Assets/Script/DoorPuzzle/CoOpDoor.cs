using UnityEngine;
using Photon.Pun;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(PhotonView))]
public class CoOpDoor : MonoBehaviourPunCallbacks
{
    [Header("Identidad")]
    [SerializeField] private string doorId = "Door_A";   
    [Tooltip("Dejar vacío para permitir cualquier equipo. Si no, 'Blue' o 'Red'.")]
    [SerializeField] private string teamRestriction = ""; 

    [Header("Refs")]
    [SerializeField] private DoorMover mover;

    private PhotonView _pv;
    private string RoomKey => $"door:{doorId}:{(string.IsNullOrEmpty(teamRestriction) ? "Any" : teamRestriction)}:open";

    void Awake()
    {
        _pv = GetComponent<PhotonView>();
        if (!mover) mover = GetComponent<DoorMover>();
    }

    void Start()
    {
        
        bool opened = false;
        if (PhotonNetwork.InRoom &&
            PhotonNetwork.CurrentRoom.CustomProperties != null &&
            PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomKey))
        {
            opened = (bool)PhotonNetwork.CurrentRoom.CustomProperties[RoomKey];
        }
        if (opened && mover) mover.Open();
    }

    public bool TeamAllowed(string team)
    {
        if (string.IsNullOrEmpty(teamRestriction)) return true;
        return string.Equals(teamRestriction, team);
    }

    
    [PunRPC]
    private void RPC_Open()
    {
        if (mover) mover.Open();
    }

    public void MarkOpenRoomProp()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        PhotonNetwork.CurrentRoom.SetCustomProperties(
            new PhotonHashtable { { RoomKey, true } });
    }

    
    public string GetDoorId() => doorId;
}
