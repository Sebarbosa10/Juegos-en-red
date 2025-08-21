using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuLauncher : MonoBehaviourPunCallbacks
{
    public string gameSceneName;
    public TMP_InputField inputField;
    public Button connectionButton;

    private const string nicknameKey = "playerNickname";
    private string nickname;

    void Start()
    {
        connectionButton.onClick.AddListener(HandleConnectButton);
        inputField.onValueChanged.AddListener(VerifyName);
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
        SceneManager.LoadScene(gameSceneName);
       
    }

}
