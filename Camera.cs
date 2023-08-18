using Godot;
using System;

public partial class Camera
    : Camera2D
{
    float _amplitude = 0.0f;
    Timer _frequenceTimer;
    Timer _durationTimer;

    public void Shake(float duration, float frequence, float amplitude)
    {
        // simple camera shake;
        _durationTimer.WaitTime = duration;
        _frequenceTimer.WaitTime = 1.0f / frequence;

        _amplitude = amplitude;

        _durationTimer.Start();
        _frequenceTimer.Start();

        StartShake();
    }

    void StartShake()
    {
        Vector2 rng = new Vector2
        (
            (float)GD.RandRange(-_amplitude, _amplitude),
            (float)GD.RandRange(-_amplitude, _amplitude)
        );

        Tween shaker = GetTree()
            .CreateTween()
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        shaker.TweenProperty(this, "offset", rng, _frequenceTimer.WaitTime);
    }

    void ResetShake()
    {
        Tween shaker = GetTree()
            .CreateTween()
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        shaker.TweenProperty(this, "offset", Vector2.Zero, _frequenceTimer.WaitTime);
    }

    #region signals
    void OnFrequencyTimeout()
    {
        StartShake();
    }

    void OnDurationTimeout()
    {
        ResetShake();
        _frequenceTimer.Stop();
    }
    #endregion

    #region override
    public override void _Ready()
    {
        _durationTimer = GetNode<Timer>("Duration");
        _frequenceTimer = GetNode<Timer>("Frequence");
    }
    #endregion
}
