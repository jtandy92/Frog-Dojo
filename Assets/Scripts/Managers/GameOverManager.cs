using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup blackScreen;
    [SerializeField] private CanvasGroup gameOverSprite;
    [SerializeField] private CanvasGroup retryButton;
    [SerializeField] private float fadeDuration = 1.5f;

    private bool isGameOver = false;

    private void Start()
    {
        // Ensure the retry button is not interactable at the start of the game
        if (retryButton != null)
        {
            retryButton.interactable = false;
            retryButton.blocksRaycasts = false;
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;

        if (blackScreen == null || gameOverSprite == null || retryButton == null)
        {
            Debug.LogError("GameOverManager: Missing UI elements!");
            return;
        }

        isGameOver = true;
        StartCoroutine(FadeInGameOverSequence());
    }

    private IEnumerator FadeInGameOverSequence()
    {
        // Fade in black screen
        yield return StartCoroutine(FadeCanvasGroup(blackScreen, 0, 1, fadeDuration));

        // Fade in "Game Over" sprite
        yield return StartCoroutine(FadeCanvasGroup(gameOverSprite, 0, 1, fadeDuration));

        // Wait a brief moment, then fade in retry button
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeCanvasGroup(retryButton, 0, 1, fadeDuration));

        // Enable interaction with the retry button now that game is over
        retryButton.interactable = true;
        retryButton.blocksRaycasts = true;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        canvasGroup.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    public void RetryGame()
    {
        // Restart the game by reloading the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
