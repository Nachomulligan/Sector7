using System;

public interface IShakeDetector
{
    event Action OnShakeDetected;
    event Action<ShakeDetectorState> OnStateChanged;
    ShakeDetectorState State { get; }
    bool IsAvailable { get; }
    void Calibrate();
}

public enum ShakeDetectorState
{
    Unavailable,
    Calibrating,
    Ready
}
