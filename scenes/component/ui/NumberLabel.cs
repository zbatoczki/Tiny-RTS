using Godot;

namespace Game.Component.Ui;

[Tool]
public partial class NumberLabel : Label
{
    [Export] public float Duration { get; set; } = 0.6f;
    [Export] public Tween.TransitionType Transition { get; set; } = Tween.TransitionType.Expo;
    [Export] public Tween.EaseType Ease { get; set; } = Tween.EaseType.Out;

    private Tween tween;
    private float currentValue = 0f;

    public void AnimateTo(float target)
    {
        tween?.Kill();
        tween = CreateTween();
        tween.SetEase(Ease);
        tween.SetTrans(Transition);

        tween.TweenMethod(
            Callable.From((float value) =>
            {
                currentValue = value;
                Text = Mathf.RoundToInt(value).ToString();
            }),
            currentValue,
            target,
            Duration
        );
    }

    /// <summary>Set the value instantly with no animation.</summary>
    public void SetImmediate(float value)
    {
        tween?.Kill();
        currentValue = value;
        Text = Mathf.RoundToInt(value).ToString();
    }
}