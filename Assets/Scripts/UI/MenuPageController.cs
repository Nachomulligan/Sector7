using UnityEngine;
using UnityEngine.UI;

public enum MenuPage { Gacha, Inventory, Play }

public sealed class MenuPageController : MonoBehaviour
{
    [SerializeField] private GameObject gachaPage;
    [SerializeField] private GameObject inventoryPage;
    [SerializeField] private GameObject playPage;
    [SerializeField] private Button gachaButton;
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button playButton;
    [SerializeField] private MenuPage initialPage = MenuPage.Play;
    private bool initialized;

    private void Start()
    {
        if (!HasReferences) return;
        gachaButton.onClick.AddListener(ShowGacha);
        inventoryButton.onClick.AddListener(ShowInventory);
        playButton.onClick.AddListener(ShowPlay);
        initialized = true;
        Show(initialPage);
    }

    private void OnDestroy()
    {
        if (!initialized) return;
        gachaButton.onClick.RemoveListener(ShowGacha);
        inventoryButton.onClick.RemoveListener(ShowInventory);
        playButton.onClick.RemoveListener(ShowPlay);
    }

    public void ShowGacha() => Show(MenuPage.Gacha);
    public void ShowInventory() => Show(MenuPage.Inventory);
    public void ShowPlay() => Show(MenuPage.Play);

    public void Show(MenuPage page)
    {
        GameObject[] pages = { gachaPage, inventoryPage, playPage };
        Button[] buttons = { gachaButton, inventoryButton, playButton };
        int selected = (int)page;
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null) pages[i].SetActive(i == selected);
            if (buttons[i] != null && buttons[i].targetGraphic is Image image)
                image.color = i == selected ? new Color(0.15f, 0.82f, 0.9f, 1f)
                    : new Color(0.11f, 0.18f, 0.29f, 1f);
        }
    }

    private bool HasReferences => gachaPage != null && inventoryPage != null && playPage != null &&
        gachaButton != null && inventoryButton != null && playButton != null;
}
