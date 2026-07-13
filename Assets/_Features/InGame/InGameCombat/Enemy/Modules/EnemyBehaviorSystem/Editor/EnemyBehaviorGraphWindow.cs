using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public sealed class EnemyBehaviorGraphWindow : EditorWindow
{
    private const float ToolbarHeight = 28f;
    private const float DetailWidth = 410f;
    private const float NodeWidth = 190f;
    private const float NodeHeight = 86f;
    private const float CanvasWidth = 3000f;
    private const float CanvasHeight = 2200f;

    [SerializeField] private EnemyBrain brain;
    [SerializeField] private string selectedStateId;
    [SerializeField] private bool isGlobalSelected;

    private SerializedObject serializedBrain;
    private Vector2 graphScroll;
    private Vector2 detailScroll;
    private GUIStyle nodeStyle;
    private GUIStyle selectedNodeStyle;
    private GUIStyle entryNodeStyle;

    public static void Open(EnemyBrain targetBrain)
    {
        EnemyBehaviorGraphWindow window = GetWindow<EnemyBehaviorGraphWindow>();
        window.titleContent = new GUIContent("Enemy Behavior");
        window.minSize = new Vector2(900f, 560f);
        window.brain = targetBrain;
        window.serializedBrain = new SerializedObject(targetBrain);
        window.Show();
    }

    private void OnEnable()
    {
        titleContent = new GUIContent("Enemy Behavior");

        if (brain != null) serializedBrain = new SerializedObject(brain);
    }

    private void OnSelectionChange()
    {
        if (Selection.activeGameObject == null) return;

        EnemyBrain selectedBrain = Selection.activeGameObject.GetComponent<EnemyBrain>();
        if (selectedBrain == null) return;

        brain = selectedBrain;
        serializedBrain = new SerializedObject(brain);
        selectedStateId = string.Empty;
        isGlobalSelected = false;
        Repaint();
    }

    private void OnGUI()
    {
        if (brain == null)
        {
            EditorGUILayout.HelpBox("EnemyBrain이 있는 오브젝트를 선택하세요.", MessageType.Info);
            return;
        }

        if (serializedBrain == null) serializedBrain = new SerializedObject(brain);

        serializedBrain.Update();
        InitializeStyles();
        DrawToolbar();

        Rect graphRect = new(
            0f,
            ToolbarHeight,
            Mathf.Max(300f, position.width - DetailWidth),
            position.height - ToolbarHeight);
        Rect detailRect = new(
            graphRect.xMax,
            ToolbarHeight,
            position.width - graphRect.width,
            position.height - ToolbarHeight);

        DrawGraph(graphRect);
        DrawDetails(detailRect);
        serializedBrain.ApplyModifiedProperties();
    }

    private void InitializeStyles()
    {
        if (nodeStyle != null) return;

        nodeStyle = new GUIStyle("flow node 0")
        {
            alignment = TextAnchor.UpperLeft,
            padding = new RectOffset(12, 10, 10, 8),
            fontStyle = FontStyle.Bold
        };
        selectedNodeStyle = new GUIStyle("flow node 0 on")
        {
            alignment = TextAnchor.UpperLeft,
            padding = new RectOffset(12, 10, 10, 8),
            fontStyle = FontStyle.Bold
        };
        entryNodeStyle = new GUIStyle("flow node 4")
        {
            alignment = TextAnchor.UpperLeft,
            padding = new RectOffset(12, 10, 10, 8),
            fontStyle = FontStyle.Bold
        };
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(ToolbarHeight));

        if (GUILayout.Button("상태 추가", EditorStyles.toolbarButton, GUILayout.Width(72f))) AddState();

        if (GUILayout.Button("Any State", EditorStyles.toolbarButton, GUILayout.Width(78f)))
        {
            isGlobalSelected = true;
            selectedStateId = string.Empty;
        }

        if (GUILayout.Button("그래프 검사", EditorStyles.toolbarButton, GUILayout.Width(82f))) ValidateGraph();

        GUILayout.FlexibleSpace();
        GUILayout.Label(brain.name, EditorStyles.miniLabel);

        if (Application.isPlaying)
        {
            string stateName = brain.CurrentState == null ? "없음" : brain.CurrentState.Name;
            GUILayout.Space(12f);
            GUILayout.Label($"실행 중: {stateName}", EditorStyles.miniBoldLabel);
            Repaint();
        }

        GUILayout.EndHorizontal();
    }

    private void DrawGraph(Rect graphRect)
    {
        EditorGUI.DrawRect(graphRect, new Color(0.12f, 0.12f, 0.12f));
        Rect canvasRect = new(0f, 0f, CanvasWidth, CanvasHeight);
        graphScroll = GUI.BeginScrollView(graphRect, graphScroll, canvasRect);

        DrawGrid(canvasRect, 24f, new Color(1f, 1f, 1f, 0.035f));
        DrawGrid(canvasRect, 120f, new Color(1f, 1f, 1f, 0.07f));
        DrawConnections();
        DrawGlobalNode();
        DrawStateNodes();

        GUI.EndScrollView();
    }

    private static void DrawGrid(Rect rect, float spacing, Color color)
    {
        Handles.BeginGUI();
        Handles.color = color;

        for (float x = 0f; x < rect.width; x += spacing) Handles.DrawLine(new Vector3(x, 0f), new Vector3(x, rect.height));

        for (float y = 0f; y < rect.height; y += spacing) Handles.DrawLine(new Vector3(0f, y), new Vector3(rect.width, y));

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void DrawGlobalNode()
    {
        Rect rect = GetGlobalNodeRect();
        GUIStyle style = isGlobalSelected ? selectedNodeStyle : nodeStyle;
        GUI.Box(rect, "Any State", style);

        SerializedProperty transitions = GetGraphProperty().FindPropertyRelative("globalTransitions");
        GUI.Label(
            new Rect(rect.x + 12f, rect.y + 38f, rect.width - 24f, 20f),
            $"전이 {transitions.arraySize}개",
            EditorStyles.miniLabel);

        Event currentEvent = Event.current;
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
        {
            isGlobalSelected = true;
            selectedStateId = string.Empty;
            currentEvent.Use();
            Repaint();
        }
    }

    private void DrawStateNodes()
    {
        SerializedProperty states = GetStatesProperty();
        SerializedProperty entryStateId = GetGraphProperty().FindPropertyRelative("entryStateId");
        Event currentEvent = Event.current;

        for (int index = 0; index < states.arraySize; index++)
        {
            SerializedProperty state = states.GetArrayElementAtIndex(index);
            string stateId = state.FindPropertyRelative("id").stringValue;
            string stateName = state.FindPropertyRelative("name").stringValue;
            SerializedProperty positionProperty = state.FindPropertyRelative("editorPosition");
            Rect rect = new(positionProperty.vector2Value, new Vector2(NodeWidth, NodeHeight));
            bool isEntry = entryStateId.stringValue == stateId;
            bool isSelected = selectedStateId == stateId && !isGlobalSelected;
            GUIStyle style = isEntry ? entryNodeStyle : isSelected ? selectedNodeStyle : nodeStyle;
            string title = isEntry ? $"▶ {stateName}" : stateName;

            GUI.Box(rect, title, style);

            int actionCount = state.FindPropertyRelative("actions").arraySize;
            int transitionCount = state.FindPropertyRelative("transitions").arraySize;
            GUI.Label(
                new Rect(rect.x + 12f, rect.y + 38f, rect.width - 24f, 20f),
                $"행동 {actionCount}개  ·  전이 {transitionCount}개",
                EditorStyles.miniLabel);

            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
            {
                selectedStateId = stateId;
                isGlobalSelected = false;
                GUI.FocusControl(null);
                currentEvent.Use();
                Repaint();
            }

            if (currentEvent.type == EventType.MouseDrag &&
                currentEvent.button == 0 &&
                isSelected)
            {
                Undo.RecordObject(brain, "Move Enemy State");
                positionProperty.vector2Value += currentEvent.delta;
                currentEvent.Use();
                Repaint();
            }
        }
    }

    private void DrawConnections()
    {
        SerializedProperty graph = GetGraphProperty();
        SerializedProperty states = graph.FindPropertyRelative("states");
        SerializedProperty globalTransitions = graph.FindPropertyRelative("globalTransitions");

        Handles.BeginGUI();
        DrawTransitionConnections(GetGlobalNodeRect(), globalTransitions, states, new Color(0.9f, 0.55f, 0.2f));

        for (int stateIndex = 0; stateIndex < states.arraySize; stateIndex++)
        {
            SerializedProperty state = states.GetArrayElementAtIndex(stateIndex);
            Vector2 statePosition = state.FindPropertyRelative("editorPosition").vector2Value;
            Rect sourceRect = new(statePosition, new Vector2(NodeWidth, NodeHeight));
            SerializedProperty transitions = state.FindPropertyRelative("transitions");
            DrawTransitionConnections(sourceRect, transitions, states, new Color(0.4f, 0.75f, 1f));
        }

        Handles.EndGUI();
    }

    private static void DrawTransitionConnections(
        Rect sourceRect,
        SerializedProperty transitions,
        SerializedProperty states,
        Color color)
    {
        for (int transitionIndex = 0; transitionIndex < transitions.arraySize; transitionIndex++)
        {
            SerializedProperty transition = transitions.GetArrayElementAtIndex(transitionIndex);
            string targetStateId = transition.FindPropertyRelative("targetStateId").stringValue;
            SerializedProperty targetState = FindStateProperty(states, targetStateId);
            if (targetState == null) continue;

            Vector2 targetPosition = targetState.FindPropertyRelative("editorPosition").vector2Value;
            Rect targetRect = new(targetPosition, new Vector2(NodeWidth, NodeHeight));
            Vector3 start = new(sourceRect.xMax, sourceRect.center.y);
            Vector3 end = new(targetRect.xMin, targetRect.center.y);

            if (end.x < start.x)
            {
                start = new Vector3(sourceRect.center.x, sourceRect.yMax);
                end = new Vector3(targetRect.center.x, targetRect.yMin);
            }

            float tangentDistance = Mathf.Max(60f, Mathf.Abs(end.x - start.x) * 0.45f);
            Vector3 startTangent = start + Vector3.right * tangentDistance;
            Vector3 endTangent = end + Vector3.left * tangentDistance;

            Handles.DrawBezier(start, end, startTangent, endTangent, color, null, 3f);
            Vector2 labelPosition = Vector2.Lerp(start, end, 0.5f);
            GUI.Label(
                new Rect(labelPosition.x - 12f, labelPosition.y - 11f, 24f, 20f),
                (transitionIndex + 1).ToString(),
                EditorStyles.miniBoldLabel);
        }
    }

    private void DrawDetails(Rect detailRect)
    {
        EditorGUI.DrawRect(detailRect, new Color(0.17f, 0.17f, 0.17f));
        GUILayout.BeginArea(new Rect(detailRect.x + 8f, detailRect.y + 8f, detailRect.width - 16f, detailRect.height - 16f));
        detailScroll = EditorGUILayout.BeginScrollView(detailScroll);

        if (isGlobalSelected)
        {
            EditorGUILayout.LabelField("Any State", EditorStyles.largeLabel);
            EditorGUILayout.HelpBox(
                "현재 상태와 관계없이 먼저 검사합니다. 체력 저하 도주나 강제 페이즈 전환에 사용하세요.",
                MessageType.Info);
            DrawTransitions(GetGraphProperty().FindPropertyRelative("globalTransitions"));
        }
        else
        {
            SerializedProperty state = FindSelectedStateProperty();

            if (state == null)
            {
                EditorGUILayout.HelpBox("상태를 선택하거나 새 상태를 추가하세요.", MessageType.Info);
            }
            else
            {
                DrawStateDetails(state);
            }
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawStateDetails(SerializedProperty state)
    {
        SerializedProperty stateName = state.FindPropertyRelative("name");
        SerializedProperty executionMode = state.FindPropertyRelative("executionMode");
        SerializedProperty loopSequence = state.FindPropertyRelative("loopSequence");

        EditorGUILayout.LabelField("상태 설정", EditorStyles.largeLabel);
        EditorGUILayout.PropertyField(stateName, new GUIContent("이름"));
        EditorGUILayout.PropertyField(executionMode, new GUIContent("행동 실행 방식"));

        if ((EnemyActionExecutionMode)executionMode.enumValueIndex == EnemyActionExecutionMode.Sequence) EditorGUILayout.PropertyField(loopSequence, new GUIContent("순서 반복"));

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("시작 상태로 지정"))
        {
            Undo.RecordObject(brain, "Set Enemy Entry State");
            GetGraphProperty().FindPropertyRelative("entryStateId").stringValue =
                state.FindPropertyRelative("id").stringValue;
        }

        GUI.backgroundColor = new Color(1f, 0.55f, 0.55f);
        if (GUILayout.Button("상태 삭제", GUILayout.Width(82f))) DeleteSelectedState();
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10f);

        EditorGUILayout.LabelField("행동", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "병렬은 모든 행동을 함께 실행하고, 순서는 위에서 아래로 완료된 뒤 다음 행동을 실행합니다.",
            MessageType.None);
        DrawActions(state.FindPropertyRelative("actions"));
        EditorGUILayout.Space(14f);
        DrawTransitions(state.FindPropertyRelative("transitions"));
    }

    private void DrawActions(SerializedProperty actions)
    {
        for (int index = 0; index < actions.arraySize; index++)
        {
            SerializedProperty action = actions.GetArrayElementAtIndex(index);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{index + 1}. {GetManagedReferenceName(action)}", EditorStyles.boldLabel);
            DrawReorderButtons(actions, index);

            if (GUILayout.Button("×", GUILayout.Width(24f)))
            {
                Undo.RecordObject(brain, "Remove Enemy Action");
                actions.DeleteArrayElementAtIndex(index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }

            EditorGUILayout.EndHorizontal();
            DrawManagedReferenceFields(action);
            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("+ 행동 추가")) ShowTypeMenu<EnemyAction>(type => AddManagedReference(actions, type, "Add Enemy Action"));
    }

    private void DrawTransitions(SerializedProperty transitions)
    {
        EditorGUILayout.LabelField("전이", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "위에 있는 전이부터 검사합니다. 조건이 없으면 최소 체류 시간이 지난 뒤 무조건 전이합니다.",
            MessageType.None);

        for (int index = 0; index < transitions.arraySize; index++)
        {
            SerializedProperty transition = transitions.GetArrayElementAtIndex(index);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"우선순위 {index + 1}", EditorStyles.boldLabel);
            DrawReorderButtons(transitions, index);

            if (GUILayout.Button("×", GUILayout.Width(24f)))
            {
                Undo.RecordObject(brain, "Remove Enemy Transition");
                transitions.DeleteArrayElementAtIndex(index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }

            EditorGUILayout.EndHorizontal();
            DrawTargetStatePopup(transition.FindPropertyRelative("targetStateId"));
            EditorGUILayout.PropertyField(
                transition.FindPropertyRelative("minimumStateDuration"),
                new GUIContent("최소 체류 시간"));
            EditorGUILayout.PropertyField(
                transition.FindPropertyRelative("allowSelfTransition"),
                new GUIContent("자기 자신으로 재진입"));
            EditorGUILayout.PropertyField(
                transition.FindPropertyRelative("evaluationMode"),
                new GUIContent("조건 평가"));
            DrawConditions(transition.FindPropertyRelative("conditions"));
            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("+ 전이 추가")) AddTransition(transitions);
    }

    private void DrawConditions(SerializedProperty conditions)
    {
        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("조건", EditorStyles.miniBoldLabel);

        for (int index = 0; index < conditions.arraySize; index++)
        {
            SerializedProperty slot = conditions.GetArrayElementAtIndex(index);
            SerializedProperty condition = slot.FindPropertyRelative("condition");
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{index + 1}. {GetManagedReferenceName(condition)}", EditorStyles.miniBoldLabel);
            DrawReorderButtons(conditions, index);

            if (GUILayout.Button("×", GUILayout.Width(24f)))
            {
                Undo.RecordObject(brain, "Remove Enemy Condition");
                conditions.DeleteArrayElementAtIndex(index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(slot.FindPropertyRelative("inverted"), new GUIContent("결과 반전"));
            DrawManagedReferenceFields(condition);
            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("+ 조건 추가"))
        {
            ShowTypeMenu<EnemyCondition>(type =>
            {
                Undo.RecordObject(brain, "Add Enemy Condition");
                int index = conditions.arraySize;
                conditions.arraySize++;
                SerializedProperty slot = conditions.GetArrayElementAtIndex(index);
                slot.FindPropertyRelative("inverted").boolValue = false;
                slot.FindPropertyRelative("condition").managedReferenceValue = Activator.CreateInstance(type);
                serializedBrain.ApplyModifiedProperties();
            });
        }
    }

    private static void DrawManagedReferenceFields(SerializedProperty managedReference)
    {
        object managedValue = managedReference.managedReferenceValue;
        if (managedValue == null)
        {
            EditorGUILayout.HelpBox("직렬화된 타입을 찾을 수 없습니다.", MessageType.Error);
            return;
        }

        bool hasVisibleField = false;

        foreach (FieldInfo field in GetSerializableFields(managedValue.GetType()))
        {
            SerializedProperty fieldProperty = managedReference.FindPropertyRelative(field.Name);
            if (fieldProperty == null) continue;

            EditorGUILayout.PropertyField(fieldProperty, true);
            hasVisibleField = true;
        }

        if (!hasVisibleField) EditorGUILayout.LabelField("설정 없음", EditorStyles.miniLabel);
    }

    private static IEnumerable<FieldInfo> GetSerializableFields(Type type)
    {
        List<Type> hierarchy = new();

        for (Type currentType = type; currentType != null && currentType != typeof(object); currentType = currentType.BaseType)
            hierarchy.Add(currentType);

        hierarchy.Reverse();

        foreach (Type hierarchyType in hierarchy)
        {
            FieldInfo[] fields = hierarchyType.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            foreach (FieldInfo field in fields.OrderBy(field => field.MetadataToken))
            {
                if (IsSerializableField(field)) yield return field;
            }
        }
    }

    private static bool IsSerializableField(FieldInfo field)
    {
        if (field.IsStatic || field.IsInitOnly) return false;
        if (field.GetCustomAttribute<NonSerializedAttribute>() != null) return false;
        if (field.GetCustomAttribute<HideInInspector>() != null) return false;
        if (field.IsPublic) return true;

        return field.GetCustomAttribute<SerializeField>() != null ||
               field.GetCustomAttribute<SerializeReference>() != null;
    }

    private void DrawTargetStatePopup(SerializedProperty targetStateId)
    {
        SerializedProperty states = GetStatesProperty();
        string[] names = new string[states.arraySize];
        string[] ids = new string[states.arraySize];
        int selectedIndex = -1;

        for (int index = 0; index < states.arraySize; index++)
        {
            SerializedProperty state = states.GetArrayElementAtIndex(index);
            names[index] = state.FindPropertyRelative("name").stringValue;
            ids[index] = state.FindPropertyRelative("id").stringValue;

            if (ids[index] == targetStateId.stringValue) selectedIndex = index;
        }

        if (states.arraySize == 0)
        {
            EditorGUILayout.HelpBox("전이 대상 상태가 없습니다.", MessageType.Error);
            return;
        }

        int newIndex = EditorGUILayout.Popup("대상 상태", Mathf.Max(0, selectedIndex), names);
        targetStateId.stringValue = ids[newIndex];
    }

    private static void DrawReorderButtons(SerializedProperty list, int index)
    {
        EditorGUI.BeginDisabledGroup(index == 0);
        if (GUILayout.Button("▲", GUILayout.Width(24f))) list.MoveArrayElement(index, index - 1);
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(index >= list.arraySize - 1);
        if (GUILayout.Button("▼", GUILayout.Width(24f))) list.MoveArrayElement(index, index + 1);
        EditorGUI.EndDisabledGroup();
    }

    private void AddState()
    {
        serializedBrain.Update();
        Undo.RecordObject(brain, "Add Enemy State");
        SerializedProperty states = GetStatesProperty();
        Vector2 position = graphScroll + new Vector2(260f, 140f + states.arraySize * 110f);
        string stateId = Guid.NewGuid().ToString("N");
        int index = states.arraySize;
        states.arraySize++;
        SerializedProperty state = states.GetArrayElementAtIndex(index);
        state.FindPropertyRelative("id").stringValue = stateId;
        state.FindPropertyRelative("name").stringValue = $"State {states.arraySize}";
        state.FindPropertyRelative("editorPosition").vector2Value = position;
        state.FindPropertyRelative("executionMode").enumValueIndex = 0;
        state.FindPropertyRelative("loopSequence").boolValue = false;
        state.FindPropertyRelative("actions").arraySize = 0;
        state.FindPropertyRelative("transitions").arraySize = 0;

        SerializedProperty entryStateId = GetGraphProperty().FindPropertyRelative("entryStateId");
        if (string.IsNullOrWhiteSpace(entryStateId.stringValue)) entryStateId.stringValue = stateId;

        selectedStateId = stateId;
        isGlobalSelected = false;
        serializedBrain.ApplyModifiedProperties();
        Repaint();
    }

    private void DeleteSelectedState()
    {
        SerializedProperty states = GetStatesProperty();
        int selectedIndex = FindStateIndex(states, selectedStateId);
        if (selectedIndex < 0) return;

        if (!EditorUtility.DisplayDialog("상태 삭제", "선택한 상태와 연결된 전이를 모두 삭제합니다.", "삭제", "취소")) return;

        Undo.RecordObject(brain, "Delete Enemy State");
        RemoveTransitionsTo(selectedStateId);
        states.DeleteArrayElementAtIndex(selectedIndex);
        SerializedProperty entryStateId = GetGraphProperty().FindPropertyRelative("entryStateId");

        if (entryStateId.stringValue == selectedStateId) entryStateId.stringValue = states.arraySize > 0
            ? states.GetArrayElementAtIndex(0).FindPropertyRelative("id").stringValue
            : string.Empty;

        selectedStateId = string.Empty;
        serializedBrain.ApplyModifiedProperties();
        Repaint();
    }

    private void RemoveTransitionsTo(string stateId)
    {
        SerializedProperty graph = GetGraphProperty();
        RemoveTransitionsTo(graph.FindPropertyRelative("globalTransitions"), stateId);
        SerializedProperty states = graph.FindPropertyRelative("states");

        for (int index = 0; index < states.arraySize; index++) RemoveTransitionsTo(states.GetArrayElementAtIndex(index).FindPropertyRelative("transitions"), stateId);
    }

    private static void RemoveTransitionsTo(SerializedProperty transitions, string stateId)
    {
        for (int index = transitions.arraySize - 1; index >= 0; index--)
        {
            SerializedProperty transition = transitions.GetArrayElementAtIndex(index);
            if (transition.FindPropertyRelative("targetStateId").stringValue == stateId) transitions.DeleteArrayElementAtIndex(index);
        }
    }

    private void AddTransition(SerializedProperty transitions)
    {
        SerializedProperty states = GetStatesProperty();
        if (states.arraySize == 0)
        {
            EditorUtility.DisplayDialog("전이 추가", "먼저 상태를 추가하세요.", "확인");
            return;
        }

        Undo.RecordObject(brain, "Add Enemy Transition");
        int index = transitions.arraySize;
        transitions.arraySize++;
        SerializedProperty transition = transitions.GetArrayElementAtIndex(index);
        transition.FindPropertyRelative("targetStateId").stringValue =
            states.GetArrayElementAtIndex(0).FindPropertyRelative("id").stringValue;
        transition.FindPropertyRelative("minimumStateDuration").floatValue = 0f;
        transition.FindPropertyRelative("allowSelfTransition").boolValue = false;
        transition.FindPropertyRelative("evaluationMode").enumValueIndex = 0;
        transition.FindPropertyRelative("conditions").arraySize = 0;
        serializedBrain.ApplyModifiedProperties();
    }

    private void AddManagedReference(SerializedProperty list, Type type, string undoName)
    {
        Undo.RecordObject(brain, undoName);
        int index = list.arraySize;
        list.arraySize++;
        list.GetArrayElementAtIndex(index).managedReferenceValue = Activator.CreateInstance(type);
        serializedBrain.ApplyModifiedProperties();
    }

    private static void ShowTypeMenu<T>(Action<Type> onSelected)
    {
        GenericMenu menu = new();
        IEnumerable<Type> types = TypeCache.GetTypesDerivedFrom<T>()
            .Where(type => !type.IsAbstract && !type.IsGenericType)
            .OrderBy(GetMenuPath);

        foreach (Type type in types)
        {
            Type capturedType = type;
            menu.AddItem(new GUIContent(GetMenuPath(type)), false, () => onSelected(capturedType));
        }

        menu.ShowAsContext();
    }

    private static string GetMenuPath(Type type)
    {
        EnemyBehaviorMenuAttribute attribute =
            type.GetCustomAttributes(typeof(EnemyBehaviorMenuAttribute), false)
                .FirstOrDefault() as EnemyBehaviorMenuAttribute;
        return attribute?.Path ?? ObjectNames.NicifyVariableName(type.Name);
    }

    private static string GetManagedReferenceName(SerializedProperty property)
    {
        if (string.IsNullOrWhiteSpace(property.managedReferenceFullTypename)) return "Missing";

        string[] typeParts = property.managedReferenceFullTypename.Split(' ');
        string assemblyName = typeParts[0];
        string typeName = typeParts[1];
        Type type = Type.GetType($"{typeName}, {assemblyName}");
        return type == null ? typeName.Split('.').Last() : GetMenuPath(type).Split('/').Last();
    }

    private void ValidateGraph()
    {
        serializedBrain.ApplyModifiedProperties();
        serializedBrain.Update();
        List<string> issues = new();
        SerializedProperty graph = GetGraphProperty();
        SerializedProperty states = graph.FindPropertyRelative("states");
        string entryStateId = graph.FindPropertyRelative("entryStateId").stringValue;

        if (states.arraySize == 0) issues.Add("상태가 없습니다.");
        else if (FindStateProperty(states, entryStateId) == null) issues.Add("시작 상태가 올바르지 않습니다.");

        HashSet<string> ids = new();

        for (int stateIndex = 0; stateIndex < states.arraySize; stateIndex++)
        {
            SerializedProperty state = states.GetArrayElementAtIndex(stateIndex);
            string stateId = state.FindPropertyRelative("id").stringValue;
            string stateName = state.FindPropertyRelative("name").stringValue;

            if (!ids.Add(stateId)) issues.Add($"'{stateName}'의 ID가 중복됩니다.");

            ValidateManagedReferences(state.FindPropertyRelative("actions"), $"'{stateName}' 행동", issues);
            ValidateTransitions(state.FindPropertyRelative("transitions"), states, $"'{stateName}'", issues);
        }

        ValidateTransitions(graph.FindPropertyRelative("globalTransitions"), states, "Any State", issues);

        if (UsesHealthCondition(graph) && !HasHealthSource()) issues.Add("체력 조건을 사용하지만 같은 오브젝트에 IEnemyHealthSource가 없습니다.");

        string message = issues.Count == 0
            ? "문제를 찾지 못했습니다."
            : string.Join("\n", issues.Take(20));
        EditorUtility.DisplayDialog("행동 그래프 검사", message, "확인");
    }

    private static void ValidateTransitions(
        SerializedProperty transitions,
        SerializedProperty states,
        string ownerName,
        ICollection<string> issues)
    {
        for (int index = 0; index < transitions.arraySize; index++)
        {
            SerializedProperty transition = transitions.GetArrayElementAtIndex(index);
            string targetStateId = transition.FindPropertyRelative("targetStateId").stringValue;

            if (FindStateProperty(states, targetStateId) == null) issues.Add($"{ownerName}의 전이 {index + 1}에 대상 상태가 없습니다.");

            SerializedProperty conditions = transition.FindPropertyRelative("conditions");

            for (int conditionIndex = 0; conditionIndex < conditions.arraySize; conditionIndex++)
            {
                SerializedProperty condition = conditions.GetArrayElementAtIndex(conditionIndex)
                    .FindPropertyRelative("condition");
                if (condition.managedReferenceValue == null) issues.Add($"{ownerName}의 전이 {index + 1}에 비어 있는 조건이 있습니다.");
                else ValidateRequiredFields(
                    condition.managedReferenceValue,
                    $"{ownerName}의 전이 {index + 1} 조건 {conditionIndex + 1}",
                    issues);
            }
        }
    }

    private static void ValidateManagedReferences(
        SerializedProperty properties,
        string ownerName,
        ICollection<string> issues)
    {
        for (int index = 0; index < properties.arraySize; index++)
        {
            object value = properties.GetArrayElementAtIndex(index).managedReferenceValue;
            if (value == null) issues.Add($"{ownerName} {index + 1}이 비어 있습니다.");
            else ValidateRequiredFields(value, $"{ownerName} {index + 1}", issues);
        }
    }

    private static void ValidateRequiredFields(
        object target,
        string ownerName,
        ICollection<string> issues)
    {
        FieldInfo[] fields = target.GetType().GetFields(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (FieldInfo field in fields)
        {
            if (field.GetCustomAttribute<RequiredAttribute>() == null) continue;
            if (field.GetValue(target) is UnityEngine.Object value && value != null) continue;

            EnemyBehaviorFieldAttribute label = field.GetCustomAttribute<EnemyBehaviorFieldAttribute>();
            string fieldName = label?.Label ?? ObjectNames.NicifyVariableName(field.Name);
            issues.Add($"{ownerName}의 '{fieldName}'이 비어 있습니다.");
        }
    }

    private static bool UsesHealthCondition(SerializedProperty graph)
    {
        SerializedProperty states = graph.FindPropertyRelative("states");

        if (TransitionsUseHealthCondition(graph.FindPropertyRelative("globalTransitions"))) return true;

        for (int index = 0; index < states.arraySize; index++)
        {
            SerializedProperty transitions = states.GetArrayElementAtIndex(index).FindPropertyRelative("transitions");
            if (TransitionsUseHealthCondition(transitions)) return true;
        }

        return false;
    }

    private bool HasHealthSource()
    {
        foreach (MonoBehaviour component in brain.GetComponents<MonoBehaviour>())
        {
            if (component is IEnemyHealthSource) return true;
        }

        return false;
    }

    private static bool TransitionsUseHealthCondition(SerializedProperty transitions)
    {
        for (int transitionIndex = 0; transitionIndex < transitions.arraySize; transitionIndex++)
        {
            SerializedProperty conditions = transitions.GetArrayElementAtIndex(transitionIndex)
                .FindPropertyRelative("conditions");

            for (int conditionIndex = 0; conditionIndex < conditions.arraySize; conditionIndex++)
            {
                SerializedProperty condition = conditions.GetArrayElementAtIndex(conditionIndex)
                    .FindPropertyRelative("condition");
                if (condition.managedReferenceValue is HealthRatioEnemyCondition) return true;
            }
        }

        return false;
    }

    private SerializedProperty GetGraphProperty() => serializedBrain.FindProperty("graph");

    private SerializedProperty GetStatesProperty() => GetGraphProperty().FindPropertyRelative("states");

    private SerializedProperty FindSelectedStateProperty() =>
        FindStateProperty(GetStatesProperty(), selectedStateId);

    private static SerializedProperty FindStateProperty(SerializedProperty states, string stateId)
    {
        int index = FindStateIndex(states, stateId);
        return index < 0 ? null : states.GetArrayElementAtIndex(index);
    }

    private static int FindStateIndex(SerializedProperty states, string stateId)
    {
        for (int index = 0; index < states.arraySize; index++)
        {
            if (states.GetArrayElementAtIndex(index).FindPropertyRelative("id").stringValue == stateId) return index;
        }

        return -1;
    }

    private static Rect GetGlobalNodeRect() => new(40f, 70f, NodeWidth, NodeHeight);
}