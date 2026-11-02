using System;
using TMPro;
using UnityEngine;

public sealed class ScoreView : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    public void Render(int score)
    {
        Render(score, false);
    }

    public void Render(int score, bool isHidden)
    {
        if (scoreText == null)
            throw new InvalidOperationException("ScoreView requires score text.");
        scoreText.text = "SCORE  " + (isHidden ? "????" : score.ToString("N0"));
    }
}
