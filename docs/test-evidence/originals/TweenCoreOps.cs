using System;
using UnityEngine;

// Author : Auguste Paccapelo

namespace Tweening
{
    /// <summary>
    /// Rounding and clamping helpers shared by the typed operations below.
    /// Weights can legitimately leave the 0..1 range (Back and Elastic overshoot by design),
    /// so every bounded type has to clamp rather than trust the weight.
    /// </summary>
    internal static class TweenCoreOps
    {
        public static int LerpInt(int a, int b, float w)
        {
            double v = Math.Round(a + ((double)b - a) * w, MidpointRounding.AwayFromZero);
            if (v <= int.MinValue) return int.MinValue;
            if (v >= int.MaxValue) return int.MaxValue;
            return (int)v;
        }

        public static uint LerpUInt(uint a, uint b, float w)
        {
            // Computed in double on purpose : (uint)b - (uint)a wraps when b < a.
            double v = Math.Round(a + ((double)b - a) * w, MidpointRounding.AwayFromZero);
            if (v <= uint.MinValue) return uint.MinValue;
            if (v >= uint.MaxValue) return uint.MaxValue;
            return (uint)v;
        }

        public static long LerpLong(long a, long b, float w)
        {
            double v = Math.Round(a + ((double)b - a) * w, MidpointRounding.AwayFromZero);
            if (v <= long.MinValue) return long.MinValue;
            if (v >= long.MaxValue) return long.MaxValue;
            return (long)v;
        }

        public static ulong LerpULong(ulong a, ulong b, float w)
        {
            double v = Math.Round(a + ((double)b - a) * w, MidpointRounding.AwayFromZero);
            if (v <= ulong.MinValue) return ulong.MinValue;
            if (v >= ulong.MaxValue) return ulong.MaxValue;
            return (ulong)v;
        }

        public static byte LerpByte(byte a, byte b, float w)
        {
            double v = Math.Round(a + ((double)b - a) * w, MidpointRounding.AwayFromZero);
            if (v <= byte.MinValue) return byte.MinValue;
            if (v >= byte.MaxValue) return byte.MaxValue;
            return (byte)v;
        }

        public static byte AddByte(byte a, byte b)
        {
            int v = a + b;
            return v >= byte.MaxValue ? byte.MaxValue : (byte)v;
        }
    }

    /// <summary>
    /// Interpolation and addition for one value type, resolved once per closed generic type
    /// by the static constructor instead of once per frame by a dictionary lookup.
    /// Nothing here boxes : the delegates are strongly typed all the way down.
    /// </summary>
    /// <typeparam name="TweenValueType">The type of value (e.g. float, Vector3, ...).</typeparam>
    public static class TweenCoreOps<TweenValueType>
    {
        /// <summary>Interpolates between two values. Null when the type is not supported.</summary>
        public static readonly Func<TweenValueType, TweenValueType, float, TweenValueType> Lerp;

        /// <summary>Adds two values, used by SetIsAdditive. Null when the type has no meaningful addition.</summary>
        public static readonly Func<TweenValueType, TweenValueType, TweenValueType> Add;

        /// <summary>True when this type can be tweened at all.</summary>
        public static bool IsSupported => Lerp != null;

        /// <summary>True when this type can be used with SetIsAdditive.</summary>
        public static bool SupportsAdditive => Add != null;

        static TweenCoreOps()
        {
            object lerp = null;
            object add = null;

            Type type = typeof(TweenValueType);

            // ----- C# types ----- \\

            if (type == typeof(float))
            {
                lerp = (Func<float, float, float, float>)((a, b, w) => a + (b - a) * w);
                add = (Func<float, float, float>)((a, b) => a + b);
            }
            else if (type == typeof(double))
            {
                lerp = (Func<double, double, float, double>)((a, b, w) => a + (b - a) * w);
                add = (Func<double, double, double>)((a, b) => a + b);
            }
            else if (type == typeof(int))
            {
                lerp = (Func<int, int, float, int>)TweenCoreOps.LerpInt;
                add = (Func<int, int, int>)((a, b) => a + b);
            }
            else if (type == typeof(uint))
            {
                lerp = (Func<uint, uint, float, uint>)TweenCoreOps.LerpUInt;
                add = (Func<uint, uint, uint>)((a, b) => a + b);
            }
            else if (type == typeof(long))
            {
                lerp = (Func<long, long, float, long>)TweenCoreOps.LerpLong;
                add = (Func<long, long, long>)((a, b) => a + b);
            }
            else if (type == typeof(ulong))
            {
                lerp = (Func<ulong, ulong, float, ulong>)TweenCoreOps.LerpULong;
                add = (Func<ulong, ulong, ulong>)((a, b) => a + b);
            }
            else if (type == typeof(decimal))
            {
                lerp = (Func<decimal, decimal, float, decimal>)((a, b, w) => a + (b - a) * (decimal)w);
                add = (Func<decimal, decimal, decimal>)((a, b) => a + b);
            }

            // ----- Unity types ----- \\

            else if (type == typeof(Vector2))
            {
                lerp = (Func<Vector2, Vector2, float, Vector2>)((a, b, w) => a + (b - a) * w);
                add = (Func<Vector2, Vector2, Vector2>)((a, b) => a + b);
            }
            else if (type == typeof(Vector3))
            {
                lerp = (Func<Vector3, Vector3, float, Vector3>)((a, b, w) => a + (b - a) * w);
                add = (Func<Vector3, Vector3, Vector3>)((a, b) => a + b);
            }
            else if (type == typeof(Vector4))
            {
                lerp = (Func<Vector4, Vector4, float, Vector4>)((a, b, w) => a + (b - a) * w);
                add = (Func<Vector4, Vector4, Vector4>)((a, b) => a + b);
            }
            else if (type == typeof(Quaternion))
            {
                // Unclamped so Back and Elastic can overshoot on rotations like they do everywhere else.
                lerp = (Func<Quaternion, Quaternion, float, Quaternion>)Quaternion.LerpUnclamped;
                // "Adding" a rotation is composing it.
                add = (Func<Quaternion, Quaternion, Quaternion>)((a, b) => a * b);
            }
            else if (type == typeof(Color))
            {
                lerp = (Func<Color, Color, float, Color>)((a, b, w) => a + (b - a) * w);
                add = (Func<Color, Color, Color>)((a, b) => a + b);
            }
            else if (type == typeof(Color32))
            {
                lerp = (Func<Color32, Color32, float, Color32>)((a, b, w) => new Color32(
                    TweenCoreOps.LerpByte(a.r, b.r, w),
                    TweenCoreOps.LerpByte(a.g, b.g, w),
                    TweenCoreOps.LerpByte(a.b, b.b, w),
                    TweenCoreOps.LerpByte(a.a, b.a, w)));
                add = (Func<Color32, Color32, Color32>)((a, b) => new Color32(
                    TweenCoreOps.AddByte(a.r, b.r),
                    TweenCoreOps.AddByte(a.g, b.g),
                    TweenCoreOps.AddByte(a.b, b.b),
                    TweenCoreOps.AddByte(a.a, b.a)));
            }

            Lerp = (Func<TweenValueType, TweenValueType, float, TweenValueType>)lerp;
            Add = (Func<TweenValueType, TweenValueType, TweenValueType>)add;
        }
    }
}
