using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Author : Auguste Paccapelo

namespace Tweening
{
    public class TweenCoreManager : MonoBehaviour
    {
        // ---------- VARIABLES ---------- \\

        // ----- Singleton ----- \\

        private static TweenCoreManager _instance;
        public static TweenCoreManager Instance
        {
            get
            {
                if (_instance != null) return _instance;

                if (!_canBeInstantiate) return null;

                // Never spawn a manager outside play mode : editor scripts and edit mode tests
                // would otherwise leave a stray GameObject in the open scene.
                if (!Application.isPlaying) return null;

                GameObject obj = new GameObject(nameof(TweenCoreManager));
                _instance = obj.AddComponent<TweenCoreManager>();

                return _instance;
            }
        }

        // ----- Objects ----- \\

        private List<TweenCore> _tweens = new List<TweenCore>();

        // Reused every frame so updating never allocates.
        private readonly List<TweenCore> _updateBuffer = new List<TweenCore>();

        // ----- Others ----- \\

        private bool _isPlaying = true;
        public bool IsPlaying => _isPlaying;
        static private bool _canBeInstantiate = true;

        public int NumTweens => _tweens.Count;

        // ---------- FUNCTIONS ---------- \\

        // ----- Buil-in ----- \\

        /// <summary>
        /// Statics survive between play sessions when domain reload is disabled, which used to
        /// leave _canBeInstantiate false for the rest of the editor session after one quit.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            _canBeInstantiate = true;
        }

        private void Awake()
        {
            // Singleton
            if (_instance != null && _instance != this)
            {
                Debug.Log(nameof(TweenCoreManager) + " Instance already exists, destroying last added.");
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (!_isPlaying) return;

            float scaledDeltaTime = Time.deltaTime;
            float unscaledDeltaTime = Time.unscaledDeltaTime;

            // A tween that finishes removes itself, so the list cannot be iterated directly.
            _updateBuffer.Clear();
            _updateBuffer.AddRange(_tweens);

            for (int i = _updateBuffer.Count - 1; i >= 0; i--)
            {
                TweenCore tween = _updateBuffer[i];
                tween.Update(tween.UseUnscaledTime ? unscaledDeltaTime : scaledDeltaTime);
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnApplicationQuit()
        {
            _canBeInstantiate = false;
        }

        // ----- My Functions ----- \\

        private void OnSceneUnloaded(Scene scene)
        {
            TweenCore[] tweens = _tweens.ToArray();

            for (int i = tweens.Length - 1; i >= 0; i--)
            {
                // sceneUnloaded fires after the scene objects are destroyed, so writing final
                // values here would reflect onto dead objects.
                if (!tweens[i].SurviveOnSceneUnload) tweens[i].Stop(false);
            }
        }

        /// <summary>
        /// Pause the manager. Individual tweens keep their own paused state.
        /// </summary>
        public void PauseAll()
        {
            _isPlaying = false;
        }

        /// <summary>
        /// Resume the manager.
        /// </summary>
        public void ResumeAll()
        {
            _isPlaying = true;
        }

        /// <summary>
        /// Stop every registered tween.
        /// </summary>
        /// <param name="setToFinalValue">Whether running properties snap to their final value.</param>
        public void StopAll(bool setToFinalValue = true)
        {
            TweenCore[] tweens = _tweens.ToArray();

            for (int i = tweens.Length - 1; i >= 0; i--)
            {
                tweens[i].Stop(setToFinalValue);
            }
        }

        public void AddTween(TweenCore tween)
        {
            if (tween != null && !_tweens.Contains(tween)) _tweens.Add(tween);
        }

        public void RemoveTween(TweenCore tween)
        {
            _tweens.Remove(tween);
        }

        // ----- Destructor ----- \\

        protected virtual void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}
