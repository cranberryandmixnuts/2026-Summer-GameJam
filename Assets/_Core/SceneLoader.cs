using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;

public sealed class SceneLoader : SingletonBehaviour<SceneLoader, GlobalScope>
{
    [Header("Fade UI")]
    [SerializeField, Required] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;

    public bool IsTransitioning { get; private set; }

    private Tween fadeTween;
    private Tween pendingRequestTween;
    private string pendingSceneName;

    private void Start()
    {
        Color imageColor = fadeImage.color;
        imageColor.a = 0f;
        fadeImage.color = imageColor;

        fadeImage.gameObject.SetActive(false);
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"'{sceneName}'은 유효하지 않습니다.");
            return;
        }

        if (IsTransitioning)
        {
            ReserveSceneLoad(sceneName);
            return;
        }

        StartCoroutine(LoadSceneSequence(sceneName));
    }

    private void ReserveSceneLoad(string sceneName)
    {
        pendingSceneName = sceneName;

        if (pendingRequestTween != null && pendingRequestTween.IsActive())
            pendingRequestTween.Kill(false);

        float delay = GetRemainingFadeTime();

        pendingRequestTween = DOVirtual
            .DelayedCall(delay, TryExecutePending, true)
            .SetUpdate(true);
    }

    private void TryExecutePending()
    {
        if (IsTransitioning) return;
        if (string.IsNullOrEmpty(pendingSceneName)) return;

        string nextSceneName = pendingSceneName;
        pendingSceneName = null;

        LoadScene(nextSceneName);
    }

    private IEnumerator LoadSceneSequence(string sceneName)
    {
        IsTransitioning = true;
        fadeImage.gameObject.SetActive(true);

        yield return FadeTo(1f).WaitForCompletion();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        yield return new WaitUntil(() => asyncLoad.isDone);

        yield return FadeTo(0f).WaitForCompletion();

        fadeImage.gameObject.SetActive(false);
        IsTransitioning = false;

        if (!string.IsNullOrEmpty(pendingSceneName))
        {
            string nextSceneName = pendingSceneName;
            pendingSceneName = null;

            LoadScene(nextSceneName);
        }
    }

    private Tween FadeTo(float targetAlpha)
    {
        if (fadeTween != null && fadeTween.IsActive())
            fadeTween.Kill(false);

        fadeTween = fadeImage
            .DOFade(targetAlpha, fadeDuration)
            .SetEase(Ease.Linear)
            .SetUpdate(true);

        return fadeTween;
    }

    private float GetRemainingFadeTime()
    {
        if (fadeTween == null) return 0f;
        if (!fadeTween.IsActive()) return 0f;
        if (!fadeTween.IsPlaying()) return 0f;

        float remaining = fadeTween.Duration(false) - fadeTween.Elapsed(false);

        if (remaining < 0f)
            remaining = 0f;

        return remaining;
    }

    public string GetCurrentSceneName() => SceneManager.GetActiveScene().name;
}