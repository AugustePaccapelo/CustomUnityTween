using UnityEngine;

// Author : Auguste Paccapelo

public class TestTween : MonoBehaviour
{
    // ---------- VARIABLES ---------- \\

    // ----- Prefabs & Assets ----- \\

    // ----- Objects ----- \\

    [SerializeField] private GameObject _target;
    [SerializeField] private GameObject _startObj;
    [SerializeField] private GameObject _endObj;

    // ----- Others ----- \\

    private Vector3 _startPos;
    private Vector3 _endPos;
    [SerializeField] private float _time = 2f;

    // ---------- FUNCTIONS ---------- \\

    // ----- Buil-in ----- \\

    private void OnEnable() { }

    private void OnDisable() { }

    private void Awake() { }

    private void Start()
    {
        _startPos = _startObj.transform.position;
        _endPos = _endObj.transform.position;

        Tween tween = Tween.CreateTween();
        tween.NewProperty(_target.transform, "position", _endPos, _time).From(_startPos)
            .SetType(TweenType.Bounce)
            .SetEase(TweenEase.In);

        tween.NewProperty(f => _target.transform.localScale = f, Vector2.zero, Vector2.one, _time * 2)
            .SetType(TweenType.Elastic).SetEase(TweenEase.Out)
            .SetDelay(1f);

        tween.Chain()
            .Play();
    }

    private void Update() { }

    // ----- My Functions ----- \\

    // ----- Destructor ----- \\

    private void OnDestroy() { }
}