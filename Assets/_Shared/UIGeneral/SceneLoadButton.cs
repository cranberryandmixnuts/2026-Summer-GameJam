using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneLoadButton : MonoBehaviour
{
    [SerializeField]
    private bool reloadCurrentScene;

    [HideIf(nameof(reloadCurrentScene))]
    [ValidateInput(nameof(IsValidSceneName), "씬 이름을 입력해야 합니다!")]
    [SerializeField]
    private string sceneName;

    private bool loaded;

    public void Load()
    {
        if (loaded) return;

        string targetSceneName = reloadCurrentScene
            ? SceneManager.GetActiveScene().name
            : sceneName;

        if (string.IsNullOrWhiteSpace(targetSceneName)) return;

        loaded = true;
        SceneManager.LoadScene(targetSceneName);
    }

    private bool IsValidSceneName(string value) =>
        reloadCurrentScene || !string.IsNullOrWhiteSpace(value);
}