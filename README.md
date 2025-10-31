# CustomUnityTween - Documentation

Author : Auguste Paccapelo

If you come here because nothing works, first make sure that the script TweenManager is attached to a GameObject in the game and is active.

----------

System used to make animations on objects, for exemple to move an object from a point to another.
You can choose to use Reflexion, a Function, or get the value and change it your self.

----------

Class : 
TweenManager : Need to be in the game, manage all tweens.
Tween : Contain and manage one or multiple TweenProperty.
TweenProperty<TweenValueType> : Calcul the current value of type TweenValueType, and if wanted set the given property or field.
TweenPropertyBase : Bastract class, parent of TweenProperty<TweenValueType> to manage multiple properties of different TweenValueTypes.

----------

Exemples of uses : 
- Reflexion : 
- - Exemple 1 :

Tween tween = Tween.CreateTween();
TweenProperty<Vector3> property = tween.NewProperty(transform, "position", Vector3.zero, new Vector3(5, 2, 0), 2f);
property.SetEase(TweenEase.Out);
property.SetType(TweenType.Bounce);

tween.Play();

- - Exemple 2 :

Tween tween = Tween.CreateTween();
tween.NewProperty(transform, TweenTarget.Transform.GLOBAL_POSITION, new Vector3(5, 2, 0), 2f)
    .SetEase(TweenEase.Out).SetType(TweenType.Bounce);

tween.Play();


- Function :

Tween tween = Tween.CreateTween();

tween.NewProperty(f => _target.transform.localScale = f, Vector3.zero, Vector3.one, _time * 2)
    .SetType(TweenType.Bounce).SetEase(TweenEase.Out);

tween.Play();


- Manual :

Tween tween = Tween.CreateTween();

TweenProperty<Vector3> property = tween.NewProperty(Vector3.zero, Vector3.one, _time * 2)
    .SetType(TweenType.Bounce).SetEase(TweenEase.Out);

tween.Play();

transform.localScale = property.CurrentValue;

----------

- - Tween :
- methods : 
.CreateTween() (static)
.Play()
.Pause()
.Resume()
.Stop()
.Update(float deltaTime)
.DestroyTweenProperty(TweenPropertyBase property)
.NewProperty(...) 4 overloads
.SetParallel(bool isParallel)
.SetChain(bool isChain)
.Parallel()
.Chain()

- Events :
OnStart
OnFinish

- - TweenPropertyBase
- methods :
.Update(float deltaTime)
.Start()
.Stop()
.AddNextProperty(TweenPropertyBase property)

- Events
OnStart
OnFinish

- - TweenProperty :
- Methods

.SetDelay(float tweenDelay)
.SetType(TweenType newType)
.SetCustomType(Func<float, float> customType)
.SetEase(TweenType newEase)
.SetCustomEase(Func<float, Func<float, float>, float> customEase)
.GetCurrentValue()
.From(TweenValueType value)
.Pause()
.Resume()

- Events :
OnUpdate<TweenValueType>

----------

Currently supported types : 
C# :
float, double, int, uint, long, ulong, decimal
Unity :
Vector2, Vector3, Vector3, Quaternion, Color, Color32
