# TweenCore — Retrofitting Test Coverage

**Date:** 2026-08-30
**Status:** Approved design, ready for implementation planning

---

## Goal

Bring TweenCore v1.1 to full behavioural test coverage using a test-first
discipline, without deleting or rewriting any production code.

The library shipped without TDD. Its 25 existing tests are regression tests
pinned to specific bugs found in the v1.1 audit — valuable, but bug-shaped
rather than behaviour-shaped. Large parts of the public surface have no test at
all: the curve functions, the property lifecycle, reflection binding, the
manager, the component, and the editor's property picker.

## Non-goals

- Rewriting production code. The Iron Law of TDD says delete untested code and
  reimplement from the tests; the project owner has explicitly ruled that out.
  Section "The cycle" describes how the test-first guarantee is bought back
  instead.
- Fixing bugs discovered along the way. If a spec-first test fails on its first
  run, the work stops and the discrepancy is reported. Whether to change the
  code or the documentation is the owner's call, not a silent decision.
- Asserting on inspector layout geometry. The property drawer's computed rects
  are excluded deliberately: pixel assertions break on every spacing tweak and
  test the drawing, not a decision.
- Performance regression tests. The v1.1 work removed the boxing in the hot
  path; guarding that needs a benchmark harness, which is a separate project.

## Constraints

| Constraint | Source |
|---|---|
| No production code is deleted or rewritten | Owner instruction |
| Coverage reaches layers A + B, plus the editor's filtering rules only | Owner decision |
| Every test carries mutation evidence that it can fail | Owner decision |
| Tests are written from documented behaviour, not from the implementation | Owner decision |
| A PlayMode suite is included | Owner decision |
| Work lands in reviewable slices | Owner decision |
| The repo is not under version control | Observed |

The last one matters: there is no `git checkout` to fall back on. Every
mutation must be restored by writing the original text back, and each cycle
must end with the production file verified byte-identical to how it started.

## Test execution

**Editor:** Unity 6000.0.59f2, installed at
`C:\Program Files\Unity\Hub\Editor\6000.0.59f2\`. This matches
`TweensProject/ProjectSettings/ProjectVersion.txt` exactly, so the project opens
without an upgrade prompt. A second editor (6000.5.10f1) exists on the machine
for unrelated work and is not used here.

**EditMode:**

```
Unity.exe -runTests -batchmode -projectPath TweensProject \
  -testPlatform EditMode -testResults results-editmode.xml
```

**PlayMode:**

```
Unity.exe -runTests -batchmode -projectPath TweensProject \
  -testPlatform PlayMode -testResults results-playmode.xml
```

Results are read from the NUnit XML, not from console output.

**Unity.exe detaches.** On Windows it returns to the shell immediately, long
before the run finishes, with an empty exit code. Any wrapper that trusts that
return will report success on a run that has not started yet. Every invocation
must instead wait on the process object until `HasExited`, then read the results
XML — and treat a missing XML as a failure, not as an empty pass. The same trap
applies to the Hub's headless installer.

**Determinism.** `TweenCore.Update(float deltaTime)` and
`TweenCorePropertyBase.Update(float deltaTime)` both take their delta as a
parameter. Tests drive time by hand in fixed steps and never depend on
`Time.deltaTime`, real elapsed time, or the manager's pump. This is what makes
the great majority of the library testable in EditMode with no frame-waiting,
and it is a rule for every new test, not merely a convenience.

**Exclusive access.** A batchmode run holds a lock on the project folder. Tests
cannot run while the project is open in the Unity editor GUI.

## The cycle

For each behaviour, in this order:

1. **Write the test from the claim, not the code.** The source of truth is
   `README.md` and the XML doc comments on the member under test. The test
   asserts what the library *says* it does.
2. **Run it.** Record the result.
3. **If it fails:** stop. This is either a genuine bug or a documentation lie.
   Report it, with the failing assertion and the claim it came from, and wait
   for a decision. Do not adjust the test to match the code.
4. **If it passes:** it has proved nothing yet. Break the exact production line
   the test targets — one edit, minimal, aimed at that behaviour.
5. **Re-run the suite.** Confirm the target test goes red, that it goes red for
   the expected reason, and that no unrelated test breaks in a way that suggests
   the mutation was too broad.
6. **Restore** the production file and verify it is byte-identical to the
   original.
7. **Re-run** to confirm the suite is green again.

Mutations are applied and reverted one at a time. Each is recorded in
`docs/test-evidence/mutation-log.md` with: the test name, the file and line
mutated, the edit made, the observed failure message, and confirmation of
restoration. The log is the evidence that the suite bites; without it, the claim
of test-first compliance is unverifiable.

**Why this is not theatre.** A test written after the code and never seen to
fail may assert the wrong thing, assert on the implementation rather than the
behaviour, or pass for a reason unrelated to what it names. Steps 4-6 restore
exactly the guarantee that writing the test first would have provided: proof
that this specific test detects the absence of this specific behaviour.

## Layout

```
TweensProject/Assets/TweenCore/Tests/
  Editor/
    TweenCore.Tests.Editor.asmdef        (exists)
    _Support/
      FakeTweenTarget.cs                 writable properties, read-only
                                         properties, fields, an obsolete member
      TweenDriver.cs                     advances a tween in fixed steps
      CurveAssertions.cs                 endpoint, monotonicity, mirror helpers
    TweenCoreOpsTests.cs                 (exists, 11 tests)
    TweenCoreSequencingTests.cs          (exists, 14 tests)
    CurveShapeTests.cs                   new
    PropertyLifecycleTests.cs            new
    PropertyValueSourceTests.cs          new
    ReflectionBindingTests.cs            new
    ChainAndParallelTests.cs             new
    LoopTests.cs                         new
    ManagerTests.cs                      new
    ComponentTests.cs                    new
    EditorPickerFilteringTests.cs        new
  Runtime/
    TweenCore.Tests.Runtime.asmdef       new, PlayMode only
    ManagerPumpTests.cs                  new
    SceneUnloadTests.cs                  new
```

The two existing files are kept unchanged. They record specific defects and
serve a different purpose from behavioural coverage; new tests are added
alongside them rather than folded into them.

`_Support` holds only fixtures shared across at least two test files. A helper
used by one file stays in that file.

## Coverage inventory

Counts are estimates for planning and will move as tests are written. They are
not commitments.

| Slice | Area | Existing | New | What is asserted |
|---|---|---|---|---|
| 1 | `TweenCoreOps<T>` | 11 | ~10 | Every documented type lerps; `Quaternion` additive composes rather than adds; `IsSupported` and `SupportsAdditive` agree with the README's type table; unsupported types fail with a clear message |
| 1 | Curve shapes | 0 * | ~25 | All 13 types start at 0 and end at 1; every type function is an *In* shape; `Out`/`InOut`/`OutIn` mirror it correctly; custom function and `AnimationCurve` replace either half |
| 2 | Property lifecycle | 0 | ~25 | `SetDelay` defers the first write; `Pause`/`Resume` preserve elapsed time; `OnStart`/`OnUpdate`/`OnFinish`/`OnUpdateValue` fire the documented number of times; zero duration completes inside `Start()` |
| 2 | Value sources | 0 | ~12 | `From` overrides the start value; `FromCurrent` reads the target at `Play()`; `SetIsAdditive` treats the final value as an offset and turns `FromCurrent` on |
| 3 | Sequencing | 14 | ~12 | Parallel starts all, chain starts one; `Stop` cancels and does not fast-forward; `Complete` lands pending links; `Restart` replays without stacking links |
| 3 | Loops | 0 | ~8 | `SetLoop(true, n)` runs exactly n iterations; negative is infinite; 0 runs nothing and writes nothing; `OnLoopFinish` fires per iteration |
| 4 | Reflection binding | 0 | ~15 | Real `Transform` and `Renderer` targets are written; the `TweenCoreTarget` constants resolve; a destroyed target ends quietly; an unresolvable name marks the property broken instead of throwing per frame |
| 5 | `TweenCoreManager` | 0 | ~12 | Creates itself on demand; `AddTween`/`RemoveTween` and `NumTweens`; `PauseAll` pauses the manager rather than each tween; `StopAll`; tweens register on `Play()`, not on construction |
| 5 | `TweenCoreComponent` | 0 | ~10 | Each public method forwards to the underlying tween; `OnDestroy` does not throw |
| 6 | Editor picker filtering | 0 | ~10 | Read-only properties are excluded; writable fields are included; obsolete members are skipped; the type menu offers exactly the README's serializable types and excludes `decimal` |
| 7 | PlayMode | 0 | ~8 | The manager's `Update` pump advances registered tweens across real frames; scene unload calls `Stop(false)` and writes nothing through reflection to destroyed objects; `SurviveOnUnload` survives |

\* Two of the existing sequencing tests touch curve shape. They are counted once,
under Sequencing, so the column sums correctly.

Approximate totals: **25 existing, ~147 new, ~172 in the finished suite.**

## Slice order and exit criteria

Slices are ordered so that the most self-contained logic is covered first and
the most environment-dependent last. Each slice is a reviewable checkpoint.

1. Type operations and curve shapes
2. Property lifecycle and value sources
3. Sequencing, chains, and loops
4. Reflection binding
5. Manager and component
6. Editor picker filtering
7. PlayMode: manager pump and scene unload

A slice is complete when: every test in it passes; every test in it has a
mutation-log entry showing it failed when its target behaviour was broken; every
mutated file is verified restored; the full suite is green; and the run produces
no console errors or warnings.

Work stops at the end of a slice for review before the next begins.

## Risks

**A spec-first test fails and the answer is ambiguous.** The README may be
imprecise rather than wrong. Handling: report the exact claim and the exact
assertion, propose both readings, and let the owner choose. Never resolve it
silently.

**Mutation restoration corrupts a file.** There is no version control to fall
back on. Handling: before the first mutation of any file, copy it to
`docs/test-evidence/originals/`; after each restore, compare against that copy
and fail loudly on any difference.

**Runtime — measured, not estimated.** Baseline on 2026-08-30: a cold run
(first import) took 97.9 s; a warm run takes **10.1 s** wall clock for the full
suite, of which the tests themselves are 0.04 s. Unity startup dominates, and
the suite's own cost is negligible, so run time will stay near flat as tests are
added.

Each mutation cycle needs three runs (confirm green, confirm red, confirm
restored green), so ~30 s per test and roughly **75 minutes of compute for the
whole suite** — unattended and backgroundable. This is no longer a material
risk, and the contingency of batching mutations to save runs is not needed;
mutations stay one at a time, which is the stronger form.

**PlayMode flakiness.** Frame-dependent tests are the usual source of
intermittent failures. Handling: assert on accumulated state after a known
number of frames, never on wall-clock timing, and keep the PlayMode suite as
small as the two behaviours that require it.

**No version control — decided.** The owner has chosen not to initialise git for
now. That is a workable call, but it changes the status of the `originals/`
backup described above: it stops being a redundant safety net and becomes the
*only* thing standing between a botched restore and lost source.

Consequences accepted deliberately:

- The backup copy is written before the first mutation of a file and verified
  after every restore. A byte mismatch halts the slice immediately rather than
  continuing and compounding.
- `docs/test-evidence/originals/` must not be deleted while any slice is in
  progress.
- There is no way to review the test suite as a diff, so review happens at slice
  boundaries against the spec instead.

Revisiting this later is cheap and loses nothing already done.
