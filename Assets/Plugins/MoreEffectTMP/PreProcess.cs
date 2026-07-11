using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public partial class MoreEffectTMP
{
    private const char TAG_OPEN = '<';
    private const char TAG_CLOSE = '>';
    private const char TAG_END_IDENTIFIER = '/';
    private const string PATTERN = @"(?<Tag>.+)=(?<Arg>.*)";

    private void Setting()
    {
        TMP_TextInfo textInfo = text.textInfo;
        StartPoint tagStartPosition = null;
        Stack<TagPoint> tagStack = new();
        string curTag = "";
        int idx = -1;
        List<TagPoint> tempTagPoint = new();
        _fixPoints.Clear();

        TMP_CharacterInfo prev = new();
        bool canTag = true;

        foreach (TMP_CharacterInfo charInfo in textInfo.characterInfo.Take(textInfo.characterCount))
        {
            idx++;

            if (!charInfo.isVisible) continue;

            if (prev.character != TAG_CLOSE && tagStartPosition != null)
            {
                if (charInfo.character != TAG_CLOSE)
                {
                    curTag += charInfo.character;
                }
                else
                {
                    if (!IsTag()) canTag = false;
                    curTag = "";
                }
            }

            Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            if (tagStartPosition != null && prev.character == TAG_CLOSE)
            {
                if (canTag)
                {
                    (int start, Vector3 pos) = tagStartPosition;
                    _fixPoints.Add(new FixPoint(start, idx, pos));
                    tagStartPosition = null;
                }
                else
                {
                    tagStartPosition = null;
                    canTag = true;
                }
            }

            if (charInfo.character == TAG_OPEN)
            {
                curTag = "";
                tagStartPosition = new StartPoint(idx, vertices[charInfo.vertexIndex]);
            }

            prev = charInfo;
        }

        if (tagStartPosition != null && prev.character == TAG_CLOSE)
        {
            if (canTag)
            {
                (int start, Vector3 pos) = tagStartPosition;
                _fixPoints.Add(new FixPoint(start, idx + 1, pos));
            }

            tagStartPosition = null;
        }

        while (tagStack.Count > 0)
        {
            TagPoint data = tagStack.Pop();
            data.End = int.MaxValue;
            tempTagPoint.Add(data);
        }

        tagPoints = tempTagPoint.OrderBy(point => point.Start).ToList();
        RemoveTagAndDivideRow();

        bool IsTag()
        {
            if (curTag.Length == 0) return false;

            if (curTag[0] == TAG_END_IDENTIFIER)
            {
                if (!Parse<TMP_EffectType>(curTag[1..], out TMP_EffectType tag)) return false;

                if (tagStack.Count > 0 && tag == tagStack.Peek().Type)
                {
                    TagPoint data = tagStack.Pop();
                    data.End = tagStartPosition.Idx - 1;
                    tempTagPoint.Add(data);
                }
                else return false;
            }
            else
            {
                Match match = Regex.Match(curTag, PATTERN);
                TMP_EffectType tag = TMP_EffectType.Error;
                string argString = "1";

                if (match.Success)
                {
                    if (!Parse(match.Groups["Tag"].Value, out tag)) return false;
                    argString = match.Groups["Arg"].Value;
                }
                else if (!Parse(curTag, out tag)) return false;

                if (!float.TryParse(argString, out float arg)) arg = 1f;

                tagStack.Push(new TagPoint(idx + 1, 0, tag, arg));
            }

            return true;
        }
    }

    private void RemoveTagAndDivideRow()
    {
        const float DEFAULT_UNITY_ROW_INTERVAL = 0.45f;

        TMP_TextInfo textInfo = text.textInfo;
        int idx = -1;
        int fixPointIdx = 0;
        Vector3 fix = Vector3.zero;
        float width = ((RectTransform)transform).sizeDelta.x;
        float startPos = 0f;
        int newLineCnt = 0;
        int needLineCnt = 0;
        int singleNewLineCnt = 0;
        int line = 0;
        float pad = 0f;

        foreach (TMP_CharacterInfo charInfo in textInfo.characterInfo.Take(textInfo.characterCount))
        {
            idx++;

            if (idx == 0) startPos = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices[charInfo.vertexIndex].x;

            if (!charInfo.isVisible)
            {
                if (charInfo.character == '\n')
                {
                    needLineCnt += singleNewLineCnt;
                    newLineCnt++;
                    fix.x = 0;
                }

                continue;
            }

            Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            bool remainSingleTag = _fixPoints.Count > fixPointIdx;
            FixPoint peek = new(0, 0, Vector3.zero);

            if (remainSingleTag)
            {
                peek = _fixPoints[fixPointIdx];

                if (idx >= peek.End)
                {
                    fixPointIdx++;
                    fix.x += peek.Pos.x - vertices[charInfo.vertexIndex].x;
                    remainSingleTag = _fixPoints.Count > fixPointIdx;

                    if (remainSingleTag) peek = _fixPoints[fixPointIdx];
                }
            }

            Vector3 lineFix = Vector3.zero;

            for (int vertex = 0; vertex < 4; vertex++)
            {
                int vertexIdx = charInfo.vertexIndex + vertex;

                if (remainSingleTag && peek.Start <= idx && idx < peek.End)
                {
                    vertices[vertexIdx] = peek.Pos;
                }
                else if (vertex == 0)
                {
                    int prevLine = line;
                    Vector3 temp = vertices[vertexIdx] + fix;
                    float dist = Mathf.Max(0, temp.x - startPos);

                    singleNewLineCnt = Mathf.FloorToInt(dist / width);
                    line = singleNewLineCnt + newLineCnt + needLineCnt;
                    float lineFixX = dist % width - dist;

                    if (line != prevLine) pad = startPos - lineFixX - temp.x;

                    lineFix.x = lineFixX + pad;
                    lineFix.y = (newLineCnt * (1 + DEFAULT_UNITY_ROW_INTERVAL) - line * (1 + rowInterval)) * charInfo.pointSize;
                    vertices[vertexIdx] = temp + lineFix;
                }
                else
                {
                    vertices[vertexIdx] += fix + lineFix;
                }
            }
        }

        TMPUpdate();
        preProcess = true;
        timer = 0;
    }

    private void TMPUpdate()
    {
        TMP_TextInfo textInfo = text.textInfo;
        Vector3[] meshVertices;

        foreach (TMP_MeshInfo mesh in textInfo.meshInfo)
        {
            meshVertices = mesh.vertices;
            mesh.mesh.RecalculateBounds();
        }

        text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }

    private static bool Parse<T>(string pContext, out T pValue) where T : struct, Enum =>
        Enum.TryParse(pContext, out pValue);
}
