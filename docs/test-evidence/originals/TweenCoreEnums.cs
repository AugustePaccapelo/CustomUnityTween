// Author : Auguste Paccapelo

namespace Tweening
{
    // The order of these members is serialized in scenes and prefabs : append, never reorder.

    public enum TweenCoreEase
    {
        In, Out, InOut, OutIn, Custom, CustomCurve
    }

    public enum TweenCoreType
    {
        Linear, Sine, Cubic, Quint, Circ, Elastic, Quad, Quart, Expo, Back, Bounce, Custom, CustomCurve
    }

    public static class TweenCoreTarget
    {
        public static class Transform
        {
            public const string GLOBAL_POSITION = "position";
            public const string LOCAL_POSITION = "localPosition";
            public const string LOCAL_SCALE = "localScale";
            public const string GLOBAL_ROTATION_QUATERNION = "rotation";
            public const string GLOBAL_ROTATION_EULER_ANGLE = "eulerAngles";
            public const string LOCAL_ROTATION_QUATERNION = "localRotation";
            public const string LOCAL_ROTATION_EULER_ANGLE = "localEulerAngles";
        }

        public static class Renderer
        {
            public const string COLOR = "color";
        }
    }
}
