using UnityEngine;

[DisallowMultipleComponent]
public sealed class CompanionController : MonoBehaviour
{
    private CompanionFollower activeVisual;

    public void SetCompanion(CompanionPresetSO preset)
    {
        if (activeVisual != null) Destroy(activeVisual.gameObject);
        activeVisual = null;
        if (preset == null || preset.VisualPrefab == null) return;
        activeVisual = Instantiate(preset.VisualPrefab, transform.position, Quaternion.identity);
        activeVisual.Initialize(transform, preset);
    }
}
