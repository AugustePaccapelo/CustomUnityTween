using Tweening;
using System.Collections;
using UnityEngine;

// Author : Auguste Paccapelo

public class DefaultScript : MonoBehaviour
{
    // ---------- VARIABLES ---------- \\

    // ----- Prefabs & Assets ----- \\

    // ----- Objects ----- \\

    private TweenCore _myTween;

    [SerializeField] private TweenCoreComponent comp;

    // ----- Others ----- \\

    // ---------- FUNCTIONS ---------- \\

    // ----- Buil-in ----- \\

    private void OnEnable() { }

    private void OnDisable() { }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _myTween = TweenCore.CreateTween().SurviveOnUnload();
        _myTween.NewProperty(TweenFunc, Vector3.zero, new Vector3(5, 5, 0), 2f)
            .SetEase(TweenCoreEase.Out).SetType(TweenCoreType.Elastic);
        //StartCoroutine(TweenCoroutine());
    }

    private void Update() { }

    // ----- My Functions ----- \\

    private IEnumerator TweenCoroutine()
    {
        yield return new WaitForSeconds(5f);
        Debug.Log("play");
        comp.Play();
    }

    private void TweenFunc(Vector3 pos)
    {
        transform.position = pos;
    }

    public void PlayTween()
    {
        //_myTween.Play();
    }

    // ----- Destructor ----- \\

    private void OnDestroy() { }
}