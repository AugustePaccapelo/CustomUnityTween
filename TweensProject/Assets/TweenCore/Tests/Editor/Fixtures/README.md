# Test fixtures

`SampleScene-v1.0.unity.txt` is a copy of `Assets/Scenes/SampleScene.unity` as it
was authored by TweenCore 1.0: its ten `[SerializeReference]` properties are
recorded as `ns: , asm: Assembly-CSharp`, which 1.1 cannot resolve.

It is kept as `.txt` so Unity does not import it as a scene, and it is what
`AssetUpgraderTests` runs the upgrader against. Do not "fix" it - being broken is
the entire point. The real `SampleScene` can be upgraded freely; this file is the
permanent record of the shape the upgrader has to handle.
