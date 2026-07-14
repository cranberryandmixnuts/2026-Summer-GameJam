using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyBrain))]
public sealed class EnemyBrainEditor : OdinEditor
{
    private SerializedProperty bodyProperty;
    private SerializedProperty animatorProperty;
    private SerializedProperty healthSourceProperty;
    private SerializedProperty graphProperty;

    protected override void OnEnable()
    {
        base.OnEnable();
        bodyProperty = serializedObject.FindProperty("body");
        animatorProperty = serializedObject.FindProperty("animator");
        healthSourceProperty = serializedObject.FindProperty("healthSource");
        graphProperty = serializedObject.FindProperty("graph");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("참조", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(bodyProperty, new GUIContent("Rigidbody 2D"));
        EditorGUILayout.PropertyField(animatorProperty, new GUIContent("Animator"));
        EditorGUILayout.PropertyField(healthSourceProperty, new GUIContent("Health Source"));

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

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("런타임", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("현재 상태", stateName);
            EditorGUILayout.LabelField("상태 경과 시간", $"{brain.StateElapsedTime:0.000}초");
            EditorGUILayout.LabelField("액션 완료", brain.ActionsComplete ? "예" : "아니오");
            Repaint();
        }

        EditorGUILayout.Space(4f);

        if (GUILayout.Button("행동 그래프 열기", GUILayout.Height(36f))) EnemyBehaviorGraphWindow.Open((EnemyBrain)target);

        serializedObject.ApplyModifiedProperties();
    }
}