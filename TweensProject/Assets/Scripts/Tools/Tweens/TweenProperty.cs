using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

// Author : Auguste Paccapelo

public class TweenProperty<ValueType> : ITweenProperty
{
    private enum MethodUse
    {
        Reflexion, Strategy
    }

    // ---------- VARIABLES ---------- \\
    
    // ----- Prefabs & Assets ----- \\

    // ----- Objects ----- \\

    // ----- Others ----- \\

    private TweenType _type = TweenType.Linear;
    private TweenEase _ease = TweenEase.In;

    private float _time = 1f;
    private ValueType _finalValue;
    private ValueType _startValue;
    private ValueType _currentValue;
    public ValueType CurrentValue => _currentValue;
    private Func<float, Func<float, float>, float> EaseFunc;
    private Func<float, float> TypeFunc;

    private const float BACK_C1 = 1.70158f;
    private const float BACK_C3 = BACK_C1 + 1;
    private const float ELASTIC_C4 = (2 * Mathf.PI) / 3;
    private const float BOUNCE_N1 = 7.5625f;
    private const float BOUNCE_D1 = 2.75f;

    private UnityEngine.Object _obj;

    public bool isPlaying = true;

    private MethodUse _currentMethod;
    private PropertyInfo _property;
    private FieldInfo _field;
    private Action<ValueType> _function;

    private Tween _myTween;

    public event Action OnFinish;

    private static readonly Dictionary<Type, Func<object, object, float, object>> _lerpsFunc = new Dictionary<Type, Func<object, object, float, object>>()
    {
        // C# types
        {typeof(float), (a, b, t) => (float)a + ((float)b - (float)a) * t},
        {typeof(double), (a, b, t) => (double)a + ((double)b - (double)a) * t},
        {typeof(int), (a, b, t) => (int)a + ((int)b - (int)a) * t },
        {typeof(uint), (a, b, t) => (uint)a + ((uint)b - (uint)a) * t },
        {typeof(long), (a, b, t) => (long)a + ((long)b - (long)a) * t },
        {typeof(ulong), (a, b, t) => (ulong)a + ((ulong)b - (ulong)a) * t },
        {typeof(decimal), (a, b, t) => (decimal)a + ((decimal)b - (decimal)a) * (decimal)t },
        // Unity types
        {typeof(Vector2), (a, b, t) => (Vector2)a + ((Vector2)b - (Vector2)a) * t },
        {typeof(Vector3), (a, b, t) => (Vector3)a + ((Vector3)b - (Vector3)a) * t },
        {typeof(Vector4), (a, b, t) => (Vector4)a + ((Vector4)b - (Vector4)a) * t },
        {typeof(Quaternion), (a, b, t) => Quaternion.Lerp((Quaternion)a, (Quaternion)b, t)},
        {typeof(Color), (a, b, t) => (Color)a + ((Color)b - (Color)a) * t },
        {typeof(Color32), (a, b, t) => Color32.Lerp((Color32)a, (Color32)b, t)},
    };

    // ---------- FUNCTIONS ---------- \\

    public TweenProperty(Action<ValueType> function, ValueType startVal, ValueType finalVal, float time, Tween tween)
    {
        _currentMethod = MethodUse.Strategy;
        _startValue = startVal;
        _finalValue = finalVal;
        _myTween = tween;
        SetType(_type);
        SetEase(_ease);
        _function = function;
    }

    public TweenProperty(UnityEngine.Object obj, string method, ValueType finalVal, float time, Tween tween)
    {
        _currentMethod = MethodUse.Reflexion;
        _finalValue = finalVal;
        _time = time;
        _myTween = tween;
        SetType(_type);
        SetEase(_ease);

        _obj = obj;
        SetReflexionFiels(method);

        _startValue = GetObjValue();
    }

    public TweenProperty(UnityEngine.Object obj, string method, ValueType startVal, ValueType finalVal, float time, Tween tween)
    {
        _currentMethod = MethodUse.Reflexion;
        _finalValue = finalVal;
        _time = time;
        _myTween = tween;
        SetType(_type);
        SetEase(_ease);

        _obj = obj;
        SetReflexionFiels(method);

        _startValue = startVal;
    }

    private void SetReflexionFiels(string method)
    {
        _property = _obj.GetType().GetProperty(method);
        if (_property == null)
            _field = _obj.GetType().GetField(method);
        if (_property == null && _field == null)
        {
            DestroyProperty();
            throw new Exception("No property or field found");
        }
    }

    public void Update(float elapseTime)
    {
        if (!isPlaying) return;
        elapseTime = Mathf.Clamp(elapseTime, 0, _time);
        float w = Mathf.Clamp01(elapseTime / _time);
        w = RealWeight(w);
        if (_lerpsFunc.ContainsKey(typeof(ValueType)))
        {
            _currentValue = (ValueType)_lerpsFunc[typeof(ValueType)](_startValue, _finalValue, w);
        }
        else
        { 
            throw new ArgumentException("The ValueType given is not supported (" + typeof(ValueType) + ").");
        }

        switch (_currentMethod)
        {
            case MethodUse.Reflexion:
                ReflexionMethod();
                break;
            case MethodUse.Strategy:
                StrategyMethod();
                break;
            default:
                throw new NotImplementedException();
        }

        if (elapseTime >= _time) DestroyProperty();
    }

    private void StrategyMethod()
    {
        _function.Invoke(_currentValue);
    }

    private void ReflexionMethod()
    {
        if (_property != null) _property.SetValue(_obj, _currentValue);
        else _field.SetValue(_obj, _currentValue);
    }

    private ValueType GetObjValue()
    {
        object value = _property != null ? _property.GetValue(_obj) : _field.GetValue(_obj);
        return (ValueType)value;
    }

    public TweenProperty<ValueType> SetType(TweenType newType)
    {
        _type = newType;
        SetTypeFunc(_type);
        return this;
    }

    public TweenProperty<ValueType> SetType(Func<float, float> customType)
    {
        TypeFunc = customType;
        _type = TweenType.Custom;
        return this;
    }

    public TweenProperty<ValueType> SetEase(TweenEase newEase)
    {
        _ease = newEase;
        SetEaseFunc(_ease);
        return this;
    }

    public TweenProperty<ValueType> SetEase(Func<float, Func<float, float>, float> customEase)
    {
        _ease = TweenEase.Custom;
        EaseFunc = customEase;
        return this;
    }

    public TweenProperty<ValueType> From(ValueType value)
    {
        _startValue = value;
        return this;
    }

    private float RealWeight(float w)
    {
        return EaseFunc(w, TypeFunc);
    }

    private void DestroyProperty()
    {
        OnFinish?.Invoke();
        _myTween.StopProperty(this);
    }

    private void SetTypeFunc(TweenType newType)
    {
        switch (newType)
        {
            case TweenType.Linear:
                TypeFunc = Linear;
                break;
            case TweenType.Quad:
                TypeFunc = Quad;
                break;
            case TweenType.Cubic:
                TypeFunc = Cubic;
                break;
            case TweenType.Quart:
                TypeFunc = Quart;
                break;
            case TweenType.Quint:
                TypeFunc = Quint;
                break;
            case TweenType.Back:
                TypeFunc = Back;
                break;
            case TweenType.Elastic:
                TypeFunc = Elastic;
                break;
            case TweenType.Bounce:
                TypeFunc = Bounce;
                break;
            case TweenType.Circ:
                TypeFunc = Circ;
                break;
            case TweenType.Sine:
                TypeFunc = Sine;
                break;
            case TweenType.Expo:
                TypeFunc = Expo;
                break;
        }
    }

    private void SetEaseFunc(TweenEase newEase)
    {
        switch (newEase)
        {
            case TweenEase.In:
                EaseFunc = In;
                break;
            case TweenEase.Out:
                EaseFunc = Out;
                break;
            case TweenEase.InOut:
                EaseFunc = InOut;
                break;
            case TweenEase.OutIn:
                EaseFunc = OutIn;
                break;
        }
    }

    // Types \\

    private float Linear(float t)
    {
        return t;
    }

    private float Quad(float t)
    {
        return t * t;
    }

    private float Cubic(float t)
    {
        return t * t * t;
    }

    private float Quart(float t)
    {
        return t * t * t * t;
    }

    private float Quint(float t)
    {
        return t * t * t * t * t;
    }

    private float Back(float t)
    {
        return BACK_C3 * t * t * t - BACK_C1 * t * t;
    }

    private float Elastic(float t)
    {
        if (t == 0f) return 0f;
        if (t == 1f) return 1f;

        return -Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((float)(t * 10 - 10.75) * ELASTIC_C4);
    }

    private float Bounce(float t)
    {
        if (t < 1f / BOUNCE_D1) return BOUNCE_N1 * t * t;
        else if (t < 2f / BOUNCE_D1)
        {
            t -= 1.5f / BOUNCE_D1;
            return BOUNCE_N1 * t * t + 0.75f;
        }
        else if (t < 2.5f / BOUNCE_D1)
        {
            t -= 2.25f / BOUNCE_D1;
            return BOUNCE_N1 * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / BOUNCE_D1;
            return BOUNCE_N1 * t * t + 0.984375f;
        }
    }

    private float Circ(float t)
    {
        return 1 - Mathf.Sqrt(1 - t * t);
    }

    private float Sine(float t)
    {
        return 1 - Mathf.Cos((t * Mathf.PI) / 2);
    }

    private float Expo(float t)
    {
        if (t == 0) return 0f;
        return Mathf.Pow(2, 10 * t - 10);
    }

    // Eases \\

    private float In(float t, Func<float, float> TypeFunc)
    {
        return TypeFunc(t);
    }

    private float Out(float t, Func<float, float> TypeFunc)
    {
        return 1 - TypeFunc(1 - t);
    }

    private float InOut(float t, Func<float, float> TypeFunc)
    {
        return t < 0.5f ?
            0.5f * TypeFunc(t * 2) :
            1 - 0.5f * TypeFunc(2 - 2 * t);
    }

    private float OutIn(float t, Func<float, float> TypeFunc)
    {
        return t > 0.5f ?
            0.5f * TypeFunc(t * 2) :
            1 - 0.5f * TypeFunc(2 - 2 * t);
    }
}