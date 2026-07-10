using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Extension {
    public partial class MoreEffectTMP {
                
        //==================================================||Fields 
        const char TAG_OPEN = '<';
        const char TAG_CLOSE = '>';
        const char TAG_END_IDENTIFIER = '/';
        const string PATTERN = @"(?<Tag>.+)=(?<Arg>.*)";

        
        private void Setting() {
            var textInfo = _text.textInfo;

            StartPoint tagStartPosition = null;
            var tagStack = new Stack<TagPoint>();
            var curTag = "";
            var idx = -1;
            var tempTagPoint = new List<TagPoint>();
            _fixPoints.Clear();
            
            var prev = new TMP_CharacterInfo();
            var canTag = true;
            foreach (var charInfo in textInfo.characterInfo.Take(textInfo.characterCount)) {
                idx++;
                
                if(!charInfo.isVisible)
                    continue;

                if (prev.character != TAG_CLOSE && tagStartPosition != null) {
                    if (charInfo.character != TAG_CLOSE) {
                        curTag += charInfo.character;
                    }
                    else {
                       if(!IsTag()) {
                            canTag = false;
                       }
                       curTag = "";
                    }
                }
                
                var vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                
                if (tagStartPosition != null && prev.character == TAG_CLOSE) {
                    if (canTag) {
                        var (start, pos) = tagStartPosition;
                        _fixPoints.Add(new(start, idx, pos));
                        tagStartPosition = null;
                    }
                    else {
                        tagStartPosition = null;
                        canTag = true;
                    }
                }
                if (charInfo.character == TAG_OPEN) {
                    curTag = "";
                    
                    tagStartPosition = new(idx,vertices[charInfo.vertexIndex]);
                }
                
                prev = charInfo;
                
            }
            if (tagStartPosition != null && prev.character == TAG_CLOSE) {
                if (canTag) {
                    var (start, pos) = tagStartPosition;
                    _fixPoints.Add(new(start, idx + 1, pos));
                }
                tagStartPosition = null;
            }

            while (tagStack.Count > 0) {
                var data = tagStack.Pop();
                data.End = int.MaxValue;
                tempTagPoint.Add(data);
            }
            
            _tagPoints = tempTagPoint.OrderBy(point => point.Start).ToList();
            RemoveTagAndDivideRow();

            bool IsTag() {
                if (curTag.Length == 0)
                    return false;
             
                if (curTag[0] == TAG_END_IDENTIFIER) {
                    if (!Parse<TMP_EffectType>(curTag[1..], out var tag))
                        return false;
                    if (tagStack.Count > 0 && tag == tagStack.Peek().Type) {
                        var data = tagStack.Pop();
                        data.End = tagStartPosition.Idx - 1;
                        tempTagPoint.Add(data);
                    }
                    else
                        return false;
                }
                else {
             
                    var match = Regex.Match(curTag, PATTERN);
                    var tag = TMP_EffectType.Error;
                    var argString = "1";
                    if (match.Success) {
                        if (!Parse(match.Groups["Tag"].Value, out tag))
                            return false;
                        argString = match.Groups["Arg"].Value;
                    }
                    else {
                        if(!Parse(curTag, out tag))
                           return false;
                    }
             
                    if (!float.TryParse(argString, out var arg))
                        arg = 1f;
                    tagStack.Push(new(idx + 1, 0, tag, arg));
                }

                return true;
            }
        }

        private void RemoveTagAndDivideRow() {
            const float DEFAULT_UNITY_ROW_INTERVAL = 0.45f;   
            var textInfo = _text.textInfo;
            var idx = -1;
            var fixPointIdx = 0;
            var fix = Vector3.zero;
            var width = (transform as RectTransform)!.sizeDelta.x;
            var startPos = 0f;
            var newLineCnt = 0;
            var needLineCnt = 0;
            var singleNewLineCnt = 0;
            var line = 0;
            var pad = 0f;
            foreach (var charInfo in textInfo.characterInfo.Take(textInfo.characterCount)) {
                idx++;

                if (idx == 0) {
                    startPos = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices[charInfo.vertexIndex].x;
                }

                if (!charInfo.isVisible) {
                    if (charInfo.character == '\n') {
                        needLineCnt += singleNewLineCnt;
                        newLineCnt++;
                        fix.x = 0;
                    }
                    continue;
                }
            
                var vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            
                var remainSingleTag = _fixPoints.Count > fixPointIdx;
                var peek = new FixPoint(0, 0, Vector3.zero);
                if (remainSingleTag) {
                    peek = _fixPoints[fixPointIdx];
                    if (idx >= peek.End) {
                        fixPointIdx++;
                        fix.x += peek.Pos.x - vertices[charInfo.vertexIndex].x;
                        
                        remainSingleTag = _fixPoints.Count > fixPointIdx;
                        if(remainSingleTag)
                            peek = _fixPoints[fixPointIdx];
                    }    
                }

                var lineFix = Vector3.zero;
                for (int vertex = 0; vertex < 4; vertex++) {
                                            
                    var vertexIdx = charInfo.vertexIndex + vertex;
                            
                    if (remainSingleTag && peek.Start <= idx && idx < peek.End) {
                        vertices[vertexIdx] = peek.Pos;
                    }
                    else {
                        if (vertex == 0) {
                            var prevLine = line;
                            var temp = vertices[vertexIdx] + fix;
                            var dist = Mathf.Max(0, temp.x - startPos);

                            singleNewLineCnt = Mathf.FloorToInt(dist / width);
                            line = singleNewLineCnt + newLineCnt + needLineCnt;
                            var lineFixX = dist % width - dist;
                            if (line != prevLine) {
                                //Set to equal start point
                                pad = startPos - lineFixX - temp.x;
                            }

                            lineFix.x = lineFixX + pad; 
                            lineFix.y = (newLineCnt * (1 + DEFAULT_UNITY_ROW_INTERVAL) - line * (1 + _rowInterval)) * charInfo.pointSize;
                            
                            vertices[vertexIdx] = temp + lineFix;
                        }
                        else
                            vertices[vertexIdx] += fix + lineFix;
                    }
                    
                }
            }
            TMPUpdate();
            _preProcess = true;
            _timer = 0;
        }

        private void TMPUpdate() {
            var textInfo = _text.textInfo;
            foreach (var mesh in textInfo.meshInfo) {
                mesh.mesh.vertices = mesh.vertices;
                mesh.mesh.RecalculateBounds();
            }
            _text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }

        private bool Parse<T>(string pContext, out T value) {
            var result = Enum.TryParse(typeof(T), pContext, out var temp);
            value = result ? (T)temp : default;
            return result;
        }
        
    }
}