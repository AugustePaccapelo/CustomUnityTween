using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting.APIUpdating;

// Author : Auguste Paccapelo

namespace Tweening
{
    [MovedFrom(autoUpdateAPI: true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: null)]
    [Serializable]
    public class TweenCoreProperty<TweenValueType> : TweenCorePropertyBase
    {
        // ---------- VARIABLES ---------- \\

        // ----- Others ----- \\

        [SerializeField] private TweenValueType _startValue;
        public TweenValueType StartValue => _startValue;

        [SerializeField] private TweenValueType _finalValue;
        public TweenValueType FinalValue => _finalValue;

        [SerializeField] private TweenValueType _increaseValue;
        public TweenValueType IncreasingValue => _increaseValue;

        private TweenValueType _currentValue;
        public TweenValueType CurrentValue => _currentValue;

        private MethodUse _currentMethod;
        private PropertyInfo _property;
        private FieldInfo _field;
        private Action<TweenValueType> _function;

        // Closed delegates bound to obj once, instead of a reflection call every frame.
        private Action<TweenValueType> _setter;
        private Func<TweenValueType> _getter;

        // True when the target could not be resolved : the property stays inert instead of
        // throwing once per frame for the rest of the tween.
        private bool _isBroken;
        public bool IsBroken => _isBroken;

        private List<TweenCorePropertyBase> _nextProperties = new List<TweenCorePropertyBase>();

        [Serializable]
        private class TweenPropertyUnityEvents
        {
            [SerializeField] public UnityEvent<TweenCoreProperty<TweenValueType>> unityOnStart;
            [SerializeField] public UnityEvent<TweenCoreProperty<TweenValueType>> unityOnUpdate;
            [SerializeField] public UnityEvent<TweenCoreProperty<TweenValueType>, TweenValueType> unityOnUpdateValue;
            [SerializeField] public UnityEvent<TweenCoreProperty<TweenValueType>> unityOnFinish;
        }

        [SerializeField] private TweenPropertyUnityEvents _unityEvents = new TweenPropertyUnityEvents();

        public event Action<TweenCoreProperty<TweenValueType>, TweenValueType> OnUpdateValue;

        // ---------- FUNCTIONS ---------- \\

        /// <summary>
        /// Constructor of TweenProperty for when not modifying a property or field.
        /// </summary>
        /// <param name="startVal">The start value.</param>
        /// <param name="finalVal">The end value.</param>
        /// <param name="time">The duration.</param>
        public TweenCoreProperty(TweenValueType startVal, TweenValueType finalVal, float time)
        {
            _currentMethod = MethodUse.ReturnValue;

            SetCommonValues(finalVal, time);

            _startValue = startVal;

            fromCurrentValue = false;
        }

        /// <summary>
        /// Constructor of TweenProperty when using a function to update a property or field.
        /// </summary>
        /// <param name="function">The function to call each Updates.</param>
        /// <param name="startVal">The start value.</param>
        /// <param name="finalVal">The end value.</param>
        /// <param name="duration">The duration.</param>
        public TweenCoreProperty(Action<TweenValueType> function, TweenValueType startVal, TweenValueType finalVal, float duration)
        {
            _currentMethod = MethodUse.Strategy;

            SetCommonValues(finalVal, duration);

            _startValue = startVal;
            _function = function;

            fromCurrentValue = false;
        }

        /// <summary>
        /// Constructor of TweenProperty using reflection, with the current value as startValue.
        /// </summary>
        /// <param name="obj">The targeted object.</param>
        /// <param name="method">The targeted property or field.</param>
        /// <param name="finalVal">The end value.</param>
        /// <param name="duration">The duration.</param>
        public TweenCoreProperty(UnityEngine.Object obj, string method, TweenValueType finalVal, float duration)
        {
            _currentMethod = MethodUse.Reflection;

            SetCommonValues(finalVal, duration, method);

            base.obj = obj;
            SetReflectionFields(propertyName);

            fromCurrentValue = true;
        }

        /// <summary>
        /// Constructor of TweenProperty using reflection.
        /// </summary>
        /// <param name="obj">The targeted object.</param>
        /// <param name="method">The targeted property or field.</param>
        /// <param name="startVal">The start value.</param>
        /// <param name="finalVal">The end value.</param>
        /// <param name="duration">The duration.</param>
        public TweenCoreProperty(UnityEngine.Object obj, string method, TweenValueType startVal, TweenValueType finalVal, float duration)
        {
            _currentMethod = MethodUse.Reflection;

            SetCommonValues(finalVal, duration, method);

            base.obj = obj;
            SetReflectionFields(propertyName);
            _startValue = startVal;

            fromCurrentValue = false;
        }

        /// <summary>
        /// An empty constructor just to create an object.
        /// Properties and fields are assigned in the editor.
        /// </summary>
        public TweenCoreProperty()
        {
            _currentMethod = MethodUse.Reflection;
        }

        public override TweenCorePropertyBase SetBaseValues()
        {
            RequireSupportedType();

            if (type == TweenCoreType.CustomCurve)
            {
                SetType(typeAnimationCurve);
            }
            else
            {
                SetType(type);
            }

            if (ease == TweenCoreEase.CustomCurve)
            {
                SetEase(easeAnimationCurve);
            }
            else
            {
                SetEase(ease);
            }

            if (isEmpty)
            {
                _currentMethod = MethodUse.ReturnValue;
            }
            else
            {
                SetReflectionFields(propertyName);
            }

            return this;
        }

        private static void RequireSupportedType()
        {
            if (TweenCoreOps<TweenValueType>.IsSupported) return;

            throw new NotSupportedException(
                $"{nameof(TweenCoreProperty<TweenValueType>)} : the value type {typeof(TweenValueType)} is not supported. " +
                "Supported types are float, double, int, uint, long, ulong, decimal, " +
                "Vector2, Vector3, Vector4, Quaternion, Color and Color32.");
        }

        private void SetCommonValues(TweenValueType finalVal, float duration, string propertyName = "")
        {
            RequireSupportedType();

            _finalValue = finalVal;
            base.duration = duration;
            base.propertyName = propertyName;
            SetType(type);
            SetEase(ease);
        }

        private void SetReflectionFields(string method)
        {
            _property = null;
            _field = null;
            _setter = null;
            _getter = null;
            _isBroken = false;

            if (obj == null)
            {
                Debug.LogError($"{nameof(TweenCore)} : the object to tween is null.");
                _isBroken = true;
                return;
            }

            if (string.IsNullOrEmpty(method))
            {
                Debug.LogError($"{nameof(TweenCore)} : no property or field name set on the tween targeting {obj.name}.");
                _isBroken = true;
                return;
            }

            _property = obj.GetType().GetProperty(method);
            if (_property == null) _field = obj.GetType().GetField(method);

            if (_property == null && _field == null)
            {
                Debug.LogError($"{nameof(TweenCore)} : no property or field named \"{method}\" on {obj.GetType().Name}.");
                _isBroken = true;
                return;
            }

            if (_property != null)
            {
                if (!_property.CanWrite)
                {
                    Debug.LogError($"{nameof(TweenCore)} : \"{method}\" on {obj.GetType().Name} is read only and cannot be tweened.");
                    _isBroken = true;
                    return;
                }

                if (_property.PropertyType != typeof(TweenValueType))
                {
                    Debug.LogError($"{nameof(TweenCore)} : \"{method}\" on {obj.GetType().Name} is a {_property.PropertyType.Name}, " +
                                   $"not a {typeof(TweenValueType).Name}.");
                    _isBroken = true;
                    return;
                }

                BindAccessors();
                return;
            }

            if (_field.FieldType != typeof(TweenValueType))
            {
                Debug.LogError($"{nameof(TweenCore)} : \"{method}\" on {obj.GetType().Name} is a {_field.FieldType.Name}, " +
                               $"not a {typeof(TweenValueType).Name}.");
                _isBroken = true;
            }
        }

        /// <summary>
        /// Bind closed delegates over the target so updating costs a direct call instead of a
        /// reflection call that boxes. Falls back to reflection when the AOT compiler could not
        /// generate the instantiation, so IL2CPP builds stay correct either way.
        /// </summary>
        private void BindAccessors()
        {
            try
            {
                MethodInfo setMethod = _property.GetSetMethod(true);
                if (setMethod != null)
                {
                    _setter = (Action<TweenValueType>)Delegate.CreateDelegate(
                        typeof(Action<TweenValueType>), obj, setMethod);
                }

                MethodInfo getMethod = _property.GetGetMethod(true);
                if (getMethod != null)
                {
                    _getter = (Func<TweenValueType>)Delegate.CreateDelegate(
                        typeof(Func<TweenValueType>), obj, getMethod);
                }
            }
            catch (Exception)
            {
                _setter = null;
                _getter = null;
            }
        }

        public override void Start()
        {
            if (hasStarted) return;

            hasStarted = true;
            isPaused = false;
            isPlaying = true;
            isFinish = false;
            elapsedTime = 0f;

            EnsureCurveFuncs();

            if (_currentMethod == MethodUse.Reflection)
            {
                if (_isBroken || obj == null)
                {
                    TriggerOnStart();
                    Stop(false);
                    return;
                }

                if (fromCurrentValue)
                {
                    _startValue = GetObjValue();
                }
            }

            TriggerOnStart();

            if (duration <= 0f)
            {
                Stop(true);
            }
        }

        public override void Update(float deltaTime)
        {
            if (!isPlaying || isPaused) return;

            // The target can be destroyed part way through a tween : end quietly instead of
            // throwing a MissingReferenceException once per frame.
            if (_currentMethod == MethodUse.Reflection && (_isBroken || obj == null))
            {
                Stop(false);
                return;
            }

            elapsedTime += deltaTime;
            if (elapsedTime <= delay) return;

            float elapse = Mathf.Clamp(elapsedTime - delay, 0f, duration);
            float w = duration > 0f ? Mathf.Clamp01(elapse / duration) : 1f;

            SetValue(Evaluate(RealWeight(w)));

            TriggerOnUpdate();

            if (elapse >= duration) Stop();
        }

        /// <summary>
        /// The value this property ends on, which is the final value plus the start value
        /// when the property is additive.
        /// </summary>
        private TweenValueType EndValue()
        {
            if (fromCurrentValue && isIncreasingValue && TweenCoreOps<TweenValueType>.SupportsAdditive)
            {
                return TweenCoreOps<TweenValueType>.Add(_startValue, _finalValue);
            }

            return _finalValue;
        }

        private TweenValueType Evaluate(float weight)
        {
            return TweenCoreOps<TweenValueType>.Lerp(_startValue, EndValue(), weight);
        }

        private void StrategyMethod()
        {
            _function?.Invoke(_currentValue);
        }

        private void ReflectionMethod()
        {
            if (_setter != null)
            {
                _setter(_currentValue);
                return;
            }

            if (_property != null)
            {
                _property.SetValue(obj, _currentValue);
                return;
            }

            _field?.SetValue(obj, _currentValue);
        }

        private TweenValueType GetObjValue()
        {
            if (_getter != null) return _getter();

            object value = null;

            if (_property != null) value = _property.GetValue(obj);
            else if (_field != null) value = _field.GetValue(obj);

            return value is TweenValueType typedValue ? typedValue : default;
        }

        /// <summary>
        /// Add a delay before the animation start.
        /// </summary>
        /// <param name="tweenDelay">Time to wait in seconds.</param>
        /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetDelay(...).SetEase(...);).</returns>
        public TweenCoreProperty<TweenValueType> SetDelay(float tweenDelay)
        {
            delay = tweenDelay;
            return this;
        }

        /// <summary>
        /// Set the tween type.
        /// </summary>
        /// <param name="newType">The wanted tween type.</param>
        /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetType(...).SetEase(...);).</returns>
        public TweenCoreProperty<TweenValueType> SetType(TweenCoreType newType)
        {
            if (newType == TweenCoreType.Custom || newType == TweenCoreType.CustomCurve)
            {
                throw new ArgumentException("Need to give a function or an AnimationCurve for a custom type.", nameof(newType));
            }

            type = newType;
            SetTypeFunc(type);
            return this;
        }

        /// <summary>
        /// Set a custom tween Type, must be a function that return a float and have a float in parameters.
        /// </summary>
        /// <param name="newType">The wanted tween type.</param>
        /// <param name="customType">The function of the custom type, must return a float and take a float in parameters.</param>
        /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetType(...).SetEase(...);).</returns>
        public TweenCoreProperty<TweenValueType> SetType(TweenCoreType newType, Func<float, float> customType)
        {
            if (newType != TweenCoreType.Custom)
            {
                SetType(newType);
                return this;
            }

            return SetType(customType);
        }

        /// <summary>
        /// Set a custom tween Type, must be a function that return a float and have a float in parameters.
        /// </summary>
        /// <param name="customType">The function of the custom type, must return a float and take a float in parameters.</param>
        /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetType(...).SetEase(...);).</returns>
        public TweenCoreProperty<TweenValueType> SetType(Func<float, float> customType)
        {
            if (customType == null) throw new ArgumentNullException(nameof(customType));

            TypeFunc = customType;
            type = TweenCoreType.Custom;
            return this;
        }

        /// <summary>
        /// Set an AnimationCurve as tween Type.
        /// </summary>
        /// <param name="newType">The wanted Tween Type.</param>
        /// <param name="animationCurve">The animation curve to use as type.</param>
        /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetType(...).SetEase(...);).</returns>
        public TweenCoreProperty<TweenValueType> SetType(TweenCoreType newType, AnimationCurve animationCurve)
        {
            if (newType != TweenCoreType.CustomCurve)
            {
                SetType(newType);
                return this;
            }

            return SetType(animationCurve);
        }

        /// <summary>
        /// Set an AnimationCurve as tween Type.
        /// </summary>
        /// <param name="animationCurve">The animation curve to use as type.</param>
        /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetType(...).SetEase(...);).</returns>
        public TweenCoreProperty<TweenValueType> SetType(AnimationCurve animationCurve)
        {
            if (animationCurve == null) throw new ArgumentNullException(nameof(animationCurve));

            type = TweenCoreType.CustomCurve;
            typeAnimationCurve = animationCurve;
            return this;
        }

        /// <summary>
        /// Set the tween Ease.
        /// </summary>
        /// <param name="newEase">The wanted Tween Ease.</param>
        /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetEase(...).SetType(...);).</returns>
        public TweenCoreProperty<TweenValueType> SetEase(TweenCoreEase newEase)
        {
            if (newEase == TweenCoreEase.Custom || newEase == TweenCoreEase.CustomCurve)
            {
                throw new ArgumentException("Need to give a function or an AnimationCurve for a custom ease.", nameof(newEase));
            }

            ease = newEase;
            SetEaseFunc(ease);
            return this;
        }

        /// <summary>
        /// Set a custom ease, must be a function that return a float and have a type function and a float in parameters.
        /// </summary>
        /// <param name="newEase">The wanted Tween Ease.</param>
        /// <param name="customEase">The function of the custom ease,
        /// must return a float and take a Type function and a float in parameters.</param>
        /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetEase(...).SetType(...);).</returns>
        public TweenCoreProperty<TweenValueType> SetEase(TweenCoreEase newEase, Func<float, Func<float, float>, float> customEase)
        {
            if (newEase != TweenCoreEase.Custom)
            {
                SetEase(newEase);
                return this;
            }

            return SetEase(customEase);
        }

        /// <summary>
        /// Set a custom ease, must be a function that return a float and have a type function and a float in parameters.
        /// </summary>
        /// <param name="customEase">The function of the custom ease,
        /// must return a float and take a Type function and a float in parameters.</param>
        /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetEase(...).SetType(...);).</returns>
        public TweenCoreProperty<TweenValueType> SetEase(Func<float, Func<float, float>, float> customEase)
        {
            if (customEase == null) throw new ArgumentNullException(nameof(customEase));

            ease = TweenCoreEase.Custom;
            EaseFunc = customEase;
            return this;
        }

        /// <summary>
        /// Set an AnimationCurve as ease.
        /// </summary>
        /// <param name="newEase">The wanted Tween Ease.</param>
        /// <param name="animationCurve">The animation curve to use as ease.</param>
        /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetEase(...).SetType(...);).</returns>
        public TweenCoreProperty<TweenValueType> SetEase(TweenCoreEase newEase, AnimationCurve animationCurve)
        {
            if (newEase != TweenCoreEase.CustomCurve)
            {
                SetEase(newEase);
                return this;
            }

            return SetEase(animationCurve);
        }

        /// <summary>
        /// Set an AnimationCurve as ease.
        /// </summary>
        /// <param name="animationCurve">The animation curve to use as ease.</param>
        /// <returns>This TweenProperty to chain the methods calls (e.g. property.SetEase(...).SetType(...);).</returns>
        public TweenCoreProperty<TweenValueType> SetEase(AnimationCurve animationCurve)
        {
            if (animationCurve == null) throw new ArgumentNullException(nameof(animationCurve));

            ease = TweenCoreEase.CustomCurve;
            easeAnimationCurve = animationCurve;
            return this;
        }

        public TweenValueType GetCurrentValue() => CurrentValue;

        /// <summary>
        /// Set the start value of the property.
        /// </summary>
        /// <param name="value">The start value.</param>
        /// <returns>This TweenProperty to chain the methods calls (e.g. property.From(...).SetEase(...);).</returns>
        public TweenCoreProperty<TweenValueType> From(TweenValueType value)
        {
            _startValue = value;
            fromCurrentValue = false;
            return this;
        }

        /// <summary>
        /// Use the value the target holds when the tween starts as the start value.
        /// !WARNING! Works only using a Reflection method.
        /// </summary>
        /// <returns>This TweenProperty.</returns>
        public TweenCoreProperty<TweenValueType> FromCurrent()
        {
            if (_currentMethod != MethodUse.Reflection)
            {
                Debug.LogWarning($"{nameof(TweenCore)} : FromCurrent() only works on a reflection tween, it will be ignored here.");
                return this;
            }

            fromCurrentValue = true;
            return this;
        }

        /// <summary>
        /// Treat the final value as an offset added to the start value instead of an absolute value.
        /// Turning it on also turns FromCurrent on.
        /// </summary>
        /// <param name="isAdd">Whether the final value is an offset.</param>
        /// <returns>This TweenProperty.</returns>
        public TweenCoreProperty<TweenValueType> SetIsAdditive(bool isAdd)
        {
            if (isAdd && !TweenCoreOps<TweenValueType>.SupportsAdditive)
            {
                Debug.LogError($"{nameof(TweenCore)} : {typeof(TweenValueType).Name} cannot be used with SetIsAdditive.");
                return this;
            }

            isIncreasingValue = isAdd;
            if (isAdd)
            {
                fromCurrentValue = true;
            }

            return this;
        }

        public override TweenCorePropertyBase AddNextProperty(TweenCorePropertyBase property)
        {
            if (property != null && property != this) _nextProperties.Add(property);
            return this;
        }

        public override TweenCorePropertyBase ClearNextProperties()
        {
            _nextProperties.Clear();
            return this;
        }

        private float RealWeight(float w)
        {
            // The ease is a curve
            if (ease == TweenCoreEase.CustomCurve)
            {
                if (type == TweenCoreType.CustomCurve)
                {
                    //Type is a curve
                    return easeAnimationCurve.Evaluate(typeAnimationCurve.Evaluate(w));
                }

                // Type is a func
                return easeAnimationCurve.Evaluate(TypeFunc(w));
            }

            // The ease is a func
            if (type == TweenCoreType.CustomCurve)
            {
                // Type is a curve
                return EaseFunc(w, typeAnimationCurve.Evaluate);
            }

            // Type is a func
            return EaseFunc(w, TypeFunc);
        }

        protected override void TriggerOnStart()
        {
            base.TriggerOnStart();
            _unityEvents.unityOnStart?.Invoke(this);
        }

        protected override void TriggerOnUpdate()
        {
            base.TriggerOnUpdate();
            OnUpdateValue?.Invoke(this, _currentValue);
            _unityEvents.unityOnUpdate?.Invoke(this);
            _unityEvents.unityOnUpdateValue?.Invoke(this, _currentValue);
        }

        protected override void TriggerOnFinish()
        {
            base.TriggerOnFinish();
            _unityEvents.unityOnFinish?.Invoke(this);
        }

        /// <summary>
        /// Pause the TweenProperty.
        /// </summary>
        public void Pause()
        {
            isPaused = true;
        }

        /// <summary>
        /// Resume the TweenProperty at the last state.
        /// </summary>
        public void Resume()
        {
            isPaused = false;
        }

        /// <summary>
        /// Stop the TweenProperty. OnFinish is called here.
        /// </summary>
        /// <param name="setToFinalValue">Whether the target should be snapped to its final value.</param>
        /// <param name="continueChain">
        /// Whether the properties queued after this one should start. The tween passes false when
        /// it is cancelling, so stopping a chain no longer fast forwards the rest of it.
        /// </param>
        public override void Stop(bool setToFinalValue = true, bool continueChain = true)
        {
            if (!hasStarted && isFinish) return;

            isPlaying = false;
            isPaused = true;
            isFinish = true;
            elapsedTime = 0f;
            hasStarted = false;

            if (setToFinalValue) SetToFinalVals();

            if (continueChain) StartNextProperties();

            TriggerOnFinish();
        }

        private void StartNextProperties()
        {
            int length = _nextProperties.Count;
            for (int i = 0; i < length; i++)
            {
                _nextProperties[i].Start();
            }
        }

        private void SetValue(TweenValueType value)
        {
            _currentValue = value;

            switch (_currentMethod)
            {
                case MethodUse.Reflection:
                    if (_isBroken || obj == null) return;
                    ReflectionMethod();
                    break;
                case MethodUse.Strategy:
                    StrategyMethod();
                    break;
                case MethodUse.ReturnValue:
                    break;
                default:
                    throw new NotImplementedException();
            }

            // OnUpdateValue is raised once per frame, from TriggerOnUpdate.
        }

        /// <summary>
        /// Snap to the exact end value. Deliberately does not run the curve : a custom
        /// AnimationCurve that does not land on 1 would otherwise leave the target short.
        /// </summary>
        public override TweenCorePropertyBase SetToFinalVals()
        {
            SetValue(EndValue());
            return this;
        }
    }
}
