using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class LightReceiverGroup : MonoBehaviour
{
    public enum Mode
    {
        All,     
        Any,        
        Exactly,    
        AtLeast     
    }

    [Header("Receptores que componen el grupo")]
    public LightReceiver[] receivers;

    [Header("Lógica de activación")]
    public Mode mode = Mode.All;
    [Min(1)] public int requiredCount = 2;   
    public bool requireContinuous = true;   
    public bool latchOn = false;             
    public bool allowManualReset = false;   

    [Header("Sincronización (opcional)")]
    public float syncWindowSeconds = 0f;

    [Header("Eventos")]
    public UnityEvent onActivated;
    public UnityEvent onDeactivated;

    private bool _active = false;

    void Update()
    {
        bool shouldBeActive = Evaluate();

        if (latchOn)
        {
            if (!_active && shouldBeActive)
            {
                _active = true;
                onActivated?.Invoke();
            }
            return;
        }

        if (_active != shouldBeActive)
        {
            _active = shouldBeActive;
            if (_active) onActivated?.Invoke();
            else onDeactivated?.Invoke();
        }
    }

    bool Evaluate()
    {
        if (receivers == null || receivers.Length == 0) return false;
        int litCount = 0;
        for (int i = 0; i < receivers.Length; i++)
            if (receivers[i] && receivers[i].isLit) litCount++;

        if (syncWindowSeconds <= 0f)
        {
            switch (mode)
            {
                case Mode.All: return litCount == receivers.Length;
                case Mode.Any: return litCount >= 1;
                case Mode.Exactly: return litCount == Mathf.Clamp(requiredCount, 1, receivers.Length);
                case Mode.AtLeast: return litCount >= Mathf.Clamp(requiredCount, 1, receivers.Length);
            }
        }

        float now = Time.time;

        var times = new System.Collections.Generic.List<float>(receivers.Length);
        for (int i = 0; i < receivers.Length; i++)
        {
            var r = receivers[i];
            if (r && r.isLit) times.Add(r.lastLitTime);
        }

        if (times.Count == 0) return false;

        times.Sort(); 
        int need = mode switch
        {
            Mode.All => receivers.Length,
            Mode.Any => 1,
            Mode.Exactly => Mathf.Clamp(requiredCount, 1, receivers.Length),
            Mode.AtLeast => Mathf.Clamp(requiredCount, 1, receivers.Length),
            _ => receivers.Length
        };

        if (times.Count < need) return false;
        for (int start = 0; start + need - 1 < times.Count; start++)
        {
            int end = start + need - 1;
            float span = times[end] - times[start];
            if (span <= syncWindowSeconds)
            {
                if (mode == Mode.Exactly)
                {
                    bool extraInside = false;
                    if (start - 1 >= 0 && (times[end] - times[start - 1]) <= syncWindowSeconds) extraInside = true;
                    if (end + 1 < times.Count && (times[end + 1] - times[start]) <= syncWindowSeconds) extraInside = true;

                    if (extraInside) continue; 
                }
                return true; 
            }
        }

        return false;
    }

    public void ResetLatch()
    {
        if (!allowManualReset) return;
        if (!_active) return;
        _active = false;
        onDeactivated?.Invoke();
    }
    public bool IsActive => _active;
}
