using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MenuPlayPanelUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private string gameplaySceneName = "Gameplay";

    private void Start()
    {
        if (playButton != null) playButton.onClick.AddListener(LoadGameplay);
    }

    private void OnDestroy()
    {
        if (playButton != null) playButton.onClick.RemoveListener(LoadGameplay);
    }

    public void LoadGameplay()
    {
        if (playButton != null) playButton.interactable = false;
        SceneManager.LoadSceneAsync(gameplaySceneName);
    }
}
