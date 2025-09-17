using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

using PhotonHashtable = ExitGames.Client.Photon.Hashtable;


public class MainMenuLauncher : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public TMP_InputField inputField;
    public Button connectButton;

    [Header("Scenes")]
    [SerializeField] private string lobbySceneName = "Lobby";

    private string nickname;
    private const string nicknameKey = "playerNickname";

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "0.1";

        connectButton.onClick.AddListener(Connect);
        inputField.onValueChanged.AddListener(n =>
        {
            nickname = n;
            connectButton.interactable = !string.IsNullOrWhiteSpace(n);
        });

        if (PlayerPrefs.HasKey(nicknameKey))
        {
            inputField.text = PlayerPrefs.GetString(nicknameKey);
            nickname = inputField.text;
            connectButton.interactable = !string.IsNullOrWhiteSpace(nickname);
        }
    }

    void Connect()
    {
        if (string.IsNullOrWhiteSpace(nickname)) return;

        PlayerPrefs.SetString(nicknameKey, nickname);
        PhotonNetwork.NickName = nickname.ToUpper();

        connectButton.interactable = false;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[MainMenu] Conectado al servidor. Cargando Lobby...");
        PhotonNetwork.LoadLevel(lobbySceneName);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        connectButton.interactable = true;
    }
}

