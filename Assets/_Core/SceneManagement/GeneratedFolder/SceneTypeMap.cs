using System;
using System.Collections.Generic;

public static class SceneTypeMap
{
    private static readonly string[] SceneNames =
    {
        "",
        "LobbyScene",
    };

    private static readonly string[] ScenePaths =
    {
        "",
        "Assets/_Scenes/LobbyScene.unity",
    };

    private static readonly bool[] EnabledInBuildSettings =
    {
        false,
        false,
    };

    private static readonly Dictionary<string, SceneType> NameToType = new(StringComparer.Ordinal)
    {
        { "LobbyScene", SceneType.LobbyScene },
    };

    public static int TotalCount => SceneNames.Length;
    public static int BuildSceneCount => SceneNames.Length - 1;
    public static string GetName(SceneType sceneType) => SceneNames[(int)sceneType];
    public static string GetPath(SceneType sceneType) => ScenePaths[(int)sceneType];
    public static bool IsEnabledInBuildSettings(SceneType sceneType) => EnabledInBuildSettings[(int)sceneType];
    public static bool TryGetTypeByName(string sceneName, out SceneType sceneType) => NameToType.TryGetValue(sceneName, out sceneType);
}
