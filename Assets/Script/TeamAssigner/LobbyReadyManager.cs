using System.Linq;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(PhotonView))]
public class LobbyReadyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text readyCountText;
    [SerializeField] private int requiredPlayers = 4;
    [SerializeField] private string[] sceneOrder = new[] { "Egypt", "Greece" };

    private const string LobbyCycleKey = "lobbyCycle";
    private const string ReadyCycleKey = "readyCycle";
    private const string TeamKey = "team";
    private const string SpawnedKey = "spawned";
    private const string Blue = "Blue";
    private const string Red = "Red";

    private bool _starting;

    void Awake() { PhotonNetwork.AutomaticallySyncScene = true; }

    void Start()
    {
        EnsureLobbyCycleExists();
        UpdateReadyUI();
        Evaluate();
    }

    private void EnsureLobbyCycleExists()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        var rp = PhotonNetwork.CurrentRoom.CustomProperties;
        if (rp == null || !rp.ContainsKey(LobbyCycleKey))
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new PhotonHashtable { { LobbyCycleKey, 0 } });
            Debug.Log("[Lobby] lobbyCycle inicializado a 0");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) { UpdateReadyUI(); Evaluate(); }
    public override void OnPlayerLeftRoom(Player otherPlayer) { _starting = false; UpdateReadyUI(); Evaluate(); }

    public override void OnPlayerPropertiesUpdate(Player target, PhotonHashtable changedProps)
    {
        if (changedProps == null) return;
        if (changedProps.ContainsKey(ReadyCycleKey)) { UpdateReadyUI(); Evaluate(); }
    }

    private int CurrentLobbyCycle()
    {
        var rp = PhotonNetwork.CurrentRoom.CustomProperties;
        if (rp != null && rp.ContainsKey(LobbyCycleKey)) return (int)rp[LobbyCycleKey];
        return 0;
    }

    private void UpdateReadyUI()
    {
        if (!readyCountText) return;
        int cycle = CurrentLobbyCycle();
        var players = PhotonNetwork.PlayerList;
        int ready = players.Count(p => p.CustomProperties != null &&
                                       p.CustomProperties.ContainsKey(ReadyCycleKey) &&
                                       (int)p.CustomProperties[ReadyCycleKey] == cycle);
        readyCountText.text = $"Ready: {ready}/{requiredPlayers} (ciclo {cycle})";
    }

    private void Evaluate()
    {
        if (!PhotonNetwork.IsMasterClient || _starting) return;

        int cycle = CurrentLobbyCycle();
        var players = PhotonNetwork.PlayerList;

        if (players.Length != requiredPlayers) return;

        bool allReadyThisCycle = players.All(p =>
            p.CustomProperties != null &&
            p.CustomProperties.ContainsKey(ReadyCycleKey) &&
            (int)p.CustomProperties[ReadyCycleKey] == cycle);

        if (!allReadyThisCycle) return;

        StartCoroutine(StartMatch(cycle));
    }

    private IEnumerator StartMatch(int cycle)
    {
        _starting = true;

        // Asignar equipos 2/2 si faltan
        var ordered = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToArray();
        int blue = 0, red = 0;
        foreach (var p in ordered)
        {
            string team = (p.CustomProperties != null && p.CustomProperties.ContainsKey(TeamKey))
                          ? (string)p.CustomProperties[TeamKey] : null;
            if (string.IsNullOrEmpty(team))
            {
                team = (blue < requiredPlayers / 2) ? Blue : Red;
                photonView.RPC(nameof(RPC_SetTeam), p, team);
            }
            if (team == Blue) blue++; else if (team == Red) red++;
        }

        // Limpiar flag de spawned
        foreach (var p in PhotonNetwork.PlayerList)
            photonView.RPC(nameof(RPC_ClearSpawnFlag), p);

        yield return new WaitForSeconds(0.2f);

        // Elegir escena: sceneOrder[lobbyCycle % sceneOrder.Length]
        if (sceneOrder == null || sceneOrder.Length == 0)
        {
            Debug.LogError("[Lobby] sceneOrder vacío");
            _starting = false;
            yield break;
        }
        string sceneToLoad = sceneOrder[cycle % sceneOrder.Length];
        Debug.Log($"[Lobby] Arrancando ciclo {cycle} → escena '{sceneToLoad}'");
        PhotonNetwork.LoadLevel(sceneToLoad);
    }

    [PunRPC]
    private void RPC_SetTeam(string team)
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new PhotonHashtable { { TeamKey, team } });
    }

    [PunRPC]
    private void RPC_ClearSpawnFlag()
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new PhotonHashtable { { SpawnedKey, false } });
    }
}
