using UnityEngine;

public interface IEnemyDifficultyInitializable
{
    public void InitializeDifficulty(float difficultyFactor);
}

public static class EnemyDifficultyUtility
{
    public const float MinimumDifficultyFactor = 0.01f;

    public static float ClampFactor(float difficultyFactor) =>
        Mathf.Max(MinimumDifficultyFactor, difficultyFactor);

    public static int ScaleStat(int baseValue, float difficultyFactor) =>
        Mathf.Max(1, Mathf.RoundToInt(baseValue * ClampFactor(difficultyFactor)));
}
