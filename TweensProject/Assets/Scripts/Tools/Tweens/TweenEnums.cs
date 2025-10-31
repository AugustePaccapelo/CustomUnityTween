// Author : Auguste Paccapelo
using UnityEngine;
public enum TweenEase
{
    In, Out, InOut, OutIn, Custom
}

public enum TweenType
{
    Linear, Sine, Cubic, Quint, Circ, Elastic, Quad, Quart, Expo, Back, Bounce, Custom
}

public static class TweenTarget
{
    public static class Transform
    {
        public const string POSITION = "position";
        public const string LOCAL_SCALE = "localScale";
        public const string ROTATION_QUATERNION = "rotation";
        public const string ROTATION_EULER_ANGLE = "eulerAngles";
    }

    public static class Renderer
    {
        public const string COLOR = "color";
    }
} 