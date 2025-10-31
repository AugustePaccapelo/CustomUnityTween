using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

// Author : Auguste Paccapelo

public class TweenProperty<TweenValueType> : TweenPropertyBase
{
    // ---------- VARIABLES ---------- \\
    
    // ----- Prefabs & Assets ----- \\

    // ----- Objects ----- \\

    // ----- Others ----- \\

    private TweenValueType _finalValue;
    private TweenValueType _startValue;
    private TweenValueType _currentValue;
    public TweenValueType CurrentValue => _currentValue;    

    private UnityEngine.Object _obj;

    private bool _isPlaying = false;

    private MethodUse _currentMethod;
    private PropertyInfo _property;
    private FieldInfo _field;
    private Action<TweenValueType> _function;

    private Tween _myTween;

    private List<TweenPropertyBase> _nextProperties = new List<TweenPropertyBase>();

    public event Action<TweenValueType> OnUpdate;

    // ---------- FUNCTIONS ---------- \\

    public TweenProperty(TweenValueType startVal, TweenValueType finalVal, float time, Tween tween)
    {
        _currentMethod = MethodUse.ReturnValue;

        SetBaseVal(finalVal, time, tween);

        _startValue = startVal;
    }

    public TweenProperty(Action<TweenValueType> function, TweenValueType startVal, TweenValueType finalVal, float duration, Tween tween)
    {
        _currentMethod = MethodUse.Strategy;

        SetBaseVal(finalVal, duration, tween);

        _startValue = startVal;
        _function = function;
    }

    public TweenProperty(UnityEngine.Object obj, string method, TweenValueType finalVal, float duration, Tween tween)
    {
        _currentMethod = MethodUse.Reflexion;
        
        SetBaseVal(finalVal, duration, tween);

        _obj = obj;
        SetReflexionFiels(method);

        _startValue = GetObjValue();
    }

    public TweenProperty(UnityEngine.Object obj, string method, TweenValueType startVal, TweenValueType finalVal, float duration, Tween tween)
    {
        _currentMethod = MethodUse.Reflexion;

        SetBaseVal(finalVal, duration, tween);

        _obj = obj;
        SetReflexionFiels(method);
        _startValue = startVal;
    }

    private void SetBaseVal(TweenValueType finalVal, float duration, Tween tween)
    {
        _finalValue = finalVal;
        time = duration;
        _myTween = tween;
        SetType(type);
        SetEase(ease);
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
        TriggerOnStart();
    }

    public override void Update(float deltaTime)
    {
        if (!_isPlaying) return;

        _elapseTime += deltaTime;
        if (_elapseTime <= delay) return;

        float elapse = Mathf.Clamp(_elapseTime - delay, 0, time);
        float w = Mathf.Clamp01(elapse / time);
        w = RealWeight(w);
        if (lerpsFunc.ContainsKey(typeof(TweenValueType)))
        {
            _currentValue = (TweenValueType)lerpsFunc[typeof(TweenValueType)](_startValue, _finalValue, w);
            OnUpdate?.Invoke(_currentValue);
        }
        else
        { 
            throw new ArgumentException("The ValueType given is not supported (" + typeof(TweenValueType) + ").");
        }

        switch (_currentMethod)
        {
            case MethodUse.Reflexion:
                ReflexionMethod();
                break;
            case MethodUse.Strategy:
                StrategyMethod();
                break;
            case MethodUse.ReturnValue:
                break;
            default:
                throw new NotImplementedException();
        }

        if (elapse >= time) Stop();
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

    private TweenValueType GetObjValue()
    {
        object value = _property != null ? _property.GetValue(_obj) : _field.GetValue(_obj);
        return (TweenValueType)value;
    }

    public TweenProperty<TweenValueType> SetDelay(float tweenDelay)
    {
        delay = tweenDelay;
        return this;
    }

    public TweenProperty<TweenValueType> SetType(TweenType newType)
    {
        type = newType;
        SetTypeFunc(type);
        return this;
    }

    public TweenProperty<TweenValueType> SetCustomType(Func<float, float> customType)
    {
        TypeFunc = customType;
        type = TweenType.Custom;
        return this;
    }

    public TweenProperty<TweenValueType> SetEase(TweenEase newEase)
    {
        ease = newEase;
        SetEaseFunc(ease);
        return this;
    }

    public TweenProperty<TweenValueType> SetCustomEase(Func<float, Func<float, float>, float> customEase)
    {
        ease = TweenEase.Custom;
        EaseFunc = customEase;
        return this;
    }

    public TweenValueType GetCurrentValue() => CurrentValue;

    public TweenProperty<TweenValueType> From(TweenValueType value)
    {
        _startValue = value;
        return this;
    }

    public override TweenPropertyBase AddNextProperty(TweenPropertyBase property)
    {
        _nextProperties.Add(property);
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
        TriggerOnFinish();
        int length = _nextProperties.Count - 1;
        for (int i = length; i >= 0; i--)
        {
            _nextProperties[i].Start();
            _nextProperties.RemoveAt(i);
        }
        
        DestroyProperty();
    }

    private void DestroyProperty()
    {
        _myTween.StopProperty(this);
    }
}