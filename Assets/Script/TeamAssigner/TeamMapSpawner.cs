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

    private const string TeamKey = "team";
    private const string TeamBlue = "Blue";
    private const string TeamRed = "Red";
    private const string MatchStartedKey = "matchStarted";
    private const string SecondRoundKey = "secondRound";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        
    }

    public override void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
    {
        if (propertiesThatChanged == null) return;

        if (propertiesThatChanged.ContainsKey(MatchStartedKey))
        {
            bool matchStarted = (bool)propertiesThatChanged[MatchStartedKey];
            Debug.Log($"[TeamMapSpawner] OnRoomPropertiesUpdate → matchStarted={matchStarted}");

            if (matchStarted)
            {
                ForceRespawnAtTeamZone();
            }
        }
    }

  
    public void RespawnLocalPlayerFromPause()
    {
        ForceRespawnAtTeamZone();
    }

   

    private void ForceRespawnAtTeamZone()
    {
        string myTeam = GetMyTeam();
        if (string.IsNullOrEmpty(myTeam))
        {
            Debug.LogWarning("[TeamMapSpawner] No tengo team asignado todavía.");
            return;
        }

        bool secondRound = IsSecondRound();
        Debug.Log($"[TeamMapSpawner] ForceRespawnAtTeamZone → team={myTeam}, secondRound={secondRound}");

        Transform spawn = PickSpawnFor(PhotonNetwork.LocalPlayer, myTeam, secondRound);
        if (spawn == null)
        {
            Debug.LogWarning($"[TeamMapSpawner] NO encontré spawn para team={myTeam}, secondRound={secondRound}");
            return;
        }

        if (PhotonNetwork.LocalPlayer.TagObject is GameObject myPlayer)
        {
            myPlayer.transform.position = spawn.position;
            myPlayer.transform.rotation = spawn.rotation;
            Debug.Log($"[TeamMapSpawner] {PhotonNetwork.NickName} movido a {myTeam} (secondRound={secondRound}) spawn {spawn.position}");
        }
        else
        {
            Debug.LogWarning("[TeamMapSpawner] LocalPlayer no tiene TagObject asignado.");
        }
    }

    private bool IsSecondRound()
    {
        var roomProps = PhotonNetwork.CurrentRoom?.CustomProperties;
        if (roomProps != null && roomProps.ContainsKey(SecondRoundKey))
        {
            return (bool)roomProps[SecondRoundKey];
        }
        return false;
    }

    private string GetMyTeam()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties == null) return null;
        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(TeamKey)) return null;
        return PhotonNetwork.LocalPlayer.CustomProperties[TeamKey] as string;
    }

    private Transform PickSpawnFor(PUNPlayer player, string team, bool secondRound)
    {
        Transform[] blueArray = secondRound ? puzzle2BlueSpawns : puzzle1BlueSpawns;
        Transform[] redArray = secondRound ? puzzle2RedSpawns : puzzle1RedSpawns;

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
            Debug.LogWarning($"[TeamMapSpawner] chosenArray vacío para team={team}, secondRound={secondRound}");
            return null;
        }

        int spawnIndex = indexInTeam % chosenArray.Length;
        Debug.Log($"[TeamMapSpawner] PickSpawnFor → team={team}, secondRound={secondRound}, spawnIndex={spawnIndex}, spawnName={chosenArray[spawnIndex].name}");

        return chosenArray[spawnIndex];
    }
}
