using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "Menu";

    public void LoadGameplayScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }
}