using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Author : Auguste Paccapelo

namespace Tweening
{
    public class TweenCoreComponent : MonoBehaviour
    {
        // ---------- VARIABLES ---------- \\

        // ----- Prefabs & Assets ----- \\

        // ----- Objects ----- \\

        private TweenCore _tween;
        public TweenCore Tween => _tween;

        // ----- Others ----- \\

        [SerializeField] private string _name = "";
        public string TweenName
        {
            get => _name;
            set => _name = value;
        }

        [SerializeField] private bool _playOnStart = true;
        [SerializeField] private bool _isParallel = true;
        [SerializeField] private bool _isLoop = false;
        [SerializeField] private bool _isInfinite = false;
        [SerializeField] private int _numIteration = 1;
        [SerializeField] private bool _DestroyWhenFinished = true;
        [SerializeField] private bool _surviveOnUnload = false;
        [SerializeField] private bool _useUnscaledTime = false;

        [SerializeReference] private List<TweenCorePropertyBase> _properties = new List<TweenCorePropertyBase>();

        [Serializable]
        private class TweenUnityEvents
        {
            public UnityEvent<TweenCore> OnStart;
            public UnityEvent<TweenCore> OnUpdate;
            public UnityEvent<TweenCore> OnFinish;
            public UnityEvent<TweenCore> OnLoopFinish;
        }

        [SerializeField] private TweenUnityEvents _unityEvents = new TweenUnityEvents();

        // ---------- FUNCTIONS ---------- \\

        // ----- Buil-in ----- \\

        private void Awake()
        {
            _tween = TweenCore.CreateTween();
        }

        private void Start()
        {
            if (_tween == null) return;

            int numIteration = _isLoop && _isInfinite ? -1 : _numIteration;

            _tween.SetLoop(_isLoop, numIteration)
                .SetParallel(_isParallel)
                .SetSurviveOnUnload(_surviveOnUnload)
                .SetUseUnscaledTime(_useUnscaledTime)
                .SetDestroyWhenFinish(_DestroyWhenFinished);

            if (_surviveOnUnload)
            {
                DontDestroyOnLoad(gameObject);
            }

            foreach (TweenCorePropertyBase property in _properties)
            {
                if (property == null) continue;

                try
                {
                    property.SetBaseValues();
                }
                catch (Exception exception)
                {
                    Debug.LogError($"{nameof(TweenCoreComponent)} \"{_name}\" on {name} : a property could not be set up and was skipped.", this);
                    Debug.LogException(exception, this);
                    continue;
                }

                _tween.AddProperty(property);
            }

            // The tween registers itself with the manager on Play().

            _tween.OnStart += OnTweenStart;
            _tween.OnUpdate += OnTweenUpdate;
            _tween.OnFinish += OnTweenFinish;
            _tween.OnLoopFinish += OnTweenLoopFinish;

            if (_playOnStart) Play();
        }

        // ----- My Functions ----- \\

        public void AddProperty(TweenCorePropertyBase property)
        {
            if (property == null) return;

            _properties.Add(property);
        }

        public void Play()
        {
            _tween?.Play();
        }

        public void Pause()
        {
            _tween?.Pause();
        }

        public void Resume()
        {
            _tween?.Resume();
        }

        public void Restart()
        {
            _tween?.Restart();
        }

        /// <summary>
        /// Jump every property to its end value, including chain links that never ran, then end.
        /// </summary>
        public void Complete()
        {
            _tween?.Complete();
        }

        public void StopAndSetToFinalValue()
        {
            _tween?.Stop(true);
        }

        public void StopAndDontChangeValue()
        {
            _tween?.Stop(false);
        }

        private void OnTweenStart(TweenCore tween)
        {
            _unityEvents.OnStart?.Invoke(tween);
        }

        private void OnTweenUpdate(TweenCore tween)
        {
            _unityEvents.OnUpdate?.Invoke(tween);
        }

        private void OnTweenFinish(TweenCore tween)
        {
            _unityEvents.OnFinish?.Invoke(tween);
        }

        private void OnTweenLoopFinish(TweenCore tween)
        {
            _unityEvents.OnLoopFinish?.Invoke(tween);
        }

        // ----- Destructor ----- \\

        private void OnDestroy()
        {
            if (_tween == null) return;

            _tween.Stop(false);
            _tween.DestroyTween();
        }
    }
}
