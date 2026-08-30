using System;
using System.Collections.Generic;

// Author : Auguste Paccapelo

namespace Tweening
{
    public class TweenCore
    {
        // ---------- VARIABLES ---------- \\

        // ----- Objects ----- \\

        private List<TweenCorePropertyBase> _tweenProperties = new List<TweenCorePropertyBase>();

        // Reused every frame so updating never allocates. Only Update() may use it :
        // the colder paths take their own copy so a re-entrant Stop() cannot clobber it.
        private readonly List<TweenCorePropertyBase> _updateBuffer = new List<TweenCorePropertyBase>();

        // ----- Others ----- \\

        private bool _isPlaying = false;
        public bool IsPlaying => _isPlaying;

        private bool _isPaused = true;
        public bool IsPaused => _isPaused;

        private bool _hasStarted = false;
        public bool HasStarted => _hasStarted;

        private bool _isFinished = false;
        public bool IsFinished => _isFinished;

        private bool _isParallel = true;
        public bool IsParallel => _isParallel;

        private bool _isLoop = false;
        public bool IsLoop => _isLoop;

        private bool _destroyOnFinish = true;
        public bool DestroyOnFinish => _destroyOnFinish;

        private bool _surviveOnSceneUnload = false;
        public bool SurviveOnSceneUnload => _surviveOnSceneUnload;

        private bool _useUnscaledTime = false;
        public bool UseUnscaledTime => _useUnscaledTime;

        private int _numPropertiesFinished = 0;
        public int NumPropertiesFinished => _numPropertiesFinished;

        private int _expectedNumProperties;
        public int NumProperties => _expectedNumProperties;

        private float _elapsedTime = 0f;
        public float ElapsedTime => _elapsedTime;

        [Obsolete("Renamed to ElapsedTime.")]
        public float ElapseTime => _elapsedTime;

        private int _numIteration = -1;
        public int NumIteration => _numIteration;

        private int _currentIteration = 0;
        public int CurrentIteration => _currentIteration;

        public event Action<TweenCore> OnStart;
        public event Action<TweenCore> OnUpdate;
        public event Action<TweenCore> OnFinish;
        public event Action<TweenCore> OnLoopFinish;

        // ---------- FUNCTIONS ---------- \\

        /// <summary>
        /// You don't need to call this function, TweenCoreManager is handling it.
        /// Update the tween and all properties, if all properties are finished, stop the tween.
        /// </summary>
        /// <param name="deltaTime">Time since last call.</param>
        /// <returns>This tween.</returns>
        public TweenCore Update(float deltaTime)
        {
            if (!_isPlaying || _isPaused) return this;

            OnUpdate?.Invoke(this);
            _elapsedTime += deltaTime;

            // A property that finishes removes itself through OnFinish, so the list cannot be
            // iterated directly.
            _updateBuffer.Clear();
            _updateBuffer.AddRange(_tweenProperties);

            for (int i = _updateBuffer.Count - 1; i >= 0; i--)
            {
                _updateBuffer[i].Update(deltaTime);
            }

            // A callback may already have stopped the tween.
            if (!_isPlaying) return this;

            if (_numPropertiesFinished >= _expectedNumProperties)
            {
                _currentIteration++;

                if (_isLoop && (_numIteration < 0 || _currentIteration < _numIteration))
                {
                    RestartTween();
                }
                else
                {
                    Stop();
                }
            }

            return this;
        }

        private void RestartTween()
        {
            OnLoopFinish?.Invoke(this);
            _numPropertiesFinished = 0;

            TweenCorePropertyBase[] properties = _tweenProperties.ToArray();

            if (!_isParallel)
            {
                if (properties.Length > 0) properties[0].Start();
                return;
            }

            for (int i = 0; i < properties.Length; i++) properties[i].Start();
        }

        /// <summary>
        /// Pause the tween. Properties keep their state and resume where they stopped.
        /// </summary>
        /// <returns>This tween, so you can chain the methods calls (e.g. tween.Pause().Resume();).</returns>
        public TweenCore Pause()
        {
            _isPaused = true;

            return this;
        }

        /// <summary>
        /// Resume the tween at the state it was paused.
        /// </summary>
        /// <returns>This tween, so you can chain the methods calls (e.g. tween.Resume().Pause();).</returns>
        public TweenCore Resume()
        {
            _isPaused = false;

            return this;
        }

        /// <summary>
        /// Start the tween if this is called for the first time.
        /// In parallel mode, all properties start at the same time, in chain mode only one runs at a time.
        /// </summary>
        /// <returns>This tween, so you can chain the methods calls (e.g. tween.Play().Pause();).</returns>
        public TweenCore Play()
        {
            // Can't start 2 times
            if (_hasStarted) return this;

            // Set values
            _numPropertiesFinished = 0;
            _currentIteration = 0;
            _hasStarted = true;
            _isPaused = false;
            _isPlaying = true;
            _isFinished = false;

            _elapsedTime = 0f;

            // Counted before anything starts : a zero duration property finishes inside Start()
            // and removes itself, which would leave the expected count short.
            _expectedNumProperties = _tweenProperties.Count;

            // Registered only once it actually plays, so a tween that is built and then
            // abandoned is never left in the manager being updated forever.
            TweenCoreManager.Instance?.AddTween(this);

            OnStart?.Invoke(this);

            // Zero iterations : nothing runs and no value is written.
            if (_isLoop && _numIteration == 0)
            {
                Stop(false);
                return this;
            }

            // Snapshot : a property that finishes inside Start() removes itself from the list.
            TweenCorePropertyBase[] properties = _tweenProperties.ToArray();
            int numProperties = properties.Length;

            // Chain links are rebuilt from scratch so replaying never stacks duplicates.
            for (int i = 0; i < numProperties; i++)
            {
                properties[i].ClearNextProperties();

                if (!_isParallel && i < numProperties - 1)
                {
                    properties[i].AddNextProperty(properties[i + 1]);
                }
            }

            if (_isParallel)
            {
                for (int i = 0; i < numProperties; i++) properties[i].Start();
            }
            else if (numProperties > 0)
            {
                properties[0].Start();
            }

            return this;
        }

        private void DestroyTweenProperty(TweenCorePropertyBase property)
        {
            _tweenProperties.Remove(property);
        }

        private void NewPropertyFinished(TweenCorePropertyBase property)
        {
            _numPropertiesFinished++;

            if (!_isLoop && _destroyOnFinish)
            {
                DestroyTweenProperty(property);
            }
        }

        /// <summary>
        /// Stop the tween. Properties that are running are stopped, and by default snapped to
        /// their final value. Chain links that never ran are left untouched : this cancels the
        /// tween, it does not complete it. Use Complete() for that.
        /// OnFinish is called here after all properties are stopped.
        /// </summary>
        /// <param name="setToFinalValue">Whether running properties snap to their final value.</param>
        public void Stop(bool setToFinalValue = true)
        {
            if (!_hasStarted) return;

            _hasStarted = false;
            _isPaused = false;
            _isPlaying = false;

            // Snapshot : stopping a property removes it from the list through OnFinish.
            TweenCorePropertyBase[] properties = _tweenProperties.ToArray();

            for (int i = properties.Length - 1; i >= 0; i--)
            {
                // continueChain is false : cancelling must not start the links that never ran.
                if (properties[i].HasStarted) properties[i].Stop(setToFinalValue, false);
            }

            _elapsedTime = 0f;
            _currentIteration = 0;
            _numPropertiesFinished = 0;

            _isFinished = true;
            OnFinish?.Invoke(this);

            if (_destroyOnFinish) DestroyTween();
        }

        /// <summary>
        /// Stop the tween without writing any value. Same as Stop(false).
        /// </summary>
        public void Kill() => Stop(false);

        /// <summary>
        /// Jump every property to its end value, including chain links that never ran, then end.
        /// </summary>
        /// <returns>This tween.</returns>
        public TweenCore Complete()
        {
            TweenCorePropertyBase[] properties = _tweenProperties.ToArray();

            for (int i = 0; i < properties.Length; i++)
            {
                properties[i].SetToFinalVals();
            }

            // Values are already written, so the stop must not write them a second time.
            Stop(false);

            return this;
        }

        /// <summary>
        /// Stop the tween without writing any value and play it again from the start.
        /// Properties are kept even when the tween is set to destroy on finish.
        /// </summary>
        /// <returns>This tween.</returns>
        public TweenCore Restart()
        {
            bool destroyOnFinish = _destroyOnFinish;

            // Keeps the properties and the tween itself alive across the stop.
            _destroyOnFinish = false;
            Stop(false);
            _destroyOnFinish = destroyOnFinish;

            _isFinished = false;

            return Play();
        }

        /// <summary>
        /// Function used to create a new Tween.
        /// A Tween handles one or multiple TweenProperty.
        /// The tween registers itself with the manager when Play() is called.
        /// </summary>
        /// <returns>The tween created.</returns>
        public static TweenCore CreateTween()
        {
            return new TweenCore();
        }

        /// <summary>
        /// Create a new TweenProperty that doesn't modify any exterior property or field.
        /// Use OnUpdate or CurrentValue to get the value of the property.
        /// </summary>
        /// <typeparam name="TweenValueType">The type of value (e.g. float, Vector3, ...).</typeparam>
        /// <param name="startVal">The start value of the property.</param>
        /// <param name="finalVal">The end value of the property.</param>
        /// <param name="time">The duration of the property.</param>
        /// <returns>The TweenProperty to chain the methods calls (e.g. NewProperty(...).SetEase(...);).</returns>
        public TweenCoreProperty<TweenValueType> NewProperty<TweenValueType>(TweenValueType startVal, TweenValueType finalVal, float time)
        {
            TweenCoreProperty<TweenValueType> property = new TweenCoreProperty<TweenValueType>(startVal, finalVal, time);
            AddProperty(property);
            return property;
        }

        /// <summary>
        /// Create a new TweenProperty that uses a function to modify a property or field.
        /// This uses fewer resources but is a bit harder to use.
        /// </summary>
        /// <typeparam name="TweenValueType">The type of value (e.g. float, Vector3, ...).</typeparam>
        /// <param name="function">The function to run each frame when updating the value
        ///  (e.g. v => transform.position = v)</param>
        /// <param name="startVal">The start value of the property.</param>
        /// <param name="finalVal">The end value of the property.</param>
        /// <param name="time">The duration of the property.</param>
        /// <returns>The TweenProperty to chain the methods calls (e.g. NewProperty(...).SetEase(...);).</returns>
        public TweenCoreProperty<TweenValueType> NewProperty<TweenValueType>(Action<TweenValueType> function, TweenValueType startVal, TweenValueType finalVal, float time)
        {
            TweenCoreProperty<TweenValueType> property = new TweenCoreProperty<TweenValueType>(function, startVal, finalVal, time);
            AddProperty(property);
            return property;
        }

        /// <summary>
        /// Create a new TweenProperty that modifies the given property or field of the given object.
        /// This uses reflection, it costs more but is a lot easier to use.
        /// By default startValue is the value when Play() is called.
        /// </summary>
        /// <typeparam name="TweenValueType">The type of value (e.g. float, Vector3, ...).</typeparam>
        /// <param name="obj">The target object of the tween (e.g. transform)</param>
        /// <param name="method">The property or field to modify (e.g. "position")</param>
        /// <param name="finalVal">The end value of the property.</param>
        /// <param name="time">The duration of the property.</param>
        /// <returns>The TweenProperty to chain the methods calls (e.g. NewProperty(...).SetEase(...);).</returns>
        public TweenCoreProperty<TweenValueType> NewProperty<TweenValueType>(UnityEngine.Object obj, string method, TweenValueType finalVal, float time)
        {
            TweenCoreProperty<TweenValueType> property = new TweenCoreProperty<TweenValueType>(obj, method, finalVal, time);
            AddProperty(property);
            return property;
        }

        /// <summary>
        /// Create a new TweenProperty that modifies the given property or field of the given object.
        /// This uses reflection, it costs more but is a lot easier to use.
        /// </summary>
        /// <typeparam name="TweenValueType">The type of value (e.g. float, Vector3, ...).</typeparam>
        /// <param name="obj">The target object of the tween (e.g. transform)</param>
        /// <param name="method">The property or field to modify (e.g. "position")</param>
        /// <param name="startVal">The start value of the property.</param>
        /// <param name="finalVal">The end value of the property.</param>
        /// <param name="time">The duration of the property.</param>
        /// <returns>The TweenProperty to chain the methods calls (e.g. NewProperty(...).SetEase(...);).</returns>
        public TweenCoreProperty<TweenValueType> NewProperty<TweenValueType>(UnityEngine.Object obj, string method, TweenValueType startVal, TweenValueType finalVal, float time)
        {
            TweenCoreProperty<TweenValueType> property = new TweenCoreProperty<TweenValueType>(obj, method, startVal, finalVal, time);
            AddProperty(property);
            return property;
        }

        public TweenCore AddProperty(TweenCorePropertyBase property)
        {
            if (property == null || _tweenProperties.Contains(property)) return this;

            _tweenProperties.Add(property);
            property.OnFinish += NewPropertyFinished;
            return this;
        }

        /// <summary>
        /// Set the Parallel or Chain mode, if Parallel all tweenProperties play at the same time, in Chain only one can play at a time.
        /// Parallel is true by default.
        /// </summary>
        /// <param name="isParallel">If is in parallel.</param>
        /// <returns>This tween, so you can chain the methods calls (e.g. tween.SetParallel(true).Play();).</returns>
        public TweenCore SetParallel(bool isParallel)
        {
            _isParallel = isParallel;
            return this;
        }

        /// <summary>
        /// Set the Parallel or Chain mode, if Parallel all tweenProperties play at the same time, in Chain only one can play at a time.
        /// Parallel is true by default.
        /// </summary>
        /// <param name="isChain">If is in chain.</param>
        /// <returns>This tween, so you can chain the methods calls (e.g. tween.SetChain(true).Play();).</returns>
        public TweenCore SetChain(bool isChain)
        {
            _isParallel = !isChain;
            return this;
        }

        /// <summary>
        /// Set the Parallel mode, all tweenProperties play at the same time.
        /// Parallel is true by default.
        /// </summary>
        /// <returns>This tween, so you can chain the methods calls (e.g. tween.Parallel().Play();).</returns>
        public TweenCore Parallel()
        {
            _isParallel = true;
            return this;
        }

        /// <summary>
        /// Set the Chain mode, only one tweenProperty can play at a time.
        /// Parallel is true by default.
        /// </summary>
        /// <returns>This tween, so you can chain the methods calls (e.g. tween.Chain().Play();).</returns>
        public TweenCore Chain()
        {
            _isParallel = false;
            return this;
        }

        /// <summary>
        /// Set the loop mode, wait for all properties to be finished, and then replay the tween.
        /// </summary>
        /// <param name="isLoop">If loop mode.</param>
        /// <param name="numIteration">Number of iterations, negative for infinite, 0 for none.</param>
        /// <returns>This tween.</returns>
        public TweenCore SetLoop(bool isLoop, int numIteration = -1)
        {
            _isLoop = isLoop;
            _numIteration = numIteration;
            return this;
        }

        /// <summary>
        /// Drive this tween with unscaled time, so it keeps running while Time.timeScale is 0.
        /// </summary>
        /// <param name="useUnscaledTime">Whether to ignore Time.timeScale.</param>
        /// <returns>This tween.</returns>
        public TweenCore SetUseUnscaledTime(bool useUnscaledTime)
        {
            _useUnscaledTime = useUnscaledTime;
            return this;
        }

        /// <summary>
        /// The tween will survive when the scene unloads.
        /// </summary>
        /// <returns>This Tween.</returns>
        public TweenCore SurviveOnUnload()
        {
            _surviveOnSceneUnload = true;
            return this;
        }

        [Obsolete("Renamed to SurviveOnUnload : it takes effect on unload, not on load.")]
        public TweenCore SurviveOnSceneLoad() => SurviveOnUnload();

        /// <summary>
        /// The tween will not survive when the scene unloads.
        /// </summary>
        /// <returns>This Tween.</returns>
        public TweenCore KillOnUnload()
        {
            _surviveOnSceneUnload = false;
            return this;
        }

        [Obsolete("Renamed to KillOnUnload.")]
        public TweenCore KillOnSceneUnLoad() => KillOnUnload();

        /// <summary>
        /// Set if the tween should survive or not on scene unloads.
        /// </summary>
        /// <returns>This Tween.</returns>
        public TweenCore SetSurviveOnUnload(bool survive)
        {
            _surviveOnSceneUnload = survive;
            return this;
        }

        /// <summary>
        /// This tween and the properties attached will be destroyed when finished.
        /// </summary>
        /// <returns>This Tween.</returns>
        public TweenCore DestroyWhenFinish()
        {
            _destroyOnFinish = true;
            return this;
        }

        /// <summary>
        /// This tween and the properties attached will not be destroyed when finished.
        /// </summary>
        /// <returns>This Tween.</returns>
        public TweenCore DontDestroyWhenFinish()
        {
            _destroyOnFinish = false;
            return this;
        }

        /// <summary>
        /// Set if this tween and the properties attached should be destroyed when finished.
        /// </summary>
        /// <returns>This Tween.</returns>
        public TweenCore SetDestroyWhenFinish(bool destroy)
        {
            _destroyOnFinish = destroy;
            return this;
        }

        /// <summary>
        /// Remove this TweenCore from the manager without modifying any value.
        /// </summary>
        public void DestroyTween()
        {
            TweenCoreManager.Instance?.RemoveTween(this);
        }
    }
}
