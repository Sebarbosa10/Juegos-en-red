using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyReadyButton : MonoBehaviourPunCallbacks
{
   
    [SerializeField] private Button readyButton;
    [SerializeField] private GameObject readyIndicator;
    [SerializeField] private GameObject readyPanel; 

    
    [SerializeField] private KeyCode readyKey = KeyCode.R;

    private const string ReadyKey = "ready";
    private const string MatchStartedKey = "matchStarted";

    private bool isReady = false;

    void Start()
    {
        if (readyButton != null)
            readyButton.onClick.AddListener(SetReady);

        if (PhotonNetwork.InRoom)
        {
            
            var props = new PhotonHashtable { { ReadyKey, false } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            isReady = false;

            
            CheckMatchState();
        }
        else if (PhotonNetwork.LocalPlayer.CustomProperties != null &&
                 PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(ReadyKey))
        {
            isReady = (bool)PhotonNetwork.LocalPlayer.CustomProperties[ReadyKey];
        }

        RefreshUI();
    }

    void Update()
    {
        if (!PhotonNetwork.InRoom) return;
        if (isReady) return;
        if (!IsReadyPanelVisible()) return; 

        if (Input.GetKeyDown(readyKey))
        {
            SetReady();
        }
    }

    private void SetReady()
    {
        
        if (IsMatchStarted()) return;

        var props = new PhotonHashtable { { ReadyKey, true } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        isReady = true;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (readyButton != null) readyButton.interactable = !isReady;
        if (readyIndicator != null) readyIndicator.SetActive(isReady);
    }

   
    private void CheckMatchState()
    {
        bool matchStarted = IsMatchStarted();
        SetReadyPanelVisible(!matchStarted);
    }

  
    private void SetReadyPanelVisible(bool visible)
    {
        if (readyPanel != null)
        {
            readyPanel.SetActive(visible);
        }

        
        if (readyButton != null && readyPanel == null)
        {
            readyButton.gameObject.SetActive(visible);
        }

        Debug.Log($"[LobbyReadyButton] Panel Ready visible: {visible}");
    }

    private bool IsReadyPanelVisible()
    {
        if (readyPanel != null)
            return readyPanel.activeSelf;

        if (readyButton != null)
            return readyButton.gameObject.activeSelf;

        return true;
    }

  
    private bool IsMatchStarted()
    {
        if (!PhotonNetwork.InRoom) return false;

        var roomProps = PhotonNetwork.CurrentRoom?.CustomProperties;
        if (roomProps != null && roomProps.ContainsKey(MatchStartedKey))
        {
            return (bool)roomProps[MatchStartedKey];
        }

        return false;
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player target, PhotonHashtable changedProps)
    {
        if (changedProps == null) return;
        if (!changedProps.ContainsKey(ReadyKey)) return;

        if (target.IsLocal)
        {
            isReady = (bool)changedProps[ReadyKey];
            RefreshUI();
        }
    }

    public override void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
    {
        if (propertiesThatChanged == null) return;

        
        if (propertiesThatChanged.ContainsKey(MatchStartedKey))
        {
            bool matchStarted = (bool)propertiesThatChanged[MatchStartedKey];

            if (matchStarted)
            {
                
                SetReadyPanelVisible(false);
                Debug.Log("[LobbyReadyButton] Partida iniciada, ocultando Ready");
            }
            else
            {
                
                isReady = false;
                RefreshUI();
                SetReadyPanelVisible(true);
                Debug.Log("[LobbyReadyButton] Volvieron al lobby, mostrando Ready");
            }
        }
    }
}
