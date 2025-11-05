using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Button joinButton;

    private string _roomName;
    private MainMenuLauncher _launcher;

    public void Setup(RoomInfo info, MainMenuLauncher launcher)
    {
        _launcher = launcher;
        Refresh(info);

        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() =>
        {
            if (_launcher != null)
                _launcher.JoinRoom(_roomName);
        });
    }

    public void Refresh(RoomInfo info)
    {
        _roomName = info.Name;
        if (roomNameText) roomNameText.text = info.Name;
        if (countText) countText.text = $"{info.PlayerCount}/{info.MaxPlayers}";

        bool joinable = info.IsOpen && info.IsVisible && info.PlayerCount < info.MaxPlayers;
        if (joinButton) joinButton.interactable = joinable;
    }
}
