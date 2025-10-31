using System;
using System.Collections.Generic;
using TMPro;

// Author : Auguste Paccapelo

public class Tween
{
    // ---------- VARIABLES ---------- \\

    // ----- Objects ----- \\

    private List<TweenPropertyBase> _tweenProperties = new List<TweenPropertyBase>();

    // ----- Others ----- \\

    private bool _isPaused = true;
    private bool _hasStarted = false;

    private bool _isParallel = true;

    public event Action OnStart;
    public event Action OnFinish;

    // ---------- FUNCTIONS ---------- \\

    public Tween Update(float deltaTime)
    {
        if (_isPaused) return this;
        
        for (int i = _tweenProperties.Count - 1; i >= 0; i--)
        {
            _tweenProperties[i].Update(deltaTime);
        }
        if (_tweenProperties.Count == 0) Stop();

        return this;
    }

    public Tween Pause()
    {
        _isPaused = true;

        return this;
    }

    public Tween Resume()
    {
        _isPaused = false;

        return this;
    }

    public Tween Play()
    {
        if (_hasStarted) return this;

        _hasStarted = true;
        _isPaused = false;

        TweenPropertyBase property;
        int length = _tweenProperties.Count-1;

        for (int i = length; i >= 1; i--)
        {
            property = _tweenProperties[i];
            if (_isParallel) property.Start();
            else
            {
                _tweenProperties[i-1].AddNextProperty(property);
            }
        }
        if (!_isParallel) _tweenProperties[0].Start();

        OnStart?.Invoke();

        return this;
    }

    public Tween Stop()
    {
        OnFinish?.Invoke();
        foreach (TweenPropertyBase property in _tweenProperties)
        {
            property.Stop();
        }
        TweenManager.Instance.RemoveTween(this);

        return this;
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

    public Tween SetParallel(bool isParallel)
    {
        _isParallel = isParallel;
        return this;
    }

    public Tween SetChain(bool isChain)
    {
        _isParallel = !isChain;
        return this;
    }

    public Tween Parallel()
    {
        _isParallel = true;
        return this;
    }

    public Tween Chain()
    {
        _isParallel = false;
        return this;
    }
}