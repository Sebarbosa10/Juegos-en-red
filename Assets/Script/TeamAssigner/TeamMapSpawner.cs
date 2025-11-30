using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

using PUNPlayer = Photon.Realtime.Player;

public class TeamMapSpawner : MonoBehaviourPunCallbacks
{
    public static TeamMapSpawner Instance;

    [Header("Puzzle 1 Spawns")]
    [SerializeField] private Transform[] puzzle1BlueSpawns;
    [SerializeField] private Transform[] puzzle1RedSpawns;

    [Header("Puzzle 2 Spawns")]
    [SerializeField] private Transform[] puzzle2BlueSpawns;
    [SerializeField] private Transform[] puzzle2RedSpawns;

    [Header("Puzzle 3 Spawns")]
    [SerializeField] private Transform[] puzzle3BlueSpawns;
    [SerializeField] private Transform[] puzzle3RedSpawns;

    private const string TeamKey = "team";
    private const string TeamBlue = "Blue";
    private const string TeamRed = "Red";

    private const string MatchStartedKey = "matchStarted";
    private const string RoundIndexKey = "roundIndex";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Nada especial aquí, esperamos a que la room diga "matchStarted = true"
    }

    public override void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
    {
        if (propertiesThatChanged == null) return;

        if (propertiesThatChanged.ContainsKey(MatchStartedKey))
        {
            bool matchStarted = (bool)propertiesThatChanged[MatchStartedKey];

            if (matchStarted)
            {
                Debug.Log("[TeamMapSpawner] matchStarted = true → respawnear jugador local en puzzle según roundIndex.");
                ForceRespawnAtTeamZone();
            }
        }
    }

    /// <summary>
    /// Lo llamas, por ejemplo, desde un menú de pausa para recolocar al player en su spawn actual.
    /// </summary>
    public void RespawnLocalPlayerFromPause()
    {
        ForceRespawnAtTeamZone();
    }

    private void ForceRespawnAtTeamZone()
    {
        string myTeam = GetMyTeam();
        if (string.IsNullOrEmpty(myTeam))
        {
            Debug.LogWarning("[TeamMapSpawner] No tengo team asignado todavía, no puedo respawnear.");
            return;
        }

        int roundIndex = GetRoundIndex();
        Transform spawn = PickSpawnFor(PhotonNetwork.LocalPlayer, myTeam, roundIndex);
        if (spawn == null)
        {
            Debug.LogWarning($"[TeamMapSpawner] No encontré spawn para team={myTeam}, roundIndex={roundIndex}");
            return;
        }

        if (PhotonNetwork.LocalPlayer.TagObject is GameObject myPlayer)
        {
            myPlayer.transform.position = spawn.position;
            myPlayer.transform.rotation = spawn.rotation;
            Debug.Log($"[TeamMapSpawner] {PhotonNetwork.NickName} ({myTeam}) respawneado en round={roundIndex}, pos={spawn.position}");
        }
        else
        {
            Debug.LogWarning("[TeamMapSpawner] LocalPlayer.TagObject no es un GameObject. ¿Te acordaste de asignarlo cuando instanciaste el Player?");
        }
    }

    private int GetRoundIndex()
    {
        var roomProps = PhotonNetwork.CurrentRoom?.CustomProperties;
        if (roomProps != null && roomProps.ContainsKey(RoundIndexKey))
        {
            object value = roomProps[RoundIndexKey];
            if (value is int ri)
                return ri;
            if (int.TryParse(value.ToString(), out int parsed))
                return parsed;
        }

        // Si por lo que sea no está seteado, asumimos Puzzle 1
        return 1;
    }

    private string GetMyTeam()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties == null) return null;
        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(TeamKey)) return null;
        return PhotonNetwork.LocalPlayer.CustomProperties[TeamKey] as string;
    }

    private Transform PickSpawnFor(PUNPlayer player, string team, int roundIndex)
    {
        Transform[] blueArray = null;
        Transform[] redArray = null;

        switch (roundIndex)
        {
            case 1:
                blueArray = puzzle1BlueSpawns;
                redArray = puzzle1RedSpawns;
                break;
            case 2:
                blueArray = puzzle2BlueSpawns;
                redArray = puzzle2RedSpawns;
                break;
            case 3:
            default:
                blueArray = puzzle3BlueSpawns;
                redArray = puzzle3RedSpawns;
                break;
        }

        var teamPlayers = PhotonNetwork.PlayerList
            .Where(p => p.CustomProperties != null &&
                        p.CustomProperties.ContainsKey(TeamKey) &&
                        (string)p.CustomProperties[TeamKey] == team)
            .OrderBy(p => p.ActorNumber)
            .ToArray();

        int indexInTeam = System.Array.IndexOf(teamPlayers, player);
        if (indexInTeam < 0) indexInTeam = 0;

        Transform[] chosenArray = (team == TeamBlue) ? blueArray : redArray;

        if (chosenArray == null || chosenArray.Length == 0)
        {
            Debug.LogWarning($"[TeamMapSpawner] chosenArray vacío para team={team}, roundIndex={roundIndex}");
            return null;
        }

        int spawnIndex = indexInTeam % chosenArray.Length;
        return chosenArray[spawnIndex];
    }
}
