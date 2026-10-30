using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public sealed class UIButtonAudioFeedback : MonoBehaviour
{
    private Button button;

    private void Awake() => button = GetComponent<Button>();

    private void OnEnable()
    {
        if (button == null) button = GetComponent<Button>();
        button.onClick.AddListener(PlayUniversalSound);
    }

    private void OnDisable()
    {
        if (button != null) button.onClick.RemoveListener(PlayUniversalSound);
    }

    private static void PlayUniversalSound()
    {
        if (ServiceLocator.Instance.TryGet(out AudioService audioService))
            audioService.PlayUniversalButton();
    }
}
