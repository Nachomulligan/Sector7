using GoogleMobileAds.Api;
using TMPro;
using UnityEngine;

public class Clase08AdsManager : MonoBehaviour
{
    private BannerView bannerView;
    private RewardedAd rewardedAd;
    private InterstitialAd interstitialAd;

    [SerializeField]
    private TMP_Text coinsText;

    private int coins = 0;

    private void Start()
    {
        coinsText.text = $"MONEDAS: {coins}";

        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("Google Mobile Ads inicializado");
        });
    }

    // BANNER

    private string GetBannerAdUnitId()
    {
#if UNITY_ANDROID
        return "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IOS
        return "ca-app-pub-3940256099942544/2934735716";
#else
        return "unused";
#endif
    }

    public void LoadBanner()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
        }

        bannerView = new BannerView(
            GetBannerAdUnitId(),
            AdSize.Banner,
            AdPosition.Bottom
        );

        AdRequest request = new AdRequest();

        bannerView.LoadAd(request);

        Debug.Log("Banner solicitado");
    }

    public void ShowBanner()
    {
        if (bannerView == null)
        {
            Debug.Log("Banner no disponible");
            return;
        }

        bannerView.Show();
    }

    public void HideBanner()
    {
        if (bannerView == null)
        {
            return;
        }

        bannerView.Hide();
    }

    public void DestroyBanner()
    {
        if (bannerView == null)
        {
            return;
        }

        bannerView.Destroy();
        bannerView = null;
    }

    // REWARDED

    private string GetRewardedAdUnitId()
    {
#if UNITY_ANDROID
        return "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IOS
        return "ca-app-pub-3940256099942544/1712485313";
#else
        return "unused";
#endif
    }

    public void LoadRewarded()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        AdRequest request = new AdRequest();

        RewardedAd.Load(
            GetRewardedAdUnitId(),
            request,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null)
                {
                    Debug.Log(
                        $"Error cargando Rewarded: {error}"
                    );

                    return;
                }

                rewardedAd = ad;

                RegisterRewardedEvents();

                Debug.Log("Rewarded cargado");
            }
        );
    }

    public void ShowRewarded()
    {
        if (
            rewardedAd == null ||
            !rewardedAd.CanShowAd()
        )
        {
            Debug.Log("Rewarded no disponible");
            return;
        }

        rewardedAd.Show(reward =>
        {
            coins += 10;

            coinsText.text =
                $"MONEDAS: {coins}";

            Debug.Log(
                $"Recompensa recibida: {reward.Amount}"
            );
        });
    }

    private void RegisterRewardedEvents()
    {
        rewardedAd.OnAdFullScreenContentClosed += () =>
        {
            rewardedAd.Destroy();
            rewardedAd = null;

            Debug.Log("Rewarded cerrado");
        };

        rewardedAd.OnAdFullScreenContentFailed += error =>
        {
            Debug.Log(
                $"Error mostrando Rewarded: {error}"
            );
        };
    }

    // INTERSTITIAL

    private string GetInterstitialAdUnitId()
    {
#if UNITY_ANDROID
        return "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IOS
        return "ca-app-pub-3940256099942544/4411468910";
#else
        return "unused";
#endif
    }

    public void LoadInterstitial()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        AdRequest request = new AdRequest();

        InterstitialAd.Load(
            GetInterstitialAdUnitId(),
            request,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null)
                {
                    Debug.Log(
                        $"Error cargando Interstitial: {error}"
                    );

                    return;
                }

                interstitialAd = ad;

                RegisterInterstitialEvents();

                Debug.Log("Interstitial cargado");
            }
        );
    }

    public void ShowInterstitial()
    {
        if (
            interstitialAd == null ||
            !interstitialAd.CanShowAd()
        )
        {
            Debug.Log("Interstitial no disponible");
            return;
        }

        interstitialAd.Show();
    }

    private void RegisterInterstitialEvents()
    {
        interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            interstitialAd.Destroy();
            interstitialAd = null;

            Debug.Log("Interstitial cerrado");
        };

        interstitialAd.OnAdFullScreenContentFailed += error =>
        {
            Debug.Log(
                $"Error mostrando Interstitial: {error}"
            );
        };
    }

    // LIMPIEZA

    private void OnDestroy()
    {
        DestroyBanner();

        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }
    }
}