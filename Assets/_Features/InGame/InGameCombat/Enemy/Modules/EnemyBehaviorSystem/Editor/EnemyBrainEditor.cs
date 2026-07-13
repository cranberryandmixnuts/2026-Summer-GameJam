using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyBrain))]
public sealed class EnemyBrainEditor : OdinEditor
{
    private SerializedProperty bodyProperty;
    private SerializedProperty graphProperty;

    protected override void OnEnable()
    {
        base.OnEnable();
        bodyProperty = serializedObject.FindProperty("body");
        graphProperty = serializedObject.FindProperty("graph");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(bodyProperty, new GUIContent("Rigidbody 2D"));

        EditorGUILayout.Space(8f);

        SerializedProperty statesProperty = graphProperty.FindPropertyRelative("states");
        SerializedProperty globalTransitionsProperty = graphProperty.FindPropertyRelative("globalTransitions");
        EditorGUILayout.LabelField("행동 그래프", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("상태", statesProperty.arraySize.ToString());
        EditorGUILayout.LabelField("Any State 전이", globalTransitionsProperty.arraySize.ToString());

        if (Application.isPlaying)
        {
            EnemyBrain brain = (EnemyBrain)target;
            string stateName = brain.CurrentState == null ? "없음" : brain.CurrentState.Name;
            EditorGUILayout.LabelField("현재 상태", stateName);
            Repaint();
        }

        EditorGUILayout.Space(4f);

        if (GUILayout.Button("행동 그래프 열기", GUILayout.Height(36f))) EnemyBehaviorGraphWindow.Open((EnemyBrain)target);

        serializedObject.ApplyModifiedProperties();
    }
}
