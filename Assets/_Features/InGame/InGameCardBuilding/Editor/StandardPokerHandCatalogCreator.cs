#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class StandardPokerHandCatalogCreator
{
    [MenuItem("Assets/Create/Cards/Standard Poker Hand Catalog", priority = 300)]
    private static void CreateCatalog()
    {
        string directory = GetSelectedDirectory();
        string catalogPath = AssetDatabase.GenerateUniqueAssetPath(
            $"{directory}/StandardPokerHandCatalog.asset"
        );

        CardHandRuleCatalog catalog = ScriptableObject.CreateInstance<CardHandRuleCatalog>();
        AssetDatabase.CreateAsset(catalog, catalogPath);

        List<CardHandRule> rules = new();

        foreach (PokerHandType handType in Enum.GetValues(typeof(PokerHandType)))
        {
            PokerHandRule rule = ScriptableObject.CreateInstance<PokerHandRule>();
            rule.name = handType.ToString();
            rule.Configure(handType, CardHandSelectionMode.Consecutive);

            AssetDatabase.AddObjectToAsset(rule, catalog);
            EditorUtility.SetDirty(rule);
            rules.Add(rule);
        }

        catalog.ReplaceRules(rules);
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(catalogPath);

        Selection.activeObject = catalog;
        EditorGUIUtility.PingObject(catalog);
    }

    private static string GetSelectedDirectory()
    {
        string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (string.IsNullOrEmpty(selectedPath)) return "Assets";
        if (AssetDatabase.IsValidFolder(selectedPath)) return selectedPath;

        return Path.GetDirectoryName(selectedPath)?.Replace('\\', '/') ?? "Assets";
    }
}
#endif
