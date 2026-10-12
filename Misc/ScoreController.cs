using System;
using TMPro;
using UnityEngine;

public class ScoreController : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI deathscoreText;
    public TextMeshProUGUI highScore;
    public TextMeshProUGUI newhighScore;
    public GameUI gameUI;
    public PlatformSpawner spawner;
    public TMP_Text playerHighScoreText;

    public int score;
    public event Action<int> OnScroreChanged;

    public void AddScore(int points)
    {
        score += points;
        scoreText.text = "Score:" + score.ToString();
        OnScroreChanged?.Invoke(score);
        HighScore();
    }

    public void ResetScore()
    {
        score = 0;
        scoreText.text = "Score:" + score.ToString();
    }
    public void DeathScore()
    {

        deathscoreText.text = "Final Score:" + score.ToString();
        highScore.text = $"HighScore:{PlayerPrefs.GetInt("HighScore", 0)}";
        deathscoreText.enabled = true;
    }


    public void HighScore()
    {
        if (score > PlayerPrefs.GetInt("HighScore", 0))
        {

            gameUI.NewHighScore();
            newhighScore.text = $"New HighScore:{score}";
            PlayerPrefs.SetInt("HighScore", score);
            highScore.text = $"HighScore:{score}";
            PlayerPrefs.Save(); // Ensure the high score is saved
            UpdateHighScore();
        }
    }
    public void UpdateHighScore()
    {
        highScore.text = $"HighScore:{PlayerPrefs.GetInt("HighScore", 0)}" ;
    }
}
