// Author : Auguste Paccapelo

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