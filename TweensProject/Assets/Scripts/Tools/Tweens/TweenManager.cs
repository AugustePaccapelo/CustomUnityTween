using System.Collections.Generic;
using UnityEngine;

// Author : Auguste Paccapelo

public class TweenManager : MonoBehaviour
{
    // ---------- VARIABLES ---------- \\

    // ----- Singleton ----- \\

    public static TweenManager Instance {get; private set;}

    // ----- Prefabs & Assets ----- \\

    // ----- Objects ----- \\

    private List<Tween> _tweens = new List<Tween>();

    // ----- Others ----- \\

    // ---------- FUNCTIONS ---------- \\

    // ----- Buil-in ----- \\

    private void OnEnable() { }

    private void OnDisable() { }

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

    void Start() { }

    void Update() { }

    private void FixedUpdate()
    {
        for (int i = _tweens.Count - 1; i >= 0; i--)
        {
            _tweens[i].Update(Time.fixedDeltaTime);
        }
    }

    // ----- My Functions ----- \\

    public void AddTween(Tween tween)
    {
        _tweens.Add(tween);
    }

    public void RemoveTween(Tween tween)
    {
        _tweens.Remove(tween);
    }

    // ----- Destructor ----- \\

    protected virtual void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}