using UnityEngine;

[DisallowMultipleComponent]
public sealed class CompanionController : MonoBehaviour
{
    private CompanionFollower activeVisual;
    private CompanionPresetSO activePreset;

    public CompanionPresetSO ActivePreset => activePreset;

    public void SetCompanion(CompanionPresetSO preset)
    {
        if (preset == activePreset && activeVisual != null)
        {
            activeVisual.Initialize(transform, preset);
            return;
        }

        ClearCompanion();
        activePreset = preset;
        if (preset == null || preset.VisualPrefab == null) return;

        activeVisual = Instantiate(preset.VisualPrefab, transform.position, Quaternion.identity);
        activeVisual.name = $"Companion_{preset.DisplayName}";
        activeVisual.Initialize(transform, preset);
    }

    public void ClearCompanion()
    {
        if (activeVisual != null) Destroy(activeVisual.gameObject);
        activeVisual = null;
        activePreset = null;
    }

    private void OnDestroy()
    {
        if (activeVisual != null) Destroy(activeVisual.gameObject);
    }
}
