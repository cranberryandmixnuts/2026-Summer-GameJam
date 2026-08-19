using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneLoader : SingletonBehaviour<SceneLoader, GlobalScope>
{
    private const float FadeDuration = 0.5f;

    [SerializeField]
    private CanvasGroup fadeOverlay;

    private bool isTransitioning;

    protected override void SingletonAwake()
    {
        if (fadeOverlay == null) return;

        fadeOverlay.alpha = 0f;
        fadeOverlay.interactable = false;
        fadeOverlay.blocksRaycasts = false;
    }

    public void LoadScene(string sceneName)
    {
        if (isTransitioning || string.IsNullOrWhiteSpace(sceneName)) return;

        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        isTransitioning = true;
        fadeOverlay.blocksRaycasts = true;

        yield return FadeTo(1f);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        if (loadOperation == null)
        {
            Debug.LogError($"씬을 불러올 수 없습니다: {sceneName}", this);
            yield return FadeTo(0f);
            fadeOverlay.blocksRaycasts = false;
            isTransitioning = false;
            yield break;
        }

        yield return loadOperation;
        yield return FadeTo(0f);

        fadeOverlay.blocksRaycasts = false;
        isTransitioning = false;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = fadeOverlay.alpha;
        float elapsed = 0f;

        while (elapsed < FadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeOverlay.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / FadeDuration);
            yield return null;
        }

        fadeOverlay.alpha = targetAlpha;
    }
}
