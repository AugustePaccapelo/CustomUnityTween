using System;
using System.Reflection;
using UnityEngine;

// Author : Auguste Paccapelo

public class TweenProperty<ValueType> : TweenPropertyBase
{
    // ---------- VARIABLES ---------- \\
    
    // ----- Prefabs & Assets ----- \\

    // ----- Objects ----- \\

    // ----- Others ----- \\

    private ValueType _finalValue;
    private ValueType _startValue;
    private ValueType _currentValue;
    public ValueType CurrentValue => _currentValue;    

    private UnityEngine.Object _obj;

    private bool _isPlaying = false;

    private MethodUse _currentMethod;
    private PropertyInfo _property;
    private FieldInfo _field;
    private Action<ValueType> _function;

    private Tween _myTween;

    public event Action OnFinish;
    public event Action OnStart;

    // ---------- FUNCTIONS ---------- \\

    public TweenProperty(Action<ValueType> function, ValueType startVal, ValueType finalVal, float time, Tween tween)
    {
        _currentMethod = MethodUse.Strategy;
        _startValue = startVal;
        _finalValue = finalVal;
        _myTween = tween;
        SetType(type);
        SetEase(ease);
        _function = function;
    }

    public TweenProperty(UnityEngine.Object obj, string method, ValueType finalVal, float duration, Tween tween)
    {
        _currentMethod = MethodUse.Reflexion;
        _finalValue = finalVal;
        time = duration;
        _myTween = tween;
        SetType(type);
        SetEase(ease);

        _obj = obj;
        SetReflexionFiels(method);

        _startValue = GetObjValue();
    }

    public TweenProperty(UnityEngine.Object obj, string method, ValueType startVal, ValueType finalVal, float duration, Tween tween)
    {
        _currentMethod = MethodUse.Reflexion;
        _finalValue = finalVal;
        time = duration;
        _myTween = tween;
        SetType(type);
        SetEase(ease);

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
            Stop();
            throw new Exception("No property or field found");
        }
    }

    public override void Start()
    {
        if (_isPlaying) return;

        _isPlaying = true;
        OnStart?.Invoke();
    }

    public override void Update(float elapseTime)
    {
        if (!_isPlaying) return;

        elapseTime = Mathf.Clamp(elapseTime - delay, 0, time);
        float w = Mathf.Clamp01(elapseTime / time);
        w = RealWeight(w);
        if (lerpsFunc.ContainsKey(typeof(ValueType)))
        {
            _currentValue = (ValueType)lerpsFunc[typeof(ValueType)](_startValue, _finalValue, w);
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

        if (elapseTime >= time + delay) Stop();
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

    public TweenProperty<ValueType> SetDelay(float tweenDelay)
    {
        delay = tweenDelay;
        return this;
    }

    public TweenProperty<ValueType> SetType(TweenType newType)
    {
        type = newType;
        SetTypeFunc(type);
        return this;
    }

    public TweenProperty<ValueType> SetType(Func<float, float> customType)
    {
        TypeFunc = customType;
        type = TweenType.Custom;
        return this;
    }

    public TweenProperty<ValueType> SetEase(TweenEase newEase)
    {
        ease = newEase;
        SetEaseFunc(ease);
        return this;
    }

    public TweenProperty<ValueType> SetEase(Func<float, Func<float, float>, float> customEase)
    {
        ease = TweenEase.Custom;
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

    public void Pause()
    {
        _isPlaying = false;
    }

    public void Resume()
    {
        _isPlaying = true;
    }

    public override void Stop()
    {
        _isPlaying = false;
        OnFinish?.Invoke();
        DestroyProperty();
    }

    private void DestroyProperty()
    {
        _myTween.StopProperty(this);
    }
}