using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Clase05AccelerometerManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text rawValuesText;
    [SerializeField] private TMP_Text smoothedValuesText;
    [SerializeField] private TMP_Text relativeValuesText;
    [SerializeField] private TMP_Text magnitudeText;
    [SerializeField] private TMP_Text shakeStatusText;
    [SerializeField] private TMP_Text shakeCountText;

    [Header("Suavizado")]
    [SerializeField] private float smoothing = 5f;

    [Header("Shake")]
    [SerializeField] private float shakeThreshold = 2f;
    [SerializeField] private float shakeCooldown = 0.8f;
    [SerializeField] private float feedbackDuration = 0.3f;

    [Header("Feedback")]
    [SerializeField] private Transform feedbackObject;
    [SerializeField] private Renderer feedbackRenderer;
    [SerializeField] private float movementSensitivity = 20f;

    private Vector3 rawAcceleration;
    private Vector3 smoothedAcceleration;
    private Vector3 previousAcceleration;

    private Vector3 baselineAcceleration;

    private bool initialized;
    private bool calibrated;

    private int shakeCount;

    private float nextShakeTime;
    private float feedbackEndTime;

    private void OnEnable()
    {
        if (Accelerometer.current != null)
        {
            InputSystem.EnableDevice(
                Accelerometer.current
            );
        }
    }

    private void OnDisable()
    {
        if (Accelerometer.current != null)
        {
            InputSystem.DisableDevice(
                Accelerometer.current
            );
        }
    }

    private void Update()
    {
        if (Accelerometer.current == null)
        {
            rawValuesText.text =
                "ACELERÓMETRO NO DISPONIBLE";

            return;
        }

        rawAcceleration =
            Accelerometer.current.acceleration.ReadValue();

        if (!initialized)
        {
            smoothedAcceleration = rawAcceleration;
            previousAcceleration = rawAcceleration;

            initialized = true;
        }

        UpdateSmoothedAcceleration();
        UpdateUI();
        UpdateMovement();
        CheckShake();
        UpdateShakeFeedback();

        previousAcceleration = rawAcceleration;
    }

    private void UpdateSmoothedAcceleration()
    {
        smoothedAcceleration =
            Vector3.Lerp(
                smoothedAcceleration,
                rawAcceleration,
                smoothing * Time.deltaTime
            );
    }

    private void UpdateUI()
    {
        rawValuesText.text =
            $"RAW: " +
            $"X: {rawAcceleration.x:F2} " +
            $"Y: {rawAcceleration.y:F2} " +
            $"Z: {rawAcceleration.z:F2}";

        smoothedValuesText.text =
            $"SUAVIZADO: " +
            $"X: {smoothedAcceleration.x:F2} " +
            $"Y: {smoothedAcceleration.y:F2} " +
            $"Z: {smoothedAcceleration.z:F2}";

        magnitudeText.text =
            $"MAGNITUD: {rawAcceleration.magnitude:F2}";

        if (calibrated)
        {
            Vector3 relativeAcceleration =
                smoothedAcceleration - baselineAcceleration;

            relativeValuesText.text =
                $"RELATIVO: " +
                $"X: {relativeAcceleration.x:F2} " +
                $"Y: {relativeAcceleration.y:F2} " +
                $"Z: {relativeAcceleration.z:F2}";
        }
        else
        {
            relativeValuesText.text =
                "RELATIVO: SIN CALIBRAR";
        }
    }

    private void UpdateMovement()
    {
        if (!calibrated || feedbackObject == null)
        {
            return;
        }

        Vector3 relativeAcceleration =
            smoothedAcceleration - baselineAcceleration;

        Quaternion targetRotation =
            Quaternion.Euler(
                relativeAcceleration.y * movementSensitivity,
                0f,
                -relativeAcceleration.x * movementSensitivity
            );

        feedbackObject.localRotation =
            Quaternion.Lerp(
                feedbackObject.localRotation,
                targetRotation,
                5f * Time.deltaTime
            );
    }

    private void CheckShake()
    {
        float delta =
            (rawAcceleration - previousAcceleration).magnitude;

        if (delta >= shakeThreshold &&
            Time.time >= nextShakeTime)
        {
            DetectShake();

            nextShakeTime =
                Time.time + shakeCooldown;
        }
    }

    private void DetectShake()
    {
        shakeCount++;

        shakeStatusText.text =
            "SHAKE: DETECTADO";

        shakeCountText.text =
            $"SHAKES: {shakeCount}";

        if (feedbackRenderer != null)
        {
            feedbackRenderer.material.color =
                Random.ColorHSV();
        }

        feedbackEndTime =
            Time.time + feedbackDuration;
    }

    private void UpdateShakeFeedback()
    {
        if (Time.time >= feedbackEndTime)
        {
            shakeStatusText.text =
                "SHAKE: NO";
        }
    }

    public void Calibrate()
    {
        baselineAcceleration =
            smoothedAcceleration;

        calibrated = true;
    }
}