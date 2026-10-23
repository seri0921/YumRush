using System;
using UnityEngine;

public sealed class TimerView : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text timerText;
    private double displayedSeconds = -1d;

    public void Render(double remainingSeconds)
    {
        if (timerText == null)
            throw new InvalidOperationException("TimerView requires timer text.");
        if (double.IsNaN(remainingSeconds) || double.IsInfinity(remainingSeconds) || remainingSeconds < 0d)
            throw new ArgumentOutOfRangeException(nameof(remainingSeconds));

        double seconds = Math.Ceiling(remainingSeconds);
        if (seconds == displayedSeconds)
            return;

        displayedSeconds = seconds;
        timerText.text = $"{Math.Floor(seconds / 60d):00}:{seconds % 60d:00}";
    }
}
