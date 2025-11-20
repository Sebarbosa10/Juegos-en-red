using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

using PUNPlayer = Photon.Realtime.Player;

public class TeamMapSpawner : MonoBehaviourPunCallbacks
{
    public static TeamMapSpawner Instance;

    
    [SerializeField] private Transform[] puzzle1BlueSpawns;
    [SerializeField] private Transform[] puzzle1RedSpawns;

   
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
           
            return;
        }

        bool secondRound = IsSecondRound();
        

        Transform spawn = PickSpawnFor(PhotonNetwork.LocalPlayer, myTeam, secondRound);
        if (spawn == null)
        {
            
            return;
        }

        if (PhotonNetwork.LocalPlayer.TagObject is GameObject myPlayer)
        {
            myPlayer.transform.position = spawn.position;
            myPlayer.transform.rotation = spawn.rotation;
            
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
            
            return null;
        }

        int spawnIndex = indexInTeam % chosenArray.Length;
        
        return chosenArray[spawnIndex];
    }
}
