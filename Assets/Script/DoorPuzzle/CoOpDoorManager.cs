using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class CoOpDoorManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [Header("Tiempo de coincidencia (s)")]
    [SerializeField] private double pressWindow = 0.75; // en segundos (PhotonNetwork.Time)

    private const byte EVT_BUTTON_PRESSED = 101;

    // doorId -> team -> lista de (actor, time)
    private readonly Dictionary<string, Dictionary<string, List<(int actor, double time)>>> _presses
        = new Dictionary<string, Dictionary<string, List<(int, double)>>>();

    void OnEnable() => PhotonNetwork.AddCallbackTarget(this);
    void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != EVT_BUTTON_PRESSED) return;

        object[] data = (object[])photonEvent.CustomData;
        string doorId = (string)data[0];
        string team = (string)data[1];
        double t = (double)data[2];
        int actor = (int)data[3];

        if (!_presses.ContainsKey(doorId))
            _presses[doorId] = new Dictionary<string, List<(int, double)>>();

        if (!_presses[doorId].ContainsKey(team))
            _presses[doorId][team] = new List<(int, double)>();

        // Limpia pulsaciones viejas fuera de ventana
        double now = PhotonNetwork.Time;
        _presses[doorId][team].RemoveAll(p => now - p.time > pressWindow);

        // ✅ CORRECCIÓN: registrar si aún NO hay una entrada de este actor
        if (!_presses[doorId][team].Any(p => p.actor == actor))
            _presses[doorId][team].Add((actor, t));

        Debug.Log($"[DoorMgr] {doorId} team={team} presses={_presses[doorId][team].Count} (actors={string.Join(",", _presses[doorId][team].Select(p => p.actor))})");

        // Solo el Master decide abrir
        if (!PhotonNetwork.IsMasterClient) return;

        // ¿Hay 2 actores distintos dentro de ventana?
        var distinctActors = _presses[doorId][team].Select(p => p.actor).Distinct().ToList();
        if (distinctActors.Count >= 2)
        {
            TryOpenDoor(doorId, team);
            _presses[doorId][team].Clear();
        }
    }

    private void TryOpenDoor(string doorId, string team)
    {
        var doors = FindObjectsOfType<CoOpDoor>();
        var door = doors.FirstOrDefault(d => d.GetDoorId() == doorId);

        if (door == null)
        {
            Debug.LogWarning($"[DoorMgr] Door '{doorId}' no encontrada");
            return;
        }

        if (!door.TeamAllowed(team))
        {
            Debug.Log($"[DoorMgr] Door '{doorId}' rechaza team={team}");
            return;
        }

        // Abrir para todos y persistir estado
        door.photonView.RPC("RPC_Open", RpcTarget.All);
        door.MarkOpenRoomProp();
        Debug.Log($"[DoorMgr] OPEN '{doorId}' (team={team})");
    }
}
