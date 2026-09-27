using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class AccelerometerShakeDetector : MonoBehaviour, IShakeDetector
{
    [Header("Calibración")]
    [Min(0.1f)] [SerializeField] private float calibrationDuration = 0.5f;
    [Min(0f)] [SerializeField] private float smoothing = 5f;

    [Header("Shake")]
    [Min(0.1f)] [SerializeField] private float shakeThreshold = 2f;
    [Min(0f)] [SerializeField] private float shakeCooldown = 0.8f;

    [Header("Pruebas")]
    [Tooltip("Permite simular un shake con Espacio dentro del Editor o Development Build.")]
    [SerializeField] private bool allowKeyboardSimulation = true;

    public event Action OnShakeDetected;
    public event Action<ShakeDetectorState> OnStateChanged;
    public ShakeDetectorState State { get; private set; } = ShakeDetectorState.Unavailable;
    public bool IsAvailable => Accelerometer.current != null || CanSimulate;

    private Vector3 smoothedAcceleration;
    private Vector3 previousAcceleration;
    private Vector3 baselineAcceleration;
    private bool initialized;
    private float nextShakeTime;
    private Coroutine calibrationRoutine;

    private bool CanSimulate
    {
        get
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return allowKeyboardSimulation && Keyboard.current != null;
#else
            return false;
#endif
        }
    }

    private void OnEnable()
    {
        if (Accelerometer.current != null)
            InputSystem.EnableDevice(Accelerometer.current);
        Calibrate();
    }

    private void OnDisable()
    {
        if (calibrationRoutine != null) StopCoroutine(calibrationRoutine);
        calibrationRoutine = null;
        initialized = false;
        if (Accelerometer.current != null)
            InputSystem.DisableDevice(Accelerometer.current);
    }

    private void Update()
    {
        if (CanSimulate && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TryRaiseShake();
            return;
        }

        if (Accelerometer.current == null || State != ShakeDetectorState.Ready) return;

        Vector3 rawAcceleration = Accelerometer.current.acceleration.ReadValue();
        if (!initialized)
        {
            smoothedAcceleration = rawAcceleration;
            previousAcceleration = rawAcceleration;
            initialized = true;
            return;
        }

        smoothedAcceleration = Vector3.Lerp(
            smoothedAcceleration, rawAcceleration, smoothing * Time.unscaledDeltaTime);

        float delta = (rawAcceleration - previousAcceleration).magnitude;
        float relativeMagnitude = (smoothedAcceleration - baselineAcceleration).magnitude;
        previousAcceleration = rawAcceleration;

        if (delta >= shakeThreshold && relativeMagnitude >= shakeThreshold * 0.35f)
            TryRaiseShake();
    }

    public void Calibrate()
    {
        if (!isActiveAndEnabled) return;
        if (calibrationRoutine != null) StopCoroutine(calibrationRoutine);
        calibrationRoutine = StartCoroutine(CalibrationRoutine());
    }

    private IEnumerator CalibrationRoutine()
    {
        SetState(IsAvailable ? ShakeDetectorState.Calibrating : ShakeDetectorState.Unavailable);
        if (!IsAvailable) yield break;

        float endTime = Time.unscaledTime + calibrationDuration;
        Vector3 sum = Vector3.zero;
        int samples = 0;
        while (Time.unscaledTime < endTime)
        {
            if (Accelerometer.current != null)
            {
                sum += Accelerometer.current.acceleration.ReadValue();
                samples++;
            }
            yield return null;
        }

        baselineAcceleration = samples > 0 ? sum / samples : Vector3.zero;
        smoothedAcceleration = baselineAcceleration;
        previousAcceleration = baselineAcceleration;
        initialized = Accelerometer.current != null;
        calibrationRoutine = null;
        SetState(ShakeDetectorState.Ready);
    }

    private void TryRaiseShake()
    {
        if (State != ShakeDetectorState.Ready || Time.unscaledTime < nextShakeTime) return;
        nextShakeTime = Time.unscaledTime + shakeCooldown;
        OnShakeDetected?.Invoke();
    }

    private void SetState(ShakeDetectorState state)
    {
        if (State == state) return;
        State = state;
        OnStateChanged?.Invoke(State);
    }
}
