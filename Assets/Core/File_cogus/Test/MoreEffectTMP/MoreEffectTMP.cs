using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public partial class MoreEffectTMP : MonoBehaviour
{
    private const float X_MOVE_RANGE = 0.08f;

    [SerializeField] private float rowInterval = 0.1f;

    private TMP_Text text;
    private List<TagPoint> tagPoints = new();
    private readonly List<FixPoint> _fixPoints = new();
    private bool preProcess;
    private float timer;
    private string prevValue = "";

    public TMP_Text Text => text;

    public IEnumerator Typing(
        string pContext,
        float pInterval,
        float pCallBackTerm,
        Action pCallback = null,
        Func<bool> pBreakCondition = null)
    {
        string cur = text.text;
        int originCnt = cur.Length;

        text.text += pContext;
        Update();
        List<FixPoint> fixPoints = _fixPoints.ToList();

        int idx = -1;
        int fixPointIdx = 0;

        foreach (TMP_CharacterInfo charInfo in text.textInfo.characterInfo.Take(text.textInfo.characterCount).Skip(originCnt))
        {
            if (pBreakCondition?.Invoke() ?? false)
            {
                text.text = pContext;
                pCallback?.Invoke();
                yield break;
            }

            idx++;
            cur += charInfo.character;
            bool remainFixPoint = fixPoints.Count > fixPointIdx;

            if (remainFixPoint && idx >= fixPoints[fixPointIdx].Start && idx < fixPoints[fixPointIdx].End) continue;
            if (!charInfo.isVisible) continue;

            text.text = cur;
            Update();
            yield return new WaitForSeconds(pInterval);
        }

        yield return new WaitForSeconds(pCallBackTerm);
        pCallback?.Invoke();
    }

    private void Apply()
    {
        if (tagPoints.Count < 1) return;

        float prevTime = timer;
        timer += Time.deltaTime / Time.timeScale;

        TMP_TextInfo textInfo = text.textInfo;
        int idx = tagPoints[0].Start;
        TMP_CharacterInfo[] charInfos = textInfo.characterInfo;

        foreach (TagPoint tag in tagPoints)
        {
            for (; idx <= tag.End && idx < textInfo.characterCount; idx++)
            {
                TMP_CharacterInfo charInfo = charInfos[idx];

                if (!charInfo.isVisible) continue;

                Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                Func<float, int, float, (Vector3, Vector3)> func = _changePosFunc[tag.Type];
                Vector3 prevPos = Vector3.zero;
                Vector3 prevRotation = Vector3.zero;

                if (!preProcess) (prevPos, prevRotation) = func(prevTime, idx, tag.Arg);

                (Vector3 pos, Vector3 rotation) = func(timer, idx, tag.Arg);
                pos -= prevPos;
                rotation -= prevRotation;
                pos.y *= charInfo.pointSize * rowInterval;
                pos.x *= charInfo.pointSize * charInfo.aspectRatio * X_MOVE_RANGE;

                for (int vertex = 0; vertex < 4; vertex++)
                {
                    int vertexIdx = charInfos[idx].vertexIndex + vertex;
                    vertices[vertexIdx] += pos;
                }
            }
        }

        preProcess = false;
        TMPUpdate();
    }

    private void Update()
    {
        if (text.text != prevValue)
        {
            prevValue = text.text;
            text.ForceMeshUpdate(true);
            Setting();
        }

        Apply();
    }

    private void OnEnable() => prevValue = "";

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        text.textWrappingMode = TextWrappingModes.NoWrap;
    }
}
