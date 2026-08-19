using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PooledObject))]
[CanEditMultipleObjects]
internal sealed class PooledObjectEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        DrawRootValidation();

        if (GUILayout.Button("Rebuild Pool Registry"))
            PoolRegistryBuilder.RebuildFromMenu();
    }

    private void DrawRootValidation()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            var pooledObject = (PooledObject)targets[i];
            if (pooledObject == null)
                continue;

            GameObject outermostRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(
                pooledObject.gameObject);

            if (outermostRoot != null && outermostRoot != pooledObject.gameObject)
            {
                EditorGUILayout.HelpBox(
                    $"'{pooledObject.name}' is not the root of its outermost prefab instance. " +
                    "Attach PooledObject to the prefab root that will be instantiated.",
                    MessageType.Warning);
                return;
            }

            if (PrefabUtility.IsPartOfPrefabAsset(pooledObject) &&
                pooledObject.transform.parent != null)
            {
                EditorGUILayout.HelpBox(
                    "PooledObject should be attached to a prefab root, not an arbitrary child.",
                    MessageType.Warning);
                return;
            }
        }
    }
}
