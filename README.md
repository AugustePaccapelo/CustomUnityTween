# TweenCore — Documentation

Tweening system for Unity. Animate any property or field by **reflection**, by **function**, or by reading the value yourself. Includes a `TweenCoreComponent` to build tweens from the inspector without writing code.

Requires Unity 6000.0 or newer.

---

## Install

**Unity Package Manager** — *Window → Package Manager → + → Add package from git URL*, or add to `Packages/manifest.json`:

```json
"com.augustepaccapelo.tweencore": "https://github.com/<user>/TweenCore.git?path=TweensProject/Assets/TweenCore"
```

**Or** import the latest `.unitypackage` from `Releases/`.

Everything lives in the `Tweening` namespace:

```csharp
using Tweening;
```

The manager creates itself the first time a tween plays — you do not need to put one in the scene.

---

## Examples of uses

### Reflection

Easiest to use, costs the most. The target property or field is resolved once when the tween starts.

```csharp
TweenCore tween = TweenCore.CreateTween();

TweenCoreProperty<Vector3> property =
    tween.NewProperty(transform, "position", Vector3.zero, new Vector3(5, 2, 0), 2f);

property.SetEase(TweenCoreEase.Out);
property.SetType(TweenCoreType.Bounce);

tween.Play();
```

The common Unity targets have constants so you do not have to type the string:

```csharp
TweenCore tween = TweenCore.CreateTween();

tween.NewProperty(transform, TweenCoreTarget.Transform.GLOBAL_POSITION, new Vector3(5, 2, 0), 2f)
    .SetEase(TweenCoreEase.Out)
    .SetType(TweenCoreType.Bounce);

tween.Play();
```

With this overload the start value is whatever the target holds when `Play()` is called.

### Function

No reflection, so it costs the least.

```csharp
TweenCore tween = TweenCore.CreateTween();

tween.NewProperty(v => _target.transform.localScale = v, Vector3.zero, Vector3.one, _time * 2)
    .SetType(TweenCoreType.Bounce)
    .SetEase(TweenCoreEase.Out);

tween.Play();
```

### Manual

The tween only computes the value; you decide what to do with it.

```csharp
TweenCore tween = TweenCore.CreateTween();

TweenCoreProperty<Vector3> property =
    tween.NewProperty(Vector3.zero, Vector3.one, _time * 2)
        .SetType(TweenCoreType.Bounce)
        .SetEase(TweenCoreEase.Out);

tween.Play();

// later, e.g. in Update
transform.localScale = property.CurrentValue;
```

### Chain, loop and unscaled time

```csharp
TweenCore tween = TweenCore.CreateTween();

tween.NewProperty(transform, TweenCoreTarget.Transform.LOCAL_POSITION, Vector3.up, 1f);
tween.NewProperty(transform, TweenCoreTarget.Transform.LOCAL_SCALE, Vector3.one * 2f, 1f);

tween.Chain()                    // one property at a time instead of all at once
     .SetLoop(true, 3)           // negative for infinite, 0 for none
     .SetUseUnscaledTime(true)   // keeps running while Time.timeScale is 0
     .Play();
```

---

## Type and Ease

They are orthogonal. **Type** is the shape of the curve, **Ease** is the direction it is applied in, so `Bounce` + `Out` and `Bounce` + `In` are mirror images of each other. Every type function is an *In* shape, which `Out`, `InOut` and `OutIn` then mirror.

Either half can be replaced by your own function or by an `AnimationCurve`:

```csharp
property.SetType(t => t * t * t);                       // custom type
property.SetEase(myAnimationCurve);                     // curve as ease
property.SetType(myCurve).SetEase(TweenCoreEase.InOut); // curve as type
```

---

## Classes

### TweenCoreManager

Drives every tween from one `Update`. Creates itself on demand, survives scene loads.

**Properties**
- `Instance`
- `IsPlaying`
- `NumTweens`

**Methods**
- `PauseAll()` — pauses the manager, not the individual tweens
- `ResumeAll()`
- `StopAll(bool setToFinalValue = true)`
- `AddTween(TweenCore tween)`
- `RemoveTween(TweenCore tween)`

### TweenCore

Contains and manages one or multiple `TweenCoreProperty`.

**Static methods**
- `CreateTween()`

**Playback**
- `Play()` — also registers the tween with the manager
- `Pause()`
- `Resume()`
- `Stop(bool setToFinalValue = true)` — cancels; chain links that never ran are left untouched
- `Kill()` — same as `Stop(false)`
- `Complete()` — lands every property on its end value, including pending chain links, then ends
- `Restart()`
- `Update(float deltaTime)` — the manager calls this for you

**Building**
- `NewProperty(...)` — 4 overloads
- `AddProperty(TweenCorePropertyBase property)`

**Configuration**
- `SetParallel(bool isParallel)` / `SetChain(bool isChain)` / `Parallel()` / `Chain()`
- `SetLoop(bool isLoop, int numIteration = -1)` — negative is infinite, 0 runs nothing
- `SetUseUnscaledTime(bool useUnscaledTime)`
- `SurviveOnUnload()` / `KillOnUnload()` / `SetSurviveOnUnload(bool survive)`
- `DestroyWhenFinish()` / `DontDestroyWhenFinish()` / `SetDestroyWhenFinish(bool destroy)`
- `DestroyTween()`

**Properties**
`IsPlaying`, `IsPaused`, `HasStarted`, `IsFinished`, `IsParallel`, `IsLoop`, `DestroyOnFinish`, `SurviveOnSceneUnload`, `UseUnscaledTime`, `NumPropertiesFinished`, `NumProperties`, `ElapsedTime`, `NumIteration`, `CurrentIteration`

**Events**
- `OnStart<TweenCore>`
- `OnUpdate<TweenCore>`
- `OnFinish<TweenCore>`
- `OnLoopFinish<TweenCore>`

### TweenCorePropertyBase

Abstract parent of `TweenCoreProperty<TweenValueType>`, so one tween can hold properties of different value types.

**Methods**
- `Update(float deltaTime)`
- `Start()`
- `Stop(bool setToFinalValue = true, bool continueChain = true)`
- `SetToFinalVals()`
- `AddNextProperty(TweenCorePropertyBase property)`
- `ClearNextProperties()`
- `SetBaseValues()`

**Events**
- `OnStart<TweenCorePropertyBase>`
- `OnUpdate<TweenCorePropertyBase>`
- `OnFinish<TweenCorePropertyBase>`

### TweenCoreProperty&lt;TweenValueType&gt;

Computes the current value and, if asked to, writes it to the target.

**Methods**
- `SetDelay(float tweenDelay)`
- `SetType(...)` — 5 overloads (enum, function, curve, and the two enum + value forms)
- `SetEase(...)` — 5 overloads
- `GetCurrentValue()`
- `From(TweenValueType value)`
- `FromCurrent()` — reflection tweens only
- `SetIsAdditive(bool isAdd)` — treats the final value as an offset; also turns `FromCurrent` on
- `Pause()` / `Resume()`
- `Stop(bool setToFinalValue = true, bool continueChain = true)`
- `SetToFinalVals()`

**Properties**
`StartValue`, `FinalValue`, `CurrentValue`, `IsBroken`, plus everything on the base.

**Events**
- `OnUpdateValue<TweenCoreProperty<TweenValueType>, TweenValueType>` — raised once per frame

### TweenCoreComponent

Build a tween from the inspector, no code. Properties are added from the **+** menu on the Tween Properties list, and every field is exposed with `UnityEvent` hooks.

**Methods**
- `Play()`, `Pause()`, `Resume()`, `Restart()`, `Complete()`
- `StopAndSetToFinalValue()`, `StopAndDontChangeValue()`
- `AddProperty(TweenCorePropertyBase property)`

**Properties**
- `Tween` — the underlying `TweenCore`
- `TweenName`

### TweenCoreOps&lt;TweenValueType&gt;

The interpolation layer, resolved once per value type. Useful if you want to check a type before building a tween.

- `Lerp(a, b, weight)`
- `Add(a, b)`
- `IsSupported`
- `SupportsAdditive`

---

## Supported types

**C#** — `float`, `double`, `int`, `uint`, `long`, `ulong`, `decimal`

**Unity** — `Vector2`, `Vector3`, `Vector4`, `Quaternion`, `Color`, `Color32`

Integer types are rounded and clamped, so a curve that overshoots (`Back`, `Elastic`) cannot wrap them around.

All of these work with `SetIsAdditive`. For `Quaternion` the offset is composed rather than added, which is the rotation equivalent.

`decimal` is available from code only — Unity's serializer has no support for it, so it is not offered in the `TweenCoreComponent` inspector. Every other type above is.

---

## Tests

The library ships with an automated suite: **260 EditMode** tests and **22
PlayMode** tests.

**In the editor** — *Window → General → Test Runner*, then the **EditMode** and
**PlayMode** tabs, and *Run All*.

**Headless**, from the repository root:

```powershell
.un-tests.ps1                     # EditMode
.un-tests.ps1 -Platform PlayMode  # PlayMode
.un-tests.ps1 -Platform Both      # both, about 30 s
```

The script finds the editor version pinned in `ProjectSettings/ProjectVersion.txt`
so the project is never opened by a version that would upgrade it, waits for the
run to finish (Unity's batchmode returns to the shell long before it is done),
and exits non-zero on failure. Results and logs land in `TestResults/`.

Close the editor first: a batchmode run takes an exclusive lock on the project.

Every test was written from this document's claims and then proved able to fail,
by breaking the line it covers and confirming it went red. That record is in
`docs/test-evidence/mutation-log.md`, and behaviour found along the way that is
worth a decision is in `docs/open-items.md`.

## Demo scene

`Assets/Scenes/SampleScene.unity` — press Play. `TestTween.cs` builds a chained
tween in code: a position move with `Bounce` / `In`, then a scale from zero with
`Elastic` / `Out` on a one second delay.

---

## Upgrading from 1.0

> **Read this before opening a 1.0 project in 1.1.**
>
> **Do not open and save a 1.0 scene or prefab until you have run the upgrader below.**
>
> 1.0 lived in the global namespace inside `Assembly-CSharp`. 1.1 moved it to the `Tweening` namespace in its own `TweenCore` assembly. Unity records the type of a `[SerializeReference]` field by namespace and assembly, so every property your inspector saved in 1.0 is stored under a name 1.1 cannot resolve. The properties load as `null`, and the tween does nothing — silently, with only a line in the editor log.
>
> The values are still in the file; only the label is wrong. But Unity **discards** managed references it cannot resolve whenever it writes an asset, so the first time you save such a scene the data is genuinely gone and nothing can bring it back.

**The upgrader.** In the menu: **Tools → TweenCore → Upgrade 1.0 scenes and prefabs**. It scans every `.unity` and `.prefab` under `Assets`, shows you what it will touch, and rewrites the stored namespace and assembly. It only touches TweenCore's own references, it leaves everything else in the file byte for byte, and running it twice is safe.

Close any open scene first, and take a backup if the project is not under version control.

*(Earlier documentation claimed `[MovedFrom]` handled this automatically. It does not — the attributes are present but the references are not remapped, so the upgrade must be run explicitly.)*

Source changes you may need to make:

| 1.0 | 1.1 |
| --- | --- |
| *(global namespace)* | `using Tweening;` |
| `SurviveOnSceneLoad()` | `SurviveOnUnload()` |
| `KillOnSceneUnLoad()` | `KillOnUnload()` |
| `ElapseTime` | `ElapsedTime` |

The old names still compile and forward to the new ones, with an `[Obsolete]` warning.

**Two behaviour changes worth knowing about:**

- **`Bounce` is now an *In* shape**, consistent with every other type function. If you had `SetType(Bounce).SetEase(In)` and want the old look, switch to `SetEase(Out)`, and vice versa.
- **`Stop()` no longer fast forwards a chain.** Stopping a chained tween used to start and instantly finish every remaining link. It now cancels; call `Complete()` if you want the old behaviour.

If you subclass `TweenCorePropertyBase` you also need to implement `ClearNextProperties()` and add the `continueChain` parameter to your `Stop` override.

---

## Changelog

### 1.1

**Fixed**
- `int`, `uint`, `long` and `ulong` threw `InvalidCastException` on the first frame; unsigned types also wrapped when tweening downwards.
- `Play()` iterated the property list while zero-duration properties removed themselves from it, throwing `ArgumentOutOfRangeException`.
- The expected property count was taken after properties had already finished, which left a parallel tween of instant properties running forever and cut a chain one link short.
- `Stop()` only stopped the first property whenever properties were not being removed — i.e. every looping tween and every `DontDestroyWhenFinish()` tween.
- Scene unload wrote final values through reflection onto objects the scene had already destroyed.
- A target destroyed part way through a tween threw once per frame; an unresolvable property name did the same.
- `OnUpdateValue` was raised twice per frame.
- The inspector offered read-only properties, which then threw at runtime. It now lists writable properties *and* writable fields, and skips obsolete members.
- Tweens created but never played stayed registered with the manager forever.
- `_canBeInstantiate` was never reset, so with domain reload disabled no tween ran again after the first quit.
- `SetToFinalVals()` used the curve instead of the end value, so a custom `AnimationCurve` that did not land on 1 left the target short. It also threw `KeyNotFoundException` on unsupported types.
- `SetLoop(true, 0)` still started and finalised every property.
- Replaying a tween stacked duplicate chain links.
- The inspector wrote enum *values* into `enumValueIndex`, which only worked by coincidence.
- `TweenCoreComponent.OnDestroy` could throw a `NullReferenceException`.

**Changed**
- Everything moved into the `Tweening` namespace, with `TweenCore` and `TweenCore.Editor` assembly definitions.
- Interpolation no longer boxes: `TweenCoreOps<T>` resolves a typed delegate once per type instead of a dictionary lookup and three boxes per property per frame.
- Reflection tweens bind a delegate to the target once instead of calling `PropertyInfo.SetValue` every frame, falling back to reflection where AOT cannot generate it.
- `Bounce` is an *In* shape; `Stop()` cancels rather than fast forwards.
- `Quaternion` interpolation is unclamped so `Back` and `Elastic` overshoot on rotations like they do everywhere else.
- The property drawer measures and draws in a single pass, and its reflection caches are invalidated when the target changes.
- Adding a property from the inspector is undoable and records a prefab override.

**Added**
- `Kill()`, `Complete()` and `Restart()` on `TweenCore`.
- `SetUseUnscaledTime(bool)` per tween, exposed on `TweenCoreComponent`.
- `Pause()`, `Resume()`, `Restart()` and `Complete()` on `TweenCoreComponent`.
- `Quaternion` and `Color32` support for `SetIsAdditive`.
- `double`, `int`, `uint`, `long`, `ulong`, `Quaternion` and `Color32` in the inspector's type menu.
- `package.json` for UPM, and an EditMode test suite.

---

## Not yet supported

Ping-pong / reverse looping, inserting a property at an absolute time in a sequence, a `FixedUpdate` pump for physics-driven tweens, and looking tweens up by target (`KillAllOn(target)`).

---

*Author: Auguste Paccapelo*
