using Tweening;
using UnityEngine;

/// <summary>
/// Drops a triangle down the screen in five bouncing steps.
///
/// The interesting part is that there is only *one* tween property. It is additive - it adds an
/// offset to wherever the target currently is, rather than moving to a fixed position - and the
/// tween loops five times. Each iteration re-reads the current position and subtracts the offset
/// again, so the triangle walks down: 10, 6, 2, -2, -6, -10.
///
/// Written five separate properties would work too, but this way retiming the whole fall is one
/// number, and adding a sixth step is one number.
///
/// Bounce is an "In" shape since 1.1, so Out is what puts the bounce at the *end* of each step,
/// which is where a falling thing lands.
///
/// TriangleFallComponent.unity is this same animation built entirely in the inspector, with no
/// script attached at all.
/// </summary>
public class TriangleFallDemo : MonoBehaviour
{
    // ---------- VARIABLES ---------- \\

    [Header("Fall")]
    [SerializeField] private float _startY = 10f;
    [SerializeField] private float _dropPerStep = -4f;
    [SerializeField] private int _steps = 5;
    [SerializeField] private float _secondsPerStep = 0.6f;

    [Header("Look")]
    [SerializeField] private Color _colour = Color.white;
    [SerializeField] private float _size = 1.5f;

    // ---------- FUNCTIONS ---------- \\

    private void Start()
    {
        Transform triangle = BuildTriangle();
        triangle.localPosition = new Vector3(0f, _startY, 0f);

        TweenCore tween = TweenCore.CreateTween();

        // One relative step. FromCurrent is implied by SetIsAdditive, but saying it out loud makes
        // the intent obvious : start wherever we are, and add the offset.
        tween.NewProperty(triangle, TweenCoreTarget.Transform.LOCAL_POSITION,
                          new Vector3(0f, _dropPerStep, 0f), _secondsPerStep)
             .SetIsAdditive(true)
             .SetType(TweenCoreType.Bounce)
             .SetEase(TweenCoreEase.Out);

        // Run that step _steps times. Each iteration starts from where the last one ended.
        tween.SetLoop(true, _steps)
             .Play();
    }

    /// <summary>
    /// An upward-pointing triangle, drawn into a texture by testing each pixel against the two
    /// sloping edges. Cheap, and it keeps the demo free of imported art.
    /// </summary>
    private Transform BuildTriangle()
    {
        const int RESOLUTION = 128;

        Texture2D texture = new Texture2D(RESOLUTION, RESOLUTION, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            name = "TriangleDemoTexture",
        };

        Color32 opaque = _colour;
        Color32 clear = new Color32(0, 0, 0, 0);

        for (int y = 0; y < RESOLUTION; y++)
        {
            // Half-width of the triangle at this row : 0 at the apex, full at the base.
            float halfWidth = (y / (float)(RESOLUTION - 1)) * 0.5f;

            for (int x = 0; x < RESOLUTION; x++)
            {
                float offsetFromCentre = Mathf.Abs(x / (float)(RESOLUTION - 1) - 0.5f);
                texture.SetPixel(x, RESOLUTION - 1 - y, offsetFromCentre <= halfWidth ? opaque : clear);
            }
        }

        texture.Apply();

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, RESOLUTION, RESOLUTION),
            new Vector2(0.5f, 0.5f),
            RESOLUTION / _size);
        sprite.name = "TriangleDemoSprite";

        GameObject go = new GameObject("Triangle");
        go.transform.SetParent(transform, false);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = _colour;

        return go.transform;
    }
}
