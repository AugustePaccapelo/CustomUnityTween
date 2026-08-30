# TweenCore

Tweening system for Unity. Animate any property or field by reflection, by function, or by reading the value yourself. Includes `TweenCoreComponent` to build tweens from the inspector without writing code.

Everything lives in the `Tweening` namespace:

```csharp
using Tweening;

TweenCore tween = TweenCore.CreateTween();

tween.NewProperty(transform, TweenCoreTarget.Transform.GLOBAL_POSITION, new Vector3(5, 2, 0), 2f)
    .SetEase(TweenCoreEase.Out)
    .SetType(TweenCoreType.Bounce);

tween.Play();
```

**The full documentation — API reference, upgrade notes and changelog — lives in the [README at the repository root](../../../README.md).**

This file is intentionally a pointer. Keeping two copies of the documentation in sync by hand is what let the previous one drift out of date.

---

*Author: Auguste Paccapelo*
