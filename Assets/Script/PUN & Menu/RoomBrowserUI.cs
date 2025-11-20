using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomBrowserUI : MonoBehaviourPunCallbacks
{
    
    [SerializeField] private Transform content;       
    [SerializeField] private GameObject itemPrefab;   

    
    [SerializeField] private MainMenuLauncher launcher;

    private readonly Dictionary<string, RoomInfo> _cache = new Dictionary<string, RoomInfo>();
    private readonly Dictionary<string, RoomListItem> _items = new Dictionary<string, RoomListItem>();

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        
        foreach (var info in roomList)
        {
            if (info.RemovedFromList || info.MaxPlayers == 0)
            {
                _cache.Remove(info.Name);
            }
            else
            {
                _cache[info.Name] = info;
            }
        }

        RebuildUI();
    }

    private void RebuildUI()
    {
       
        var toRemove = new List<string>();
        foreach (var kv in _items)
            if (!_cache.ContainsKey(kv.Key))
                toRemove.Add(kv.Key);
        foreach (var name in toRemove)
        {
            Destroy(_items[name].gameObject);
            _items.Remove(name);
        }

        
        foreach (var kv in _cache)
        {
            var info = kv.Value;
            if (!_items.ContainsKey(info.Name))
            {
                var go = Instantiate(itemPrefab, content);
                var item = go.GetComponent<RoomListItem>();
                item.Setup(info, launcher);

               
                _items[info.Name] = item;
            }
            else
            {
                _items[info.Name].Refresh(info);
            }
        }
    }
}
