using UnityEngine;

public partial class MoreEffectTMP
{
    private sealed class StartPoint
    {
        public int Idx { get; }
        public Vector3 Pos { get; }

        public StartPoint(int pIdx, Vector3 pPos) => (Idx, Pos) = (pIdx, pPos);

        public void Deconstruct(out int pIdx, out Vector3 pPos) => (pIdx, pPos) = (Idx, Pos);
    }

    private sealed class FixPoint
    {
        public int Start;
        public int End;
        public Vector3 Pos;

        public FixPoint(int pStart, int pEnd, Vector3 pPos) =>
            (Start, End, Pos) = (pStart, pEnd, pPos);
    }

    private sealed class TagPoint
    {
        public int Start;
        public int End;
        public TMP_EffectType Type;
        public float Arg;

        public TagPoint(int pStart, int pEnd, TMP_EffectType pType, float pArg) =>
            (Start, End, Type, Arg) = (pStart, pEnd, pType, pArg);
    }

    private enum TMP_EffectType
    {
        Error = 0,
        Flow,
        Shake,
        Rotate
    }
}
