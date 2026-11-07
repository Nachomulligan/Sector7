using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SimulatedGemPackUI : MonoBehaviour
{
    [Min(1)] [SerializeField] private int gems = 100;
    [Min(0)] [SerializeField] private float priceUsd = 0.99f;
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_Text feedback;

    private void Start()
    {
        if (label != null) label.text = $"{gems} GEMAS\nUSD {priceUsd:0.00}";
        if (buyButton != null) buyButton.onClick.AddListener(Buy);
    }
    private void OnDestroy() { if (buyButton != null) buyButton.onClick.RemoveListener(Buy); }
    private void Buy()
    {
        if (gems <= 0 || !ServiceLocator.Instance.TryGet(out PlayerInventory inventory))
        {
            if (feedback != null) feedback.text = "NO SE PUDO COMPLETAR LA COMPRA";
            return;
        }
        inventory.AddCurrency(GachaCurrency.Premium, gems);
        if (ServiceLocator.Instance.TryGet(out AudioService audioService))
            audioService.PlayGemPurchaseFeedback();
        if (feedback != null) feedback.text = $"COMPRA SIMULADA · +{gems} GEMAS";
    }
}
