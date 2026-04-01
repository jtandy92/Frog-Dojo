using UnityEngine;
using System.Collections;
namespace Sv6.Dojo
{
public class MenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public PlayerController player;    // Drag your PlayerController in the Inspector
    public float inputEnableDelay = 1f; // Wait 1 second before enabling player input

    void Start()
    {
        // Show the main menu, pause the game
        mainMenuPanel.SetActive(true);
        Time.timeScale = 0; 
    }

    public void StartGame()
    {
        SoundManager.PlaySound("Music");
        Time.timeScale = 1;
        if (player != null) player.isActive = true;
        mainMenuPanel.SetActive(false);
    }



    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game is quitting");
    }
}
}