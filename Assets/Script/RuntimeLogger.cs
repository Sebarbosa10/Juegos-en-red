using UnityEngine;
using TMPro;

public class RuntimeLogger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI logText;
    private string logs = "";

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        logs += logString + "\n";
        if (logs.Length > 1000) // para no explotar memoria
            logs = logs.Substring(logs.Length - 1000);

        if (logText != null)
            logText.text = logs;
    }
}
