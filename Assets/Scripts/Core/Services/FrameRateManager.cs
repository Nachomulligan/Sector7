using UnityEngine;

/// <summary>Configura de forma explícita el ritmo de render para todas las escenas.</summary>
[DefaultExecutionOrder(-1200)]
[DisallowMultipleComponent]
public sealed class FrameRateManager : MonoBehaviour
{
    [Min(30)]
    [SerializeField] private int targetFrameRate = 60;

    public int TargetFrameRate => targetFrameRate;

    private void Awake()
    {
        Apply();
    }

    public void Apply()
    {
        // Android e iOS ignoran vSyncCount y utilizan targetFrameRate.
        // Mantener VSync desactivado también hace que PC respete el mismo límite.
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = Mathf.Max(30, targetFrameRate);
    }
}
