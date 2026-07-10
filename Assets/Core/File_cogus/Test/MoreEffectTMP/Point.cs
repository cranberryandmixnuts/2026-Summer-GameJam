using UnityEngine;

namespace System.Runtime.CompilerServices { public class IsExternalInit { } }

namespace Extension {
    public partial class MoreEffectTMP {
        //==================================================||SubType 
        private record StartPoint(
            int Idx,
            Vector3 Pos
        );  

        private class FixPoint {
            public int Start;
            public int End;
            public Vector3 Pos;
        
            public FixPoint(int pStart, int pEnd, Vector3 pPos) =>
                (Start, End, Pos) = (pStart, pEnd, pPos);
        }
        
        private class TagPoint {
            public int Start;
            public int End;
            public TMP_EffectType Type;
            public float Arg;
        
            public TagPoint(int pStart, int pEnd, TMP_EffectType pType, float pArg) =>
                (Start, End, Type, Arg) = (pStart, pEnd, pType, pArg);
        }
        private enum TMP_EffectType {
            Error = 0,
            Flow,
            Shake,
            Rotate
        }
    }
}