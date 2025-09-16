using System.Collections.Generic;
using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    private HashSet<string> _activeProgressFlags = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetProgressFlag(string flag, bool value)
    {
        if (value)
        {
            _activeProgressFlags.Add(flag);
            Debug.Log($"Progreso agregado: {flag}");
        }
        else
        {
            _activeProgressFlags.Remove(flag);
            Debug.Log($"Progreso removido: {flag}");
        }
    }

    public void ResetProgress()
    {
        _activeProgressFlags.Clear();
        Debug.Log("GameProgressManager: Flags de progreso reseteados.");
    }


    public bool HasProgressFlag(string flag)
    {
        return _activeProgressFlags.Contains(flag);
    }

    public List<string> GetActiveProgressFlags()
    {
        return new List<string>(_activeProgressFlags);
    }

    public void SetActiveProgressFlags(List<string> flags)
    {
        _activeProgressFlags = new HashSet<string>(flags);
    }
}