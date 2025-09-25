using UnityEngine;
using TMPro;

public class ScoreBoard : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreUpdated += UpdateScore;
        }

        UpdateScore(0, 0);
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreUpdated -= UpdateScore;
        }
    }

    private void UpdateScore(int blue, int red)
    {
        scoreText.text = $"Blue: {blue}   -   Red: {red}";
    }
}
