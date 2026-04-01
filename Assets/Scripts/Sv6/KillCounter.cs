using UnityEngine;
using UnityEngine.UI;
using TMPro;



public class KillCounter : MonoBehaviour


{
    public TMP_Text killCounterText; // Assign this in the Inspector
    private int killCount = 0;

    // Call this method whenever an enemy is killed
    public void IncrementKillCount()
    {
        killCount++;
        UpdateKillCounterUI();
    }

    // Updates the on-screen kill counter text
    private void UpdateKillCounterUI()
    {
        killCounterText.text = "Kills: " + killCount;
    }
}
