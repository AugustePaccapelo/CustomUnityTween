using System.Collections.Generic;
using UnityEngine;

// Author : Auguste Paccapelo

public class TweenManager : MonoBehaviour
{
    // ---------- VARIABLES ---------- \\

    // ----- Singleton ----- \\

    public static TweenManager Instance {get; private set;}

    // ----- Objects ----- \\

    private List<Tween> _tweens = new List<Tween>();

    // ----- Others ----- \\

    private bool _isPlaying = true;

    // ---------- FUNCTIONS ---------- \\

    // ----- Buil-in ----- \\

    private void Awake()
    {
        // Singleton
        if (Instance != null)
        {
            Debug.Log(nameof(TweenManager) + " Instance already exist, destorying last added.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void FixedUpdate()
    {
        if (!_isPlaying) return;

        for (int i = _tweens.Count - 1; i >= 0; i--)
        {
            _tweens[i].Update(Time.fixedDeltaTime);
        }
    }

    // ----- My Functions ----- \\

    public void PauseAll()
    {
        _isPlaying = false;
    }

    public void ResumeAll()
    {
        _isPlaying = true;
    }

    public void StopAll()
    {
        int length = _tweens.Count - 1;
        for (int i = length;  i >= 0; i--)
        {
            _tweens[i].Stop();
        }
    }

    public void AddTween(Tween tween)
    {
        if (!_tweens.Contains(tween)) _tweens.Add(tween);
    }

    public void RemoveTween(Tween tween)
    {
        if (_tweens.Contains(tween)) _tweens.Remove(tween);
    }

    // ----- Destructor ----- \\

    protected virtual void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}