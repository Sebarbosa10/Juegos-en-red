using UnityEngine;
using System.Collections.Generic;

public class LightReceiverRegister : MonoBehaviour
{
    public static readonly List<LightReceiver> All = new();

    public LightReceiver receiver;

    void Reset() { receiver = GetComponent<LightReceiver>(); }

    void OnEnable()
    {
        if (!receiver) receiver = GetComponent<LightReceiver>();
        if (receiver && !All.Contains(receiver)) All.Add(receiver);
    }

    void OnDisable()
    {
        if (receiver) All.Remove(receiver);
    }
}
