# Mutation evidence log

Every test in this suite was written first from the README's claims and the XML
doc comments, run, and then proved capable of failing: the production line it
targets was broken, the suite re-run, and the file restored from `originals/`
and checked byte-identical.

A test that cannot fail is not evidence of anything. This log is what makes the
test-first claim auditable rather than asserted.

**Method.** One mutation at a time. Restore is a file copy from
`docs/test-evidence/originals/`, never a reverse edit, so a botched replacement
cannot survive a cycle. `restored=True` means the MD5 matched the pristine copy
afterwards.

**Environment.** Unity 6000.0.59f2, headless. EditMode and PlayMode warm runs are
both about 10 s. Each batch runs against the platform its tests live on.

**Limit of the method, stated plainly.** This proves each test *can* fail when
the behaviour it names is broken. It does not prove the tests catch every way
that behaviour could break, nor that the suite is complete. M74, M94, M128 and
M136 are worked examples of that limit being hit and then closed. M126 was a
fifth that could not be closed while the code stood; removing the dead guard it
targeted made the assertion provable, and M141 now carries its evidence.

## Slice 1 - type operations and curve shapes

EditMode. Suite: 136 tests.

| # | File | Mutation | Suite failed | Tests that went red | Restored |
|---|---|---|---|---|---|
| M01 | `TweenCorePropertyBase.cs` | Out no longer mirrors In | 31 | `CurveShapeTests.CustomEaseFunction_ReplacesTheDirection`<br>`CurveShapeTests.Out_IsFrontLoaded`<br>`CurveShapeTests.Out_IsTheMirrorOfIn`<br>`CurveShapeTests.OutIn_IsTheOutShapeCompressedIntoTheFirstHalf` | True |
| M02 | `TweenCorePropertyBase.cs` | InOut second half misses the midpoint | 11 | `CurveShapeTests.InOut_PassesThroughTheMidpoint` | True |
| M03 | `TweenCorePropertyBase.cs` | InOut first half not compressed 2x | 11 | `CurveShapeTests.InOut_IsTheInShapeCompressedIntoTheFirstHalf` | True |
| M04 | `TweenCorePropertyBase.cs` | OutIn first half not compressed 2x | 11 | `CurveShapeTests.OutIn_IsTheOutShapeCompressedIntoTheFirstHalf` | True |
| M05 | `TweenCorePropertyBase.cs` | OutIn second half misses the midpoint | 11 | `CurveShapeTests.OutIn_PassesThroughTheMidpoint` | True |
| M06 | `TweenCorePropertyBase.cs` | Quad is front loaded instead of back loaded | 3 | `CurveShapeTests.EveryBuiltInType_IsAnInShape`<br>`CurveShapeTests.EveryBuiltInTypeExceptLinear_IsStrictlyBackLoaded`<br>`CurveShapeTests.Out_IsFrontLoaded` | True |
| M08 | `TweenCorePropertyBase.cs` | Expo is front loaded | 3 | `CurveShapeTests.EveryBuiltInType_IsAnInShape`<br>`CurveShapeTests.EveryBuiltInTypeExceptLinear_IsStrictlyBackLoaded`<br>`CurveShapeTests.Out_IsFrontLoaded` | True |
| M09 | `TweenCoreProperty.cs` | custom type function ignored | 2 | `CurveShapeTests.CustomTypeFunction_ReplacesTheShape`<br>`CurveShapeTests.SetType_WithEnumAndFunction_UsesTheFunction` | True |
| M10 | `TweenCoreProperty.cs` | custom type AnimationCurve ignored | 2 | `CurveShapeTests.AnimationCurveAsType_ReplacesTheShape`<br>`CurveShapeTests.SetType_WithEnumAndCurve_UsesTheCurve` | True |
| M11 | `TweenCoreProperty.cs` | custom ease function ignored | 3 | `CurveShapeTests.CustomEaseFunction_CanIgnoreTheTypeEntirely`<br>`CurveShapeTests.CustomEaseFunction_ReplacesTheDirection`<br>`CurveShapeTests.SetEase_WithEnumAndFunction_UsesTheFunction` | True |
| M12 | `TweenCoreProperty.cs` | custom ease AnimationCurve ignored | 2 | `CurveShapeTests.AnimationCurveAsEase_ReplacesTheDirection`<br>`CurveShapeTests.SetEase_WithEnumAndCurve_UsesTheCurve` | True |
| M13 | `TweenCorePropertyBase.cs` | Linear is not the identity | 4 | `CurveShapeTests.AnimationCurveAsEase_ReplacesTheDirection`<br>`CurveShapeTests.EveryBuiltInType_IsAnInShape`<br>`CurveShapeTests.Linear_IsTheIdentity`<br>`CurveShapeTests.SetEase_WithEnumAndCurve_UsesTheCurve` | True |
| M14 | `TweenCorePropertyBase.cs` | Cubic front loaded | 3 | `CurveShapeTests.EveryBuiltInType_IsAnInShape`<br>`CurveShapeTests.EveryBuiltInTypeExceptLinear_IsStrictlyBackLoaded`<br>`CurveShapeTests.Out_IsFrontLoaded` | True |
| M15 | `TweenCorePropertyBase.cs` | Quart front loaded | 3 | `CurveShapeTests.EveryBuiltInType_IsAnInShape`<br>`CurveShapeTests.EveryBuiltInTypeExceptLinear_IsStrictlyBackLoaded`<br>`CurveShapeTests.Out_IsFrontLoaded` | True |
| M16 | `TweenCorePropertyBase.cs` | Quint front loaded | 3 | `CurveShapeTests.EveryBuiltInType_IsAnInShape`<br>`CurveShapeTests.EveryBuiltInTypeExceptLinear_IsStrictlyBackLoaded`<br>`CurveShapeTests.Out_IsFrontLoaded` | True |
| M17 | `TweenCorePropertyBase.cs` | Back front loaded | 3 | `CurveShapeTests.EveryBuiltInType_IsAnInShape`<br>`CurveShapeTests.EveryBuiltInTypeExceptLinear_IsStrictlyBackLoaded`<br>`CurveShapeTests.Out_IsFrontLoaded` | True |
| M18 | `TweenCorePropertyBase.cs` | Elastic front loaded | 3 | `CurveShapeTests.EveryBuiltInType_IsAnInShape`<br>`CurveShapeTests.EveryBuiltInTypeExceptLinear_IsStrictlyBackLoaded`<br>`CurveShapeTests.Out_IsFrontLoaded` | True |
| M19 | `TweenCorePropertyBase.cs` | Bounce front loaded (not mirrored to In) | 4 | `CurveShapeTests.EveryBuiltInType_IsAnInShape`<br>`CurveShapeTests.EveryBuiltInTypeExceptLinear_IsStrictlyBackLoaded`<br>`CurveShapeTests.Out_IsFrontLoaded`<br>`TweenCoreSequencingTests.Bounce_IsAnInShape_LikeEveryOtherTypeFunction` | True |
| M20 | `TweenCorePropertyBase.cs` | Circ front loaded | 3 | `CurveShapeTests.EveryBuiltInType_IsAnInShape`<br>`CurveShapeTests.EveryBuiltInTypeExceptLinear_IsStrictlyBackLoaded`<br>`CurveShapeTests.Out_IsFrontLoaded` | True |
| M21 | `TweenCorePropertyBase.cs` | Sine front loaded | 3 | `CurveShapeTests.EveryBuiltInType_IsAnInShape`<br>`CurveShapeTests.EveryBuiltInTypeExceptLinear_IsStrictlyBackLoaded`<br>`CurveShapeTests.Out_IsFrontLoaded` | True |
| M22 | `TweenCoreOps.cs` | float lerp halved | 52 | `CurveShapeTests.AnimationCurveAsEase_ReplacesTheDirection`<br>`CurveShapeTests.AnimationCurveAsType_ReplacesTheShape`<br>`CurveShapeTests.CustomEaseFunction_CanIgnoreTheTypeEntirely`<br>`CurveShapeTests.CustomTypeFunction_ReplacesTheShape`<br>`CurveShapeTests.InOut_PassesThroughTheMidpoint`<br>`CurveShapeTests.Linear_IsTheIdentity`<br>`CurveShapeTests.Out_IsFrontLoaded`<br>`CurveShapeTests.Out_IsTheMirrorOfIn`<br>`CurveShapeTests.OutIn_PassesThroughTheMidpoint`<br>`CurveShapeTests.SetEase_WithEnumAndCurve_UsesTheCurve`<br>`CurveShapeTests.SetEase_WithEnumAndFunction_UsesTheFunction`<br>`CurveShapeTests.SetType_WithEnumAndCurve_UsesTheCurve`<br>`CurveShapeTests.SetType_WithEnumAndFunction_UsesTheFunction`<br>`TweenCoreOpsBehaviourTests.Float_InterpolatesLinearly`<br>`TweenCoreOpsBehaviourTests.Lerp_AtTheEndpoints_ReturnsThoseValuesExactly` | True |
| M23 | `TweenCoreOps.cs` | double lerp halved | 1 | `TweenCoreOpsBehaviourTests.Double_InterpolatesLinearly` | True |
| M24 | `TweenCoreOps.cs` | Vector2 lerp halved | 1 | `TweenCoreOpsBehaviourTests.Vector2_InterpolatesComponentwise` | True |
| M25 | `TweenCoreOps.cs` | Vector3 lerp halved | 2 | `TweenCoreOpsBehaviourTests.Lerp_AtTheEndpoints_ReturnsThoseValuesExactly`<br>`TweenCoreOpsTests.Vector3_AllowsOvershoot` | True |
| M26 | `TweenCoreOps.cs` | Vector4 lerp halved | 1 | `TweenCoreOpsBehaviourTests.Vector4_InterpolatesComponentwise` | True |
| M27 | `TweenCoreOps.cs` | Color lerp halved | 1 | `TweenCoreOpsBehaviourTests.Color_InterpolatesComponentwise` | True |
| M28 | `TweenCoreOps.cs` | Vector3 add drops the second operand | 1 | `TweenCoreOpsBehaviourTests.Add_IsComponentwiseForVectors` | True |
| M29 | `TweenCoreOps.cs` | int add drops the second operand | 1 | `TweenCoreOpsBehaviourTests.Add_SumsTheNumericTypes` | True |
| M30 | `TweenCoreOps.cs` | ulong loses additive support | 1 | `TweenCoreOpsBehaviourTests.SupportsAdditive_IsTrueForEveryDocumentedType` | True |
| M31 | `TweenCoreOps.cs` | Quaternion add stops composing | 1 | `TweenCoreOpsBehaviourTests.Quaternion_AdditiveComposesTheRotationRatherThanAddingComponents` | True |
| M32 | `TweenCoreOps.cs` | Quaternion lerp becomes clamped | 1 | `TweenCoreOpsBehaviourTests.Quaternion_InterpolationIsUnclamped_SoOvershootingCurvesWork` | True |
| M33 | `TweenCoreOps.cs` | IsSupported claims every type | 3 | `TweenCoreOpsBehaviourTests.AnUndocumentedType_SupportsNeitherLerpNorAdditive`<br>`TweenCoreOpsTests.UnsupportedType_ReportsItself`<br>`TweenCoreSequencingTests.UnsupportedValueType_FailsFastWithAClearMessage` | True |

## Slice 2 - property lifecycle and value sources

EditMode. Suite: 177 tests.

| # | File | Mutation | Suite failed | Tests that went red | Restored |
|---|---|---|---|---|---|
| M34 | `TweenCoreProperty.cs` | SetDelay stores nothing | 3 | `PropertyLifecycleTests.SetDelay_IsExposedAsDelay`<br>`PropertyLifecycleTests.SetDelay_PushesCompletionOutByTheDelay`<br>`PropertyLifecycleTests.SetDelay_ShiftsTheAnimationRatherThanCompressingIt` | True |
| M35 | `TweenCoreProperty.cs` | weight ignores the delay | 2 | `PropertyLifecycleTests.SetDelay_PushesCompletionOutByTheDelay`<br>`PropertyLifecycleTests.SetDelay_ShiftsTheAnimationRatherThanCompressingIt` | True |
| M36 | `TweenCore.cs` | Play no longer guards a replay | 87 | `CurveShapeTests.AnimationCurveAsEase_ReplacesTheDirection`<br>`CurveShapeTests.AnimationCurveAsType_ReplacesTheShape`<br>`CurveShapeTests.CustomEaseFunction_CanIgnoreTheTypeEntirely`<br>`CurveShapeTests.CustomTypeFunction_ReplacesTheShape`<br>`CurveShapeTests.CustomTypeFunction_StillLandsExactlyOnTheFinalValue`<br>`CurveShapeTests.InOut_PassesThroughTheMidpoint`<br>`CurveShapeTests.Linear_IsTheIdentity`<br>`CurveShapeTests.Out_IsFrontLoaded`<br>`CurveShapeTests.Out_IsTheMirrorOfIn`<br>`CurveShapeTests.OutIn_PassesThroughTheMidpoint`<br>`CurveShapeTests.SetEase_WithEnumAndCurve_UsesTheCurve`<br>`CurveShapeTests.SetEase_WithEnumAndFunction_UsesTheFunction`<br>`CurveShapeTests.SetType_WithEnumAndCurve_UsesTheCurve`<br>`CurveShapeTests.SetType_WithEnumAndFunction_UsesTheFunction`<br>`PropertyLifecycleTests.AZeroDurationProperty_HoldsItsFinalValueImmediately`<br>`PropertyLifecycleTests.ElapsedTime_AccumulatesTheDeltasPassedIn`<br>`PropertyLifecycleTests.NumProperties_CountsWhatWillRun`<br>`PropertyLifecycleTests.Play_ATweenAlreadyPlaying_DoesNotRestartIt`<br>`PropertyLifecycleTests.Play_MarksTheTweenStartedAndPlaying`<br>`PropertyLifecycleTests.PropertyOnFinish_FiresOnce_WhenThePropertyCompletes`<br>`PropertyLifecycleTests.PropertyOnStart_FiresOnce_WhenThePropertyStarts`<br>`PropertyLifecycleTests.PropertyOnUpdate_FiresOncePerFrameWhileRunning`<br>`PropertyLifecycleTests.PropertyPause_StopsThatPropertyOnly`<br>`PropertyLifecycleTests.Resume_ContinuesFromWhereItPaused`<br>`PropertyLifecycleTests.SetDelay_PushesCompletionOutByTheDelay`<br>`PropertyLifecycleTests.SetDelay_ShiftsTheAnimationRatherThanCompressingIt`<br>`PropertyLifecycleTests.Stop_ClearsPlayingAndMarksFinished`<br>`PropertyLifecycleTests.Stop_WithSetToFinalValue_LandsOnTheFinalValue`<br>`PropertyLifecycleTests.TweenOnFinish_FiresWhenTheTweenEnds`<br>`PropertyLifecycleTests.TweenOnStart_FiresOnPlay`<br>`PropertyLifecycleTests.TweenOnUpdate_FiresOncePerUpdateCall`<br>`PropertyValueSourceTests.Additive_ComposesAQuaternionOffset`<br>`PropertyValueSourceTests.Additive_HalfwayIsHalfTheOffset`<br>`PropertyValueSourceTests.Additive_OffsetsAColor`<br>`PropertyValueSourceTests.Additive_OffsetsAVector3`<br>`PropertyValueSourceTests.From_ChangesWhereTheAnimationBegins`<br>`PropertyValueSourceTests.From_ThenAdditive_OffsetsFromTheGivenStartValue`<br>`PropertyValueSourceTests.SetIsAdditive_False_RestoresAbsoluteBehaviour`<br>`PropertyValueSourceTests.SetIsAdditive_TreatsTheFinalValueAsAnOffset`<br>`PropertyValueSourceTests.WithoutSetIsAdditive_TheFinalValueIsAbsolute`<br>`TweenCoreSequencingTests.EveryTypeAndEase_LandsOnTheFinalValue`<br>`TweenCoreSequencingTests.FinalValueIsExact_EvenWhenACustomCurveDoesNotEndAtOne`<br>`TweenCoreSequencingTests.Loop_RunsTheRequestedNumberOfIterations`<br>`TweenCoreSequencingTests.OnUpdateValue_FiresOncePerFrame`<br>`TweenCoreSequencingTests.Restart_ReplaysAChainCorrectly`<br>`TweenCoreSequencingTests.Stop_StopsEveryProperty_NotOnlyTheFirst`<br>`TweenCoreSequencingTests.ZeroDurationFirstLink_DoesNotCutTheChainShort`<br>`TweenCoreSequencingTests.ZeroDurationProperties_InParallel_DoNotThrowAndCompleteTheTween` | True |
| M37 | `TweenCore.cs` | tween Pause does nothing | 3 | `PropertyLifecycleTests.Pause_IsReportedByIsPaused`<br>`PropertyLifecycleTests.Pause_StopsTheValueAdvancing`<br>`PropertyLifecycleTests.Resume_ContinuesFromWhereItPaused` | True |
| M38 | `TweenCore.cs` | Update ignores the paused flag | 2 | `PropertyLifecycleTests.Pause_StopsTheValueAdvancing`<br>`PropertyLifecycleTests.Resume_ContinuesFromWhereItPaused` | True |
| M39 | `TweenCore.cs` | tween Resume does not resume | 2 | `PropertyLifecycleTests.Pause_IsReportedByIsPaused`<br>`PropertyLifecycleTests.Resume_ContinuesFromWhereItPaused` | True |
| M40 | `TweenCore.cs` | elapsed time accumulates halved | 2 | `PropertyLifecycleTests.ElapsedTime_AccumulatesTheDeltasPassedIn`<br>`PropertyLifecycleTests.Play_ATweenAlreadyPlaying_DoesNotRestartIt` | True |
| M41 | `TweenCore.cs` | Stop does not reset elapsed time | 1 | `PropertyLifecycleTests.ElapsedTime_ResetsWhenTheTweenStops` | True |
| M42 | `TweenCoreProperty.cs` | property OnStart never raised | 1 | `PropertyLifecycleTests.PropertyOnStart_FiresOnce_WhenThePropertyStarts` | True |
| M43 | `TweenCoreProperty.cs` | property OnUpdate never raised | 1 | `PropertyLifecycleTests.PropertyOnUpdate_FiresOncePerFrameWhileRunning` | True |
| M44 | `TweenCoreProperty.cs` | property OnFinish never raised | 7 | `PropertyLifecycleTests.PropertyOnFinish_FiresOnce_WhenThePropertyCompletes`<br>`PropertyLifecycleTests.SetDelay_PushesCompletionOutByTheDelay`<br>`PropertyLifecycleTests.TweenOnFinish_FiresWhenTheTweenEnds`<br>`TweenCoreSequencingTests.Loop_RunsTheRequestedNumberOfIterations`<br>`TweenCoreSequencingTests.Restart_ReplaysAChainCorrectly`<br>`TweenCoreSequencingTests.ZeroDurationFirstLink_DoesNotCutTheChainShort`<br>`TweenCoreSequencingTests.ZeroDurationProperties_InParallel_DoNotThrowAndCompleteTheTween` | True |
| M45 | `TweenCore.cs` | tween OnStart never raised | 1 | `PropertyLifecycleTests.TweenOnStart_FiresOnPlay` | True |
| M46 | `TweenCore.cs` | tween OnUpdate never raised | 1 | `PropertyLifecycleTests.TweenOnUpdate_FiresOncePerUpdateCall` | True |
| M47 | `TweenCore.cs` | tween OnFinish never raised | 1 | `PropertyLifecycleTests.TweenOnFinish_FiresWhenTheTweenEnds` | True |
| M48 | `TweenCoreProperty.cs` | Stop never writes the final value | 4 | `PropertyLifecycleTests.AZeroDurationProperty_HoldsItsFinalValueImmediately`<br>`PropertyLifecycleTests.Stop_WithSetToFinalValue_LandsOnTheFinalValue`<br>`TweenCoreSequencingTests.FinalValueIsExact_EvenWhenACustomCurveDoesNotEndAtOne`<br>`TweenCoreSequencingTests.Stop_StopsEveryProperty_NotOnlyTheFirst` | True |
| M49 | `TweenCoreProperty.cs` | Stop always writes the final value | 2 | `PropertyLifecycleTests.Kill_IsStopWithoutWritingAValue`<br>`PropertyLifecycleTests.Stop_WithoutSetToFinalValue_LeavesTheValueWhereItWas` | True |
| M50 | `TweenCore.cs` | Kill writes the final value | 1 | `PropertyLifecycleTests.Kill_IsStopWithoutWritingAValue` | True |
| M51 | `TweenCoreProperty.cs` | SetToFinalVals writes the start | 17 | `CurveShapeTests.CustomTypeFunction_StillLandsExactlyOnTheFinalValue`<br>`PropertyLifecycleTests.AZeroDurationProperty_HoldsItsFinalValueImmediately`<br>`PropertyLifecycleTests.SetDelay_PushesCompletionOutByTheDelay`<br>`PropertyLifecycleTests.SetToFinalVals_LandsOnTheEndValueWithoutRunning`<br>`PropertyLifecycleTests.Stop_WithSetToFinalValue_LandsOnTheFinalValue`<br>`PropertyValueSourceTests.Additive_ComposesAQuaternionOffset`<br>`PropertyValueSourceTests.Additive_OffsetsAColor`<br>`PropertyValueSourceTests.Additive_OffsetsAVector3`<br>`PropertyValueSourceTests.From_ThenAdditive_OffsetsFromTheGivenStartValue`<br>`PropertyValueSourceTests.SetIsAdditive_False_RestoresAbsoluteBehaviour`<br>`PropertyValueSourceTests.SetIsAdditive_TreatsTheFinalValueAsAnOffset`<br>`PropertyValueSourceTests.WithoutSetIsAdditive_TheFinalValueIsAbsolute`<br>`TweenCoreSequencingTests.Complete_LandsEveryProperty_IncludingPendingChainLinks`<br>`TweenCoreSequencingTests.EveryTypeAndEase_LandsOnTheFinalValue`<br>`TweenCoreSequencingTests.FinalValueIsExact_EvenWhenACustomCurveDoesNotEndAtOne`<br>`TweenCoreSequencingTests.Stop_StopsEveryProperty_NotOnlyTheFirst`<br>`TweenCoreSequencingTests.ZeroDurationFirstLink_DoesNotCutTheChainShort` | True |
| M52 | `TweenCoreProperty.cs` | zero duration does not complete | 2 | `PropertyLifecycleTests.AZeroDurationProperty_HoldsItsFinalValueImmediately`<br>`TweenCoreSequencingTests.ZeroDurationFirstLink_DoesNotCutTheChainShort` | True |
| M53 | `TweenCore.cs` | expected property count is zero | 99 | `CurveShapeTests.AnimationCurveAsEase_ReplacesTheDirection`<br>`CurveShapeTests.AnimationCurveAsType_ReplacesTheShape`<br>`CurveShapeTests.CustomEaseFunction_CanIgnoreTheTypeEntirely`<br>`CurveShapeTests.CustomTypeFunction_ReplacesTheShape`<br>`CurveShapeTests.EveryBuiltInType_IsAnInShape`<br>`CurveShapeTests.EveryBuiltInTypeExceptLinear_IsStrictlyBackLoaded`<br>`CurveShapeTests.InOut_IsTheInShapeCompressedIntoTheFirstHalf`<br>`CurveShapeTests.InOut_PassesThroughTheMidpoint`<br>`CurveShapeTests.Linear_IsTheIdentity`<br>`CurveShapeTests.Out_IsTheMirrorOfIn`<br>`CurveShapeTests.OutIn_IsTheOutShapeCompressedIntoTheFirstHalf`<br>`CurveShapeTests.OutIn_PassesThroughTheMidpoint`<br>`CurveShapeTests.SetEase_WithEnumAndCurve_UsesTheCurve`<br>`CurveShapeTests.SetEase_WithEnumAndFunction_UsesTheFunction`<br>`CurveShapeTests.SetType_WithEnumAndCurve_UsesTheCurve`<br>`CurveShapeTests.SetType_WithEnumAndFunction_UsesTheFunction`<br>`PropertyLifecycleTests.ElapsedTime_AccumulatesTheDeltasPassedIn`<br>`PropertyLifecycleTests.NumProperties_CountsWhatWillRun`<br>`PropertyLifecycleTests.Play_ATweenAlreadyPlaying_DoesNotRestartIt`<br>`PropertyLifecycleTests.PropertyOnFinish_FiresOnce_WhenThePropertyCompletes`<br>`PropertyLifecycleTests.PropertyOnUpdate_FiresOncePerFrameWhileRunning`<br>`PropertyLifecycleTests.PropertyPause_StopsThatPropertyOnly`<br>`PropertyLifecycleTests.Resume_ContinuesFromWhereItPaused`<br>`PropertyLifecycleTests.SetDelay_PushesCompletionOutByTheDelay`<br>`PropertyLifecycleTests.SetDelay_ShiftsTheAnimationRatherThanCompressingIt`<br>`PropertyLifecycleTests.TweenOnUpdate_FiresOncePerUpdateCall`<br>`PropertyValueSourceTests.Additive_HalfwayIsHalfTheOffset`<br>`PropertyValueSourceTests.From_ChangesWhereTheAnimationBegins`<br>`TweenCoreSequencingTests.Bounce_IsAnInShape_LikeEveryOtherTypeFunction`<br>`TweenCoreSequencingTests.ZeroDurationFirstLink_DoesNotCutTheChainShort` | True |
| M54 | `TweenCoreProperty.cs` | From does not store the value | 3 | `PropertyValueSourceTests.From_ChangesWhereTheAnimationBegins`<br>`PropertyValueSourceTests.From_OverridesTheStartValue`<br>`PropertyValueSourceTests.From_ThenAdditive_OffsetsFromTheGivenStartValue` | True |
| M55 | `TweenCoreProperty.cs` | SetIsAdditive skips FromCurrent | 7 | `PropertyValueSourceTests.Additive_ComposesAQuaternionOffset`<br>`PropertyValueSourceTests.Additive_HalfwayIsHalfTheOffset`<br>`PropertyValueSourceTests.Additive_OffsetsAColor`<br>`PropertyValueSourceTests.Additive_OffsetsAVector3`<br>`PropertyValueSourceTests.From_ThenAdditive_OffsetsFromTheGivenStartValue`<br>`PropertyValueSourceTests.SetIsAdditive_AlsoTurnsFromCurrentOn`<br>`PropertyValueSourceTests.SetIsAdditive_TreatsTheFinalValueAsAnOffset` | True |
| M56 | `TweenCoreProperty.cs` | SetIsAdditive does not set the flag | 7 | `PropertyValueSourceTests.Additive_ComposesAQuaternionOffset`<br>`PropertyValueSourceTests.Additive_HalfwayIsHalfTheOffset`<br>`PropertyValueSourceTests.Additive_OffsetsAColor`<br>`PropertyValueSourceTests.Additive_OffsetsAVector3`<br>`PropertyValueSourceTests.From_ThenAdditive_OffsetsFromTheGivenStartValue`<br>`PropertyValueSourceTests.SetIsAdditive_IsExposedAsIsIncreasingValue`<br>`PropertyValueSourceTests.SetIsAdditive_TreatsTheFinalValueAsAnOffset` | True |
| M57 | `TweenCoreProperty.cs` | additive end value is not summed | 6 | `PropertyValueSourceTests.Additive_ComposesAQuaternionOffset`<br>`PropertyValueSourceTests.Additive_HalfwayIsHalfTheOffset`<br>`PropertyValueSourceTests.Additive_OffsetsAColor`<br>`PropertyValueSourceTests.Additive_OffsetsAVector3`<br>`PropertyValueSourceTests.From_ThenAdditive_OffsetsFromTheGivenStartValue`<br>`PropertyValueSourceTests.SetIsAdditive_TreatsTheFinalValueAsAnOffset` | True |
| M58 | `TweenCoreProperty.cs` | property Pause does nothing | 1 | `PropertyLifecycleTests.PropertyPause_StopsThatPropertyOnly` | True |
| M59 | `TweenCorePropertyBase.cs` | Duration is not exposed | 1 | `PropertyLifecycleTests.DurationAndTypeAndEase_AreExposed` | True |
| M60 | `TweenCoreProperty.cs` | GetCurrentValue returns default | 1 | `PropertyLifecycleTests.GetCurrentValue_MatchesTheCurrentValueProperty` | True |

## Slice 3 - sequencing, chains and loops

EditMode. Suite: 213 tests.

| # | File | Mutation | Suite failed | Tests that went red | Restored |
|---|---|---|---|---|---|
| M61 | `TweenCore.cs` | tweens default to chain not parallel | 3 | `ChainAndParallelTests.ATweenIsParallelByDefault`<br>`PropertyLifecycleTests.PropertyPause_StopsThatPropertyOnly`<br>`TweenCoreSequencingTests.Stop_StopsEveryProperty_NotOnlyTheFirst` | True |
| M62 | `TweenCore.cs` | SetParallel ignores its argument | 1 | `ChainAndParallelTests.SetParallel_AndSetChain_AreOpposites` | True |
| M63 | `TweenCore.cs` | SetChain does not invert | 1 | `ChainAndParallelTests.SetParallel_AndSetChain_AreOpposites` | True |
| M64 | `TweenCore.cs` | Chain() leaves the tween parallel | 7 | `ChainAndParallelTests.Chain_LeavesLaterLinksUntouchedWhileTheFirstRuns`<br>`ChainAndParallelTests.Chain_MakesTheTweenNotParallel`<br>`ChainAndParallelTests.Chain_StartsTheNextLinkWhenTheCurrentOneFinishes`<br>`ChainAndParallelTests.Chain_TakesTheSumOfItsLinksToFinish`<br>`ChainAndParallelTests.Stop_LeavesAChainLinkThatNeverRanUntouched`<br>`TweenCoreSequencingTests.Restart_ReplaysAChainCorrectly`<br>`TweenCoreSequencingTests.ZeroDurationFirstLink_DoesNotCutTheChainShort` | True |
| M65 | `TweenCore.cs` | Parallel() leaves the tween chained | 3 | `ChainAndParallelTests.Parallel_FinishesWhenTheLongestPropertyDoes`<br>`ChainAndParallelTests.Parallel_MakesTheTweenParallel`<br>`ChainAndParallelTests.Parallel_StartsEveryPropertyAtTheSameTime` | True |
| M66 | `TweenCore.cs` | chain links are never wired up | 5 | `ChainAndParallelTests.Chain_StartsTheNextLinkWhenTheCurrentOneFinishes`<br>`ChainAndParallelTests.Chain_TakesTheSumOfItsLinksToFinish`<br>`LoopTests.ALoopedChain_RestartsFromItsFirstLink`<br>`TweenCoreSequencingTests.Restart_ReplaysAChainCorrectly`<br>`TweenCoreSequencingTests.ZeroDurationFirstLink_DoesNotCutTheChainShort` | True |
| M67 | `TweenCore.cs` | parallel starts only the first | 5 | `ChainAndParallelTests.Parallel_FinishesWhenTheLongestPropertyDoes`<br>`ChainAndParallelTests.Parallel_StartsEveryPropertyAtTheSameTime`<br>`PropertyLifecycleTests.PropertyPause_StopsThatPropertyOnly`<br>`TweenCoreSequencingTests.Stop_StopsEveryProperty_NotOnlyTheFirst`<br>`TweenCoreSequencingTests.ZeroDurationProperties_InParallel_DoNotThrowAndCompleteTheTween` | True |
| M68 | `TweenCore.cs` | SetLoop discards the iteration count | 6 | `LoopTests.AFiniteLoop_StopsAfterTheRequestedIterations`<br>`LoopTests.OnFinish_FiresOnceWhenAFiniteLoopEnds`<br>`LoopTests.SetLoop_IsExposedAsIsLoopAndNumIteration`<br>`LoopTests.ZeroIterations_FinishesImmediatelyWithoutPlaying`<br>`TweenCoreSequencingTests.Loop_RunsTheRequestedNumberOfIterations`<br>`TweenCoreSequencingTests.ZeroIterations_RunNothingAndWriteNothing` | True |
| M69 | `TweenCore.cs` | SetLoop never enables looping | 10 | `LoopTests.AFiniteLoop_StopsAfterTheRequestedIterations`<br>`LoopTests.ALoopedChain_RestartsFromItsFirstLink`<br>`LoopTests.AnInfiniteLoop_KeepsPlayingWellPastItsDuration`<br>`LoopTests.CurrentIteration_AdvancesOncePerCompletedCycle`<br>`LoopTests.EachIteration_RestartsThePropertyFromItsStartValue`<br>`LoopTests.OnLoopFinish_FiresOncePerCompletedCycleOfAnInfiniteLoop`<br>`LoopTests.SetLoop_IsExposedAsIsLoopAndNumIteration`<br>`LoopTests.ZeroIterations_FinishesImmediatelyWithoutPlaying`<br>`TweenCoreSequencingTests.Loop_RunsTheRequestedNumberOfIterations`<br>`TweenCoreSequencingTests.ZeroIterations_RunNothingAndWriteNothing` | True |
| M70 | `TweenCore.cs` | infinite loops are not infinite | 5 | `LoopTests.ALoopedChain_RestartsFromItsFirstLink`<br>`LoopTests.AnInfiniteLoop_KeepsPlayingWellPastItsDuration`<br>`LoopTests.CurrentIteration_AdvancesOncePerCompletedCycle`<br>`LoopTests.EachIteration_RestartsThePropertyFromItsStartValue`<br>`LoopTests.OnLoopFinish_FiresOncePerCompletedCycleOfAnInfiniteLoop` | True |
| M71 | `TweenCore.cs` | iteration counter never advances | 4 | `LoopTests.AFiniteLoop_StopsAfterTheRequestedIterations`<br>`LoopTests.CurrentIteration_AdvancesOncePerCompletedCycle`<br>`LoopTests.OnFinish_FiresOnceWhenAFiniteLoopEnds`<br>`TweenCoreSequencingTests.Loop_RunsTheRequestedNumberOfIterations` | True |
| M72 | `TweenCore.cs` | OnLoopFinish never raised | 2 | `LoopTests.OnLoopFinish_FiresOncePerCompletedCycleOfAnInfiniteLoop`<br>`TweenCoreSequencingTests.Loop_RunsTheRequestedNumberOfIterations` | True |
| M73 | `TweenCore.cs` | loop does not restart properties | 6 | `LoopTests.AFiniteLoop_StopsAfterTheRequestedIterations`<br>`LoopTests.CurrentIteration_AdvancesOncePerCompletedCycle`<br>`LoopTests.EachIteration_RestartsThePropertyFromItsStartValue`<br>`LoopTests.OnFinish_FiresOnceWhenAFiniteLoopEnds`<br>`LoopTests.OnLoopFinish_FiresOncePerCompletedCycleOfAnInfiniteLoop`<br>`TweenCoreSequencingTests.Loop_RunsTheRequestedNumberOfIterations` | True |
| M74 | `TweenCore.cs` | loop does not reset the done count | 2 | `LoopTests.AMultiPropertyLoop_CountsOneIterationPerFullCycle`<br>`LoopTests.ANewIteration_DoesNotCompleteUntilItsPropertiesDo` | True |
| M75 | `TweenCore.cs` | looped chain does not restart | 1 | `LoopTests.ALoopedChain_RestartsFromItsFirstLink` | True |
| M76 | `TweenCore.cs` | zero iterations still runs | 2 | `LoopTests.ZeroIterations_FinishesImmediatelyWithoutPlaying`<br>`TweenCoreSequencingTests.ZeroIterations_RunNothingAndWriteNothing` | True |
| M77 | `TweenCore.cs` | SetUseUnscaledTime ignored | 1 | `ChainAndParallelTests.SetUseUnscaledTime_IsExposedAsUseUnscaledTime` | True |
| M78 | `TweenCore.cs` | SurviveOnUnload does not survive | 2 | `ChainAndParallelTests.SurviveOnUnload_IsExposedAndReversible`<br>`ChainAndParallelTests.TheObsoleteUnloadAliases_ForwardToTheNewNames` | True |
| M79 | `TweenCore.cs` | KillOnUnload does not clear the flag | 2 | `ChainAndParallelTests.SurviveOnUnload_IsExposedAndReversible`<br>`ChainAndParallelTests.TheObsoleteUnloadAliases_ForwardToTheNewNames` | True |
| M80 | `TweenCore.cs` | SetSurviveOnUnload ignored | 1 | `ChainAndParallelTests.SurviveOnUnload_IsExposedAndReversible` | True |
| M81 | `TweenCore.cs` | DontDestroyWhenFinish still destroys | 3 | `ChainAndParallelTests.DestroyWhenFinish_IsOnByDefaultAndCanBeTurnedOff`<br>`ChainAndParallelTests.DontDestroyWhenFinish_KeepsThePropertiesAfterTheTweenEnds`<br>`TweenCoreSequencingTests.Restart_ReplaysAChainCorrectly` | True |
| M82 | `TweenCore.cs` | SetDestroyWhenFinish ignored | 1 | `ChainAndParallelTests.DestroyWhenFinish_IsOnByDefaultAndCanBeTurnedOff` | True |
| M83 | `TweenCore.cs` | finished properties always dropped | 9 | `ChainAndParallelTests.DontDestroyWhenFinish_KeepsThePropertiesAfterTheTweenEnds`<br>`LoopTests.AFiniteLoop_StopsAfterTheRequestedIterations`<br>`LoopTests.ALoopedChain_RestartsFromItsFirstLink`<br>`LoopTests.CurrentIteration_AdvancesOncePerCompletedCycle`<br>`LoopTests.EachIteration_RestartsThePropertyFromItsStartValue`<br>`LoopTests.OnFinish_FiresOnceWhenAFiniteLoopEnds`<br>`LoopTests.OnLoopFinish_FiresOncePerCompletedCycleOfAnInfiniteLoop`<br>`TweenCoreSequencingTests.Loop_RunsTheRequestedNumberOfIterations`<br>`TweenCoreSequencingTests.Restart_ReplaysAChainCorrectly` | True |
| M84 | `TweenCore.cs` | Stop guard inverted | 17 | `ChainAndParallelTests.Chain_TakesTheSumOfItsLinksToFinish`<br>`ChainAndParallelTests.Parallel_FinishesWhenTheLongestPropertyDoes`<br>`ChainAndParallelTests.Stop_OnATweenThatNeverPlayed_DoesNothing`<br>`LoopTests.AFiniteLoop_StopsAfterTheRequestedIterations`<br>`LoopTests.OnFinish_FiresOnceWhenAFiniteLoopEnds`<br>`LoopTests.ZeroIterations_FinishesImmediatelyWithoutPlaying`<br>`PropertyLifecycleTests.ElapsedTime_ResetsWhenTheTweenStops`<br>`PropertyLifecycleTests.Kill_IsStopWithoutWritingAValue`<br>`PropertyLifecycleTests.SetDelay_PushesCompletionOutByTheDelay`<br>`PropertyLifecycleTests.Stop_ClearsPlayingAndMarksFinished`<br>`PropertyLifecycleTests.Stop_WithSetToFinalValue_LandsOnTheFinalValue`<br>`PropertyLifecycleTests.TweenOnFinish_FiresWhenTheTweenEnds`<br>`TweenCoreSequencingTests.Loop_RunsTheRequestedNumberOfIterations`<br>`TweenCoreSequencingTests.Restart_ReplaysAChainCorrectly`<br>`TweenCoreSequencingTests.Stop_StopsEveryProperty_NotOnlyTheFirst`<br>`TweenCoreSequencingTests.ZeroDurationFirstLink_DoesNotCutTheChainShort`<br>`TweenCoreSequencingTests.ZeroDurationProperties_InParallel_DoNotThrowAndCompleteTheTween` | True |
| M85 | `TweenCore.cs` | finished properties never counted | 15 | `ChainAndParallelTests.Chain_TakesTheSumOfItsLinksToFinish`<br>`ChainAndParallelTests.NumPropertiesFinished_CountsCompletedProperties`<br>`ChainAndParallelTests.Parallel_FinishesWhenTheLongestPropertyDoes`<br>`LoopTests.AFiniteLoop_StopsAfterTheRequestedIterations`<br>`LoopTests.ALoopedChain_RestartsFromItsFirstLink`<br>`LoopTests.CurrentIteration_AdvancesOncePerCompletedCycle`<br>`LoopTests.EachIteration_RestartsThePropertyFromItsStartValue`<br>`LoopTests.OnFinish_FiresOnceWhenAFiniteLoopEnds`<br>`LoopTests.OnLoopFinish_FiresOncePerCompletedCycleOfAnInfiniteLoop`<br>`PropertyLifecycleTests.SetDelay_PushesCompletionOutByTheDelay`<br>`PropertyLifecycleTests.TweenOnFinish_FiresWhenTheTweenEnds`<br>`TweenCoreSequencingTests.Loop_RunsTheRequestedNumberOfIterations`<br>`TweenCoreSequencingTests.Restart_ReplaysAChainCorrectly`<br>`TweenCoreSequencingTests.ZeroDurationFirstLink_DoesNotCutTheChainShort`<br>`TweenCoreSequencingTests.ZeroDurationProperties_InParallel_DoNotThrowAndCompleteTheTween` | True |
| M86 | `TweenCoreProperty.cs` | function properties never written | 1 | `ChainAndParallelTests.NewProperty_WithAFunction_WritesThroughThatFunction` | True |

## Slice 4 - reflection binding

EditMode. Suite: 232 tests.

| # | File | Mutation | Suite failed | Tests that went red | Restored |
|---|---|---|---|---|---|
| M87 | `TweenCoreProperty.cs` | reflection never writes to the target | 12 | `ReflectionBindingTests.Additive_OnAReflectionTween_OffsetsFromTheTargetsValue`<br>`ReflectionBindingTests.AReflectionTween_WritesAPublicField`<br>`ReflectionBindingTests.AReflectionTween_WritesAQuaternionRotation`<br>`ReflectionBindingTests.AReflectionTween_WritesAWritableProperty`<br>`ReflectionBindingTests.AReflectionTween_WritesEulerAngles`<br>`ReflectionBindingTests.AReflectionTween_WritesGlobalPosition`<br>`ReflectionBindingTests.AReflectionTween_WritesToTheTargetsProperty`<br>`ReflectionBindingTests.FromCurrent_ReadsTheTargetWhenTheTweenPlays`<br>`ReflectionBindingTests.TheStartValueIsReadAtPlay_NotAtConstruction`<br>`ReflectionBindingTests.TheTargetConstants_ResolveTheSameMembersAsTheirStrings`<br>`ReflectionBindingTests.WithAStartValue_TheTargetsCurrentValueIsIgnored`<br>`ReflectionBindingTests.WithoutAStartValue_TheTargetsCurrentValueIsTheStart` | True |
| M88 | `TweenCoreProperty.cs` | the bound setter is never called | 6 | `ReflectionBindingTests.AReflectionTween_WritesAQuaternionRotation`<br>`ReflectionBindingTests.AReflectionTween_WritesAWritableProperty`<br>`ReflectionBindingTests.AReflectionTween_WritesEulerAngles`<br>`ReflectionBindingTests.AReflectionTween_WritesGlobalPosition`<br>`ReflectionBindingTests.AReflectionTween_WritesToTheTargetsProperty`<br>`ReflectionBindingTests.TheTargetConstants_ResolveTheSameMembersAsTheirStrings` | True |
| M89 | `TweenCoreProperty.cs` | the field fallback never writes | 6 | `ReflectionBindingTests.Additive_OnAReflectionTween_OffsetsFromTheTargetsValue`<br>`ReflectionBindingTests.AReflectionTween_WritesAPublicField`<br>`ReflectionBindingTests.FromCurrent_ReadsTheTargetWhenTheTweenPlays`<br>`ReflectionBindingTests.TheStartValueIsReadAtPlay_NotAtConstruction`<br>`ReflectionBindingTests.WithAStartValue_TheTargetsCurrentValueIsIgnored`<br>`ReflectionBindingTests.WithoutAStartValue_TheTargetsCurrentValueIsTheStart` | True |
| M90 | `TweenCoreProperty.cs` | FromCurrent never reads the target | 4 | `ReflectionBindingTests.Additive_OnAReflectionTween_OffsetsFromTheTargetsValue`<br>`ReflectionBindingTests.FromCurrent_ReadsTheTargetWhenTheTweenPlays`<br>`ReflectionBindingTests.TheStartValueIsReadAtPlay_NotAtConstruction`<br>`ReflectionBindingTests.WithoutAStartValue_TheTargetsCurrentValueIsTheStart` | True |
| M91 | `TweenCoreProperty.cs` | reading the target returns default | 4 | `ReflectionBindingTests.Additive_OnAReflectionTween_OffsetsFromTheTargetsValue`<br>`ReflectionBindingTests.FromCurrent_ReadsTheTargetWhenTheTweenPlays`<br>`ReflectionBindingTests.TheStartValueIsReadAtPlay_NotAtConstruction`<br>`ReflectionBindingTests.WithoutAStartValue_TheTargetsCurrentValueIsTheStart` | True |
| M92 | `TweenCoreProperty.cs` | a missing member is not flagged | 2 | `ReflectionBindingTests.AnUnresolvableName_DoesNotThrowOnEveryFrame`<br>`ReflectionBindingTests.AnUnresolvableName_MarksThePropertyBrokenInsteadOfThrowing` | True |
| M93 | `TweenCoreProperty.cs` | a read only property is not flagged | 1 | `ReflectionBindingTests.AReadOnlyProperty_MarksThePropertyBrokenInsteadOfThrowing` | True |
| M94 | `TweenCoreProperty.cs` | a property type mismatch is not flagged | 1 | `ReflectionBindingTests.APropertyOfTheWrongType_MarksThePropertyBroken` | True |
| M95 | `TweenCoreProperty.cs` | destroyed target no longer detected | 1 | `ReflectionBindingTests.ADestroyedTarget_EndsTheTweenQuietlyInsteadOfThrowing` | True |
| M96 | `TweenCoreEnums.cs` | LOCAL_SCALE constant points elsewhere | 1 | `ReflectionBindingTests.TheTargetConstants_ResolveTheSameMembersAsTheirStrings` | True |
| M97 | `TweenCoreProperty.cs` | a field type mismatch is not flagged | 1 | `ReflectionBindingTests.AMemberOfTheWrongType_MarksThePropertyBroken` | True |

## Slice 5a - TweenCoreManager

EditMode. Suite: 246 tests.

| # | File | Mutation | Suite failed | Tests that went red | Restored |
|---|---|---|---|---|---|
| M98 | `TweenCoreManager.cs` | AddTween allows duplicates | 1 | `ManagerTests.AddTween_IgnoresATweenItAlreadyHas` | True |
| M99 | `TweenCoreManager.cs` | AddTween accepts null | 2 | `ManagerTests.AddTween_IgnoresATweenItAlreadyHas`<br>`ManagerTests.AddTween_IgnoresNull` | True |
| M100 | `TweenCoreManager.cs` | RemoveTween does nothing | 1 | `ManagerTests.RemoveTween_UnregistersTheTween` | True |
| M101 | `TweenCoreManager.cs` | PauseAll does not pause | 1 | `ManagerTests.PauseAll_StopsTheManager` | True |
| M102 | `TweenCoreManager.cs` | ResumeAll does not resume | 1 | `ManagerTests.ResumeAll_RestartsTheManager` | True |
| M103 | `TweenCoreManager.cs` | PauseAll also pauses each tween | 1 | `ManagerTests.PauseAll_PausesTheManagerNotTheIndividualTweens` | True |
| M104 | `TweenCoreManager.cs` | StopAll stops only the last tween | 1 | `ManagerTests.StopAll_StopsEveryRegisteredTween` | True |
| M105 | `TweenCoreManager.cs` | StopAll ignores its argument | 1 | `ManagerTests.StopAll_False_LeavesTheValuesWhereTheyWere` | True |
| M106 | `TweenCoreManager.cs` | NumTweens always reports zero | 3 | `ManagerTests.AddTween_IgnoresATweenItAlreadyHas`<br>`ManagerTests.AddTween_RegistersTheTween`<br>`ManagerTests.RemoveTween_OfATweenItNeverHad_DoesNothing` | True |
| M107 | `TweenCoreManager.cs` | a new manager starts paused | 1 | `ManagerTests.ANewManager_IsPlaying` | True |
| M108 | `TweenCoreManager.cs` | Instance spawns outside play mode | 1 | `ManagerTests.Instance_DoesNotSpawnAManagerOutsidePlayMode` | True |

## Slice 5b - TweenCoreComponent

**PlayMode.** Suite: 16 tests.

| # | File | Mutation | Suite failed | Tests that went red | Restored |
|---|---|---|---|---|---|
| M109 | `TweenCoreComponent.cs` | Awake creates no tween | 13 | `ComponentTests.AComponent_HasATweenAsSoonAsItExists`<br>`ComponentTests.AddProperty_WiresThePropertyIntoTheTweenOnStart`<br>`ComponentTests.ByDefault_TheTweenPlaysOnStart`<br>`ComponentTests.Complete_LandsThePropertiesOnTheirFinalValues`<br>`ComponentTests.DestroyingTheComponent_DoesNotThrow`<br>`ComponentTests.DestroyingTheComponent_StopsItsTweenWithoutWritingValues`<br>`ComponentTests.Pause_PausesTheUnderlyingTween`<br>`ComponentTests.Play_StartsTheUnderlyingTween`<br>`ComponentTests.Restart_ReplaysFromTheStart`<br>`ComponentTests.Resume_ResumesTheUnderlyingTween`<br>`ComponentTests.StopAndDontChangeValue_LeavesThePropertiesWhereTheyWere`<br>`ComponentTests.StopAndSetToFinalValue_LandsTheProperties`<br>`ComponentTests.TheManagerDrivesTheComponentsTweenAcrossFrames` | True |
| M110 | `TweenCoreComponent.cs` | Play does not forward | 5 | `ComponentTests.AddProperty_WiresThePropertyIntoTheTweenOnStart`<br>`ComponentTests.ByDefault_TheTweenPlaysOnStart`<br>`ComponentTests.Play_StartsTheUnderlyingTween`<br>`ComponentTests.StopAndSetToFinalValue_LandsTheProperties`<br>`ComponentTests.TheManagerDrivesTheComponentsTweenAcrossFrames` | True |
| M111 | `TweenCoreComponent.cs` | Pause does not forward | 1 | `ComponentTests.Pause_PausesTheUnderlyingTween` | True |
| M112 | `TweenCoreComponent.cs` | Resume does not forward | 1 | `ComponentTests.Resume_ResumesTheUnderlyingTween` | True |
| M113 | `TweenCoreComponent.cs` | Complete does not forward | 1 | `ComponentTests.Complete_LandsThePropertiesOnTheirFinalValues` | True |
| M114 | `TweenCoreComponent.cs` | Restart does not forward | 1 | `ComponentTests.Restart_ReplaysFromTheStart` | True |
| M115 | `TweenCoreComponent.cs` | StopAndSetToFinalValue writes nothing | 1 | `ComponentTests.StopAndSetToFinalValue_LandsTheProperties` | True |
| M116 | `TweenCoreComponent.cs` | StopAndDontChangeValue writes values | 1 | `ComponentTests.StopAndDontChangeValue_LeavesThePropertiesWhereTheyWere` | True |
| M117 | `TweenCoreComponent.cs` | AddProperty stores nothing | 3 | `ComponentTests.AddProperty_WiresThePropertyIntoTheTweenOnStart`<br>`ComponentTests.ByDefault_TheTweenPlaysOnStart`<br>`ComponentTests.TheManagerDrivesTheComponentsTweenAcrossFrames` | True |
| M118 | `TweenCoreComponent.cs` | OnDestroy writes final values | 1 | `ComponentTests.DestroyingTheComponent_StopsItsTweenWithoutWritingValues` | True |
| M119 | `TweenCoreComponent.cs` | play on start is ignored | 3 | `ComponentTests.AddProperty_WiresThePropertyIntoTheTweenOnStart`<br>`ComponentTests.ByDefault_TheTweenPlaysOnStart`<br>`ComponentTests.TheManagerDrivesTheComponentsTweenAcrossFrames` | True |

## Slice 6 - editor property picker filtering

EditMode. Suite: 260 tests.

| # | File | Mutation | Suite failed | Tests that went red | Restored |
|---|---|---|---|---|---|
| M120 | `TweenCorePropertyBaseEditor.cs` | property type filter dropped | 1 | `EditorPickerFilteringTests.MembersOfTheRequestedTypeOnly_AreOffered` | True |
| M121 | `TweenCorePropertyBaseEditor.cs` | CanWrite filter dropped | 1 | `EditorPickerFilteringTests.AReadOnlyProperty_IsNotOffered` | True |
| M122 | `TweenCorePropertyBaseEditor.cs` | indexer filter dropped | 1 | `EditorPickerFilteringTests.AnIndexer_IsNotOffered` | True |
| M123 | `TweenCorePropertyBaseEditor.cs` | obsolete property filter dropped | 1 | `EditorPickerFilteringTests.AnObsoleteProperty_IsNotOffered` | True |
| M124 | `TweenCorePropertyBaseEditor.cs` | field type filter dropped | 2 | `EditorPickerFilteringTests.AskingForVector3_OffersTheVector3Members`<br>`EditorPickerFilteringTests.MembersOfTheRequestedTypeOnly_AreOffered` | True |
| M125 | `TweenCorePropertyBaseEditor.cs` | readonly field guard dropped | 1 | `EditorPickerFilteringTests.AReadonlyField_IsNotOffered` | True |
| M126 | `TweenCorePropertyBaseEditor.cs` | const field guard dropped | 0 | **none - see the M126 note** | True |
| M127 | `TweenCorePropertyBaseEditor.cs` | obsolete field filter dropped | 1 | `EditorPickerFilteringTests.AnObsoleteField_IsNotOffered` | True |
| M128 | `TweenCorePropertyBaseEditor.cs` | the result is not sorted | 1 | `EditorPickerFilteringTests.TheOfferedNames_AreSorted` | True |
| M129 | `TweenCorePropertyBaseEditor.cs` | null guard dropped | 1 | `EditorPickerFilteringTests.ANullTarget_OffersNothing` | True |

## Slice 7 - the scene unload hook

**PlayMode.** Suite: 22 tests.

| # | File | Mutation | Suite failed | Tests that went red | Restored |
|---|---|---|---|---|---|
| M130 | `TweenCoreManager.cs` | the unload hook is never subscribed | 3 | `SceneUnloadTests.AStoppedTween_IsUnregisteredFromTheManager`<br>`SceneUnloadTests.UnloadingAScene_DoesNotWriteThroughReflectionToADestroyedTarget`<br>`SceneUnloadTests.UnloadingAScene_StopsATweenThatDoesNotSurvive` | True |
| M131 | `TweenCoreManager.cs` | unload ignores SurviveOnUnload | 1 | `SceneUnloadTests.UnloadingAScene_LeavesASurvivingTweenPlaying` | True |
| M132 | `TweenCoreManager.cs` | unload writes final values | 2 | `SceneUnloadTests.UnloadingAScene_DoesNotWriteFinalValues`<br>`SceneUnloadTests.UnloadingAScene_DoesNotWriteThroughReflectionToADestroyedTarget` | True |
| M133 | `TweenCore.cs` | DestroyTween does not unregister | 1 | `SceneUnloadTests.AStoppedTween_IsUnregisteredFromTheManager` | True |

## The 1.0 asset upgrader

EditMode. Suite: 270 tests. New code, written test-first.

| # | File | Mutation | Suite failed | Tests that went red | Restored |
|---|---|---|---|---|---|
| M134 | `TweenCoreAssetUpgrader.cs` | the class-name guard is neutered | 1 | `AssetUpgraderTests.AnUnrelatedManagedReference_IsNotTouched` | True |
| M135 | `TweenCoreAssetUpgrader.cs` | the namespace guard is dropped | 1 | `AssetUpgraderTests.AnIdentifierAlreadyInANamespace_IsNotTouched` | True |
| M136 | `TweenCoreAssetUpgrader.cs` | the assembly guard is dropped | 1 | `AssetUpgraderTests.AnIdentifierInADifferentAssembly_IsNotTouched` | True |
| M137 | `TweenCoreAssetUpgrader.cs` | rewrites to the wrong assembly | 3 | `AssetUpgraderTests.AV10Identifier_IsRewrittenToTheV11Form`<br>`AssetUpgraderTests.EveryValueTypeIsHandled_NotJustVector3`<br>`AssetUpgraderTests.TheShippedSampleScene_HasItsPropertiesRestored` | True |
| M138 | `TweenCoreAssetUpgrader.cs` | rewrites to the wrong namespace | 3 | `AssetUpgraderTests.AV10Identifier_IsRewrittenToTheV11Form`<br>`AssetUpgraderTests.EveryValueTypeIsHandled_NotJustVector3`<br>`AssetUpgraderTests.TheShippedSampleScene_HasItsPropertiesRestored` | True |
| M139 | `TweenCoreAssetUpgrader.cs` | the counter never increments | 5 | `AssetUpgraderTests.AV10Identifier_IsRewrittenToTheV11Form`<br>`AssetUpgraderTests.EveryIdentifierInTheFile_IsRewritten`<br>`AssetUpgraderTests.EveryValueTypeIsHandled_NotJustVector3`<br>`AssetUpgraderTests.TheRestOfTheFile_IsUntouched`<br>`AssetUpgraderTests.TheShippedSampleScene_HasItsPropertiesRestored` | True |
| M140 | `TweenCoreAssetUpgrader.cs` | the rewrite is computed then discarded | 3 | `AssetUpgraderTests.AV10Identifier_IsRewrittenToTheV11Form`<br>`AssetUpgraderTests.EveryValueTypeIsHandled_NotJustVector3`<br>`AssetUpgraderTests.TheShippedSampleScene_HasItsPropertiesRestored` | True |

## After removing the dead const guard

EditMode. Suite: 270 tests.

| # | File | Mutation | Suite failed | Tests that went red | Restored |
|---|---|---|---|---|---|
| M141 | `TweenCorePropertyBaseEditor.cs` | picker enumerates static members too | 1 | `EditorPickerFilteringTests.AConstField_IsNotOffered` | True |

## Totals

| | |
|---|---|
| Mutations applied | 140 |
| Produced the expected failures | 139 |
| Left the suite green | 1 (M126, explained below) |
| Restored byte-identical | 140 |

## The mutations that found gaps

These are the reason the step is worth its cost. In each case the tests were
written correctly against the documentation, passed correctly, and were still
unable to detect a real regression.

**M74 - the loop completion counter.** Removing the reset of the finished-property
count on loop restart left the suite entirely green. Every loop test used a single
property stepped by exactly its duration, which looks identical whether or not the
counter resets. Two tests using fractional steps closed it. *Lesson: stepping a
tween by exactly its duration is a blind spot.*

**M94 - the property type guard.** The resolver type checks properties and fields
on two separate branches. A single test using a field covered only one, so
disabling the property branch broke nothing. A second test closed it, and M97
covers the field branch independently. *Lesson: one test per guard, not one per
behaviour, when the implementation branches.* Slice 6 was written to that rule
from the start, which is why its six filters have six separate tests.

**M136 - the upgrader's assembly guard.** The "different assembly" test also used
a non-empty namespace, so the *namespace* guard rejected it first and the
assembly guard was never reached: dropping it broke nothing. The test now uses an
empty namespace so only the assembly check can save it. This is the same shape as
M94, found for the third time - overlapping conditions in a test mean only the
first one is actually covered.

**M128 - the picker's sort.** The sort test used the fixture, whose two Vector3
members happen to come back from reflection already in ordinal order, making the
sort a no-op. Re-pointed at `Transform`, which returns a dozen members in
declaration order. *Lesson: a test of ordering needs input whose natural order is
wrong.*

## M126, and how removing dead code made a test provable

**M126** dropped the `IsLiteral` half of the picker's field guard and left the
suite green. `GetFields` is called with `BindingFlags.Instance |
BindingFlags.Public`, and `const` fields are implicitly *static*, so a `const` was
never returned to be filtered in the first place - the check was unreachable.
`AConstField_IsNotOffered` asserted a real and desirable behaviour and passed,
but two independent mechanisms protected it, so no single mutation could
falsify it. For a while it was the one assertion in the suite without evidence
behind it.

Deleting the unreachable half fixed that. The behaviour is now guarded by the
binding flags alone, so a single mutation *can* falsify the test - **M141** adds
`BindingFlags.Static` to the enumeration and the test goes red. Every assertion
in the suite now carries evidence.

Worth keeping as a general point: dead code is not merely inert. It made a
correct test unprovable, and hid that fact behind a passing run.

## Other notes worth keeping

- **M01** (breaking the `Out` mirror) took down 31 cases across four test methods.
- **M19** (Bounce no longer an In shape) also failed the pre-existing regression
  test `Bounce_IsAnInShape_LikeEveryOtherTypeFunction`: the new behavioural suite
  and the old bug-pinned suite agree with each other.
- **M48 / M49** are complementary - one stops `Stop` writing the final value, the
  other makes it always write. Both branches of `setToFinalValue` are pinned.
- **M53** (expected property count forced to zero) failed 99 cases and **M85**
  failed 15. A broad red there means the completion machinery, not whatever was
  being changed.
- **M88 / M89** separate the two write paths: properties go through the bound
  setter, fields through `FieldInfo.SetValue`.
- **M109** (Awake creates no tween) failed 13 of the 16 PlayMode tests. The
  component is almost entirely a forwarder.
- **M132 recreates the original B5 defect exactly** - flipping the unload hook
  from `Stop(false)` to `Stop(true)` so final values are written through
  reflection onto objects the scene has already destroyed. Two independent tests
  catch it, so the audit's headline fix is now guarded.
- **M07** aborted - a CRLF find string against LF sources. Re-run as **M13**. An
  aborted mutation is reported, never skipped silently.

## What this suite cannot verify

- **That reflection binds a delegate rather than calling `PropertyInfo.SetValue`
  each frame.** A performance claim; behaviour is identical either way.
- **Inspector layout geometry**, excluded by the spec on purpose.
- **IL2CPP / AOT behaviour**, including the reflection fallback for platforms
  where delegate creation fails. This suite runs on Mono in the editor.
- **Domain reload disabled.** `ResetStatics` carries a
  `[RuntimeInitializeOnLoadMethod]` for the case where Unity's domain reload is
  turned off. The test runner reloads the domain, so that path is never taken
  here.
- **`OnApplicationQuit`.** It latches `_canBeInstantiate` so no manager spawns
  during teardown. A batchmode test run does not quit the way a built player
  does, so this is not exercised.
