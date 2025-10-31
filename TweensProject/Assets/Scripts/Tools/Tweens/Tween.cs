using System;
using System.Collections.Generic;

// Author : Auguste Paccapelo

public class Tween
{
    // ---------- VARIABLES ---------- \\

    // ----- Objects ----- \\

    private List<TweenPropertyBase> _tweenProperties = new List<TweenPropertyBase>();

    // ----- Others ----- \\

    private float _elapseTime = 0f;

    private bool _isPaused = true;
    private bool _hasStarted = false;

    public event Action OnStart;
    public event Action OnFinish;

    // ---------- FUNCTIONS ---------- \\

    public void Update(float deltaTime)
    {
        if (_isPaused) return;
        
        _elapseTime += deltaTime;
        for (int i = _tweenProperties.Count - 1; i >= 0; i--)
        {
            _tweenProperties[i].Update(_elapseTime);
        }
        if (_tweenProperties.Count == 0) Stop();
    }

    public void Pause()
    {
        _isPaused = true;
    }

    public void Resume()
    {
        _isPaused = false;
    }

    public void Play()
    {
        if (_hasStarted) return;

        _hasStarted = true;
        _isPaused = false;
        foreach (TweenPropertyBase property in _tweenProperties)
        {
            property.Start();
        }
        OnStart?.Invoke();
    }

    public void Stop()
    {
        OnFinish?.Invoke();
        foreach (TweenPropertyBase property in _tweenProperties)
        {
            property.Stop();
        }
        TweenManager.Instance.RemoveTween(this);
    }

    public static Tween CreateTween()
    {
        Tween tween = new Tween();
        TweenManager.Instance.AddTween(tween);
        return tween;
    }

    public TweenProperty<ValueType> NewProperty<ValueType>(Action<ValueType> function, ValueType startVal, ValueType finalVal, float time)
    {
        TweenProperty<ValueType> property = new TweenProperty<ValueType>(function, startVal, finalVal, time, this);
        _tweenProperties.Add(property);
        return property;
    }

    public TweenProperty<ValueType> NewProperty<ValueType>(UnityEngine.Object obj, string method, ValueType finalVal, float time)
    {
        TweenProperty<ValueType> property = new TweenProperty<ValueType>(obj, method, finalVal, time, this);
        _tweenProperties.Add(property);
        return property;
    }

    public TweenProperty<ValueType> NewProperty<ValueType>(UnityEngine.Object obj, string method,ValueType startVal, ValueType finalVal, float time)
    {
        TweenProperty<ValueType> property = new TweenProperty<ValueType>(obj, method, startVal, finalVal, time, this);
        _tweenProperties.Add(property);
        return property;
    }

    public void StopProperty(TweenPropertyBase property)
    {
        _tweenProperties.Remove(property);
    }
}