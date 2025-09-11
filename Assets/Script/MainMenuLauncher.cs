using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;


public class MainMenuLauncher : MonoBehaviourPunCallbacks
{
    public string gameSceneName;
    public TMP_InputField inputField;
    public Button connectionButton;
    [SerializeField] private byte maxPlayers = 4;

    private const string nicknameKey = "playerNickname";
    private string nickname;

    void Start()
    {
        connectionButton.onClick.AddListener(HandleConnectButton);
        inputField.onValueChanged.AddListener(VerifyName);
        PhotonNetwork.AutomaticallySyncScene = true;

    }

    private void VerifyName(string newName)
    {
        if (inputField.text.Length == 0)
        {
            connectionButton.interactable = false;
        }

        if (inputField.text.Length >= 1 && !connectionButton.interactable)
        {
            connectionButton.interactable = true;
        }

        nickname = newName;
    }

    public void HandleConnectButton()
    {
        PlayerPrefs.SetString(nicknameKey, nickname);

        PhotonNetwork.NickName = nickname.ToUpper();
        print(nickname + " is trying to connect...");

        PhotonNetwork.ConnectUsingSettings();

        connectionButton.interactable = false;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(nickname + " connected to master");
        QuickMatch();


    }



    public void QuickMatch()
    {
        Debug.Log("[Photon] Intentando unirse a una sala aleatoria...");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[Photon] No se encontró ninguna sala disponible ({message}). Creando una nueva...");

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            IsOpen = true,
            IsVisible = true
        };

        PhotonNetwork.CreateRoom(null, roomOptions, null);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[Photon] Entraste a: {PhotonNetwork.CurrentRoom.Name} " +
                  $"({PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers})");

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[Photon] Soy el MasterClient, cargando escena...");
            PhotonNetwork.LoadLevel(gameSceneName); 
        }
    }


}
