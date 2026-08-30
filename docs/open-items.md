# Open items

Decisions deferred rather than forgotten. Each records what was found, why it
was left alone, and what deciding it would involve.

## Quaternion interpolation uses LerpUnclamped, not SlerpUnclamped

**Found:** 2026-08-30, during slice 1 of the test coverage work.
**Status:** Logged, no change made. Owner's call, taken deliberately.

`TweenCoreOps.cs:148` resolves `Quaternion` interpolation to
`Quaternion.LerpUnclamped` — a component-wise lerp with a normalise, not a
spherical interpolation.

The README's claim is satisfied: *"Quaternion interpolation is unclamped so
`Back` and `Elastic` overshoot on rotations like they do everywhere else."* It
does overshoot. `Quaternion_InterpolationIsUnclamped_SoOvershootingCurvesWork`
asserts exactly that documented property and passes, and is written to hold
under either implementation.

**What is different from a spherical interpolation:**

- Angular velocity is not uniform. A rotation tween on a linear curve does not
  turn at an even rate; the effect grows with the angle and is negligible for
  small ones.
- Overshoot is compressed. From identity to a 90° yaw, weight 2 reaches about
  147°, where a spherical interpolation reaches 180°. `Back` and `Elastic`
  therefore overshoot rotations less than the curve shape implies.

**If it were changed:** switching to `Quaternion.SlerpUnclamped` is a one line
edit, but it changes shipped v1.1 behaviour for anyone tweening `Quaternion`,
so it belongs in a release with a changelog entry rather than in test work. It
would also cost slightly more per frame. The existing test would still pass; a
new test pinning the 180° figure would be the way to lock the new semantics in.

**Why it was left:** the coverage work is scoped to covering the code, not
changing it. Fixing behaviour discovered along the way would mean the test suite
and the library moved at the same time, which is precisely what makes retrofitted
tests untrustworthy.

## TweenCoreComponent.AddProperty rejects code-created manual and function properties

**Found:** 2026-08-30, during slice 5 of the test coverage work.
**Status:** Reported, not fixed. Scope of this work is covering the code, not changing it.

`TweenCoreProperty.SetBaseValues()` (`TweenCoreProperty.cs:170`) chooses between the
reflection path and the manual path using the serialized `isEmpty` flag:

```csharp
if (isEmpty)  _currentMethod = MethodUse.ReturnValue;
else          SetReflectionFields(propertyName);
```

`isEmpty` is only ever written by the inspector's property drawer. The four
code constructors set `_currentMethod` directly and never touch it, so it is
`false` for every property built in code.

`TweenCoreComponent.Start()` calls `SetBaseValues()` on each property it holds.
So this documented sequence:

```csharp
component.AddProperty(new TweenCoreProperty<float>(0f, 10f, 1f));  // manual
```

takes the reflection branch with a null target and an empty member name, and on
the object's first frame logs:

```
TweenCore : the object to tween is null.
```

It also leaves `IsBroken` reporting `true`.

**Impact.** Cosmetic but misleading rather than fatal. `_currentMethod` keeps the
value its constructor set, and neither the `ReturnValue` nor the `Strategy` write
path consults `_isBroken`, so the tween still animates correctly. What the user
gets is an error in the console on every start and a public `IsBroken` property
that lies. Inspector-authored properties are unaffected, which is why this has
not been noticed: `isEmpty` is correct for every property the drawer creates.

**Reproduction.** Three PlayMode tests hit it before being switched to reflection
properties: `AddProperty_WiresThePropertyIntoTheTweenOnStart`,
`ByDefault_TheTweenPlaysOnStart`, `TheManagerDrivesTheComponentsTweenAcrossFrames`.

**Possible fixes, for the owner to choose between.**

1. Have `SetBaseValues()` branch on `_currentMethod` when it is already set, and
   fall back to `isEmpty` only for deserialized properties. Smallest change,
   keeps scenes loading identically.
2. Have the manual and function constructors set `isEmpty = true`. Simpler to
   read, but `isEmpty` is serialized, so its meaning would shift from "the
   inspector marked this empty" to "this property has no reflection target".
3. Document `AddProperty` as inspector-only and have it reject code-created
   manual properties loudly. Least work, most restrictive.

A regression test should be added with whichever fix is chosen; there is no test
pinning this today, because pinning current behaviour would mean asserting the
spurious error.

## The picker's `field.IsLiteral` check is unreachable

**Found:** 2026-08-30, during slice 6 of the test coverage work.
**Status:** **FIXED** 2026-08-30. The unreachable half of the guard was deleted, which also made `AConstField_IsNotOffered` provable for the first time: mutation M141 adds `BindingFlags.Static` and the test goes red. Every assertion in the suite now carries mutation evidence.

`TweenCorePropertyBaseEditor.GetTweenableMemberNames` filters candidate fields
with:

```csharp
if (field.IsInitOnly || field.IsLiteral) continue;
```

`IsLiteral` is true only for `const` fields, and `const` fields are implicitly
**static**. The enumeration above it uses:

```csharp
const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
```

so `GetFields(flags)` never returns a `const` field to begin with. The
`IsLiteral` half of the guard is dead code.

**Consequence for the tests.** `AConstField_IsNotOffered` asserts a real and
desirable behaviour - a `const` is never offered - and it passes. But it cannot
be given mutation evidence: removing `IsLiteral` changes nothing (M126 left the
suite green), and adding `BindingFlags.Static` would not break it either,
because `IsLiteral` would then do the filtering. The behaviour is protected by
two independent mechanisms, so no single mutation can falsify the test. This is
recorded rather than papered over: the test is kept because the behaviour is
worth pinning, and the log marks it as the one assertion in the suite without
mutation evidence.

**If it were changed:** deleting `|| field.IsLiteral` is safe today and makes the
guard honest. Keeping it is also defensible as protection against someone later
adding `BindingFlags.Static` to the flags. Either is fine; the point is that it
currently reads as a live filter and is not one.

## v1.0 scenes lose their inspector-authored tweens (the `[MovedFrom]` remap does not work)

**Found:** 2026-08-30, opening `SampleScene` to demo the library.
**Status:** **FIXED** 2026-08-30 by `TweenCoreAssetUpgrader`
(*Tools -> TweenCore -> Upgrade 1.0 scenes and prefabs*), written test-first with
10 tests and 7 verified mutations. The README now carries the warning and the
instructions. What remains open is only item 2 below - whether `[MovedFrom]`
could be made to work, which would remove the need for the upgrade step at all.

This was the most serious of the findings here.

### What happens

`SampleScene` plays and nothing animates. The editor log says, once per component:

```
Missing types referenced from component TweenCoreComponent on game object StayObj (3):
	TweenCoreProperty`1[[UnityEngine.Vector3, UnityEngine.CoreModule]], Assembly-CSharp (2 objects)
```

Every `[SerializeReference]` entry in the scene is recorded as:

```yaml
type: {class: 'TweenCoreProperty`1[[UnityEngine.Vector3, UnityEngine.CoreModule]]', ns: , asm: Assembly-CSharp}
```

`ns:` is empty - the global namespace - and `asm:` is `Assembly-CSharp`, which is
where the type lived in v1.0. Since v1.1 it is `Tweening.TweenCoreProperty<T>` in
the `TweenCore` assembly. Unity cannot resolve the old identifier, so each
property deserializes as `null`, `TweenCoreComponent` starts a tween with no
properties, and nothing moves. All ten references in this scene are affected.

### Why it matters

`README.md` states, under *Upgrading from 1.0*:

> Existing scenes and prefabs keep working: the serialized properties carry
> `[MovedFrom]` so Unity remaps them to the new namespace and assembly on first
> import.

They do not. Every scene or prefab authored with the v1.0 inspector component
silently loses its tweens on upgrade - no exception, no error in the Console for
a user who is not reading the editor log, just an object that stops animating.
For a released package this is the worst shape of defect: silent data loss on
upgrade.

The attributes are present and look correct:

```csharp
[MovedFrom(autoUpdateAPI: true, sourceNamespace: null,
           sourceAssembly: "Assembly-CSharp", sourceClassName: null)]
```

on both `TweenCoreProperty<T>` and `TweenCorePropertyBase`.

### What is confirmed, and what is not

**Confirmed:** the stored identifiers point at the old namespace and assembly,
Unity reports them as missing, and the properties come back null.

**Not confirmed:** *why* `[MovedFrom]` does not take effect. The most likely
explanation is that it does not apply to **generic** types behind
`[SerializeReference]` - the stored class is ``TweenCoreProperty`1[[...]]`` - but
that hypothesis has not been tested. `TweenCorePropertyBase` is non-generic and
carries the same attribute; whether it remaps correctly was not isolated.

### Tested: the data is intact, and rewriting the identifier restores it

A probe copied `SampleScene`, rewrote only the managed-reference type identifiers
from the v1.0 form to the v1.1 one:

```yaml
# from
type: {class: 'TweenCoreProperty`1[[UnityEngine.Vector3, UnityEngine.CoreModule]]', ns: , asm: Assembly-CSharp}
# to
type: {class: 'TweenCoreProperty`1[[UnityEngine.Vector3, UnityEngine.CoreModule]]', ns: Tweening, asm: TweenCore}
```

then opened both scenes and counted how many properties survived deserialization:

| Scene | Deserialized |
|---|---|
| `SampleScene.unity` as shipped | **0 of 12** |
| The same scene, identifiers rewritten | **10 of 12** |

The two that did not come back are empty slots on the inactive `GameObject`: the
scene holds ten real managed references and two unconfigured entries. **All ten
real references were restored.** The values were never lost - only the label
saying which type they belong to was wrong.

This makes a migration utility a *proven* fix rather than a hopeful one.

### Directions

1. **A migration utility - DONE.** `TweenCoreAssetUpgrader.UpgradeText` rewrites
   the `ns:` / `asm:` of matching `RefIds`; the menu item walks every `.unity` and
   `.prefab` under `Assets` and previews before writing. Guarded by 10 tests,
   including one that upgrades the committed `SampleScene` and asserts all ten
   references deserialize. Seven mutations verify them (M134-M140).
2. **Make `[MovedFrom]` work, if it can be.** Strictly better if achievable,
   because users would need to do nothing at all. Untested: the probe measured
   the *fix*, not the *cause*, since a working fix makes the cause academic. The
   failure is total - 0 of 12, not a partial remap - which suggests the attribute
   is not being applied to these managed references at all. Whether that is
   because the type is generic is still unverified.
3. **A compatibility shim.** A non-generic type in `Assembly-CSharp` that
   deserializes the old data and converts it. Most machinery, permanently, for a
   one-time problem. Not recommended.

Whichever is chosen, it needs a regression test - and the fixture already exists:
`SampleScene.unity` is committed in the broken v1.0 form, so a test can assert
that its properties deserialize. The current suite cannot catch this because
every test builds its properties in code and never deserializes an asset.

### Urgent, ahead of any code change

**Do not open and save a v1.0 scene or prefab under v1.1.** The data survives
today only because nothing has re-saved it. Unity drops unresolvable managed
references when it writes an asset, and at that point the values really are gone
and no migration can recover them. This warning belongs at the top of the README
now, before any fix ships.

### Unrelated, but also true of this scene

`Tester` - the GameObject carrying `TestTween.cs`, the code-driven demo - is
saved **inactive**, along with `Square`, `StartPos`, `EndPos` and four of the
five `StayObj` objects. That is scene state, not a defect. Enabling `Tester`,
`Square`, `StartPos` and `EndPos` makes the code path animate, because code-built
tweens never go through serialization and are unaffected by the problem above.
