using GoogleMobileAds.Api;
using UnityEngine;

public class GoogleAdMobBanner : MonoBehaviour
{
    public void Start()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus => { });
        LoadAd();
    }

    // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-3940256099942544~3347511713";
#elif UNITY_IPHONE
    private readonly string _adUnitId = "cca-app-pub-8110178142432057/3131330955";
#else
    private string _adUnitId = "unused";
#endif

    BannerView _bannerView;

    /// <summary>
    /// Creates the banner view and loads a banner ad.
    /// </summary>
    public void LoadAd()
    {
        // Create an instance of a banner view first.
        if (_bannerView == null)
        {
            CreateBannerView();
        }

        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // Send the request to load the ad.
        Debug.Log("Loading banner ad.");
        _bannerView.LoadAd(adRequest);
    }

    /// <summary>
    /// Creates a 320x50 banner view at the top of the screen.
    /// </summary>
    public void CreateBannerView()
    {
        Debug.Log("Creating banner view");

        // If we already have a banner, destroy the old one.
        if (_bannerView != null)
        {
            DestroyAd();
        }

        // Create a 320x50 banner at the top of the screen.
        _bannerView = new BannerView(_adUnitId, AdSize.Banner, AdPosition.Top);

        // Add event listeners for ad loading success and failure.
        _bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner ad loaded successfully - showing the ad.");
            _bannerView.Show();
        };
        _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError($"Banner ad failed to load: {error.GetMessage()}");
        };
    }

    /// <summary>
    /// Destroys the banner view.
    /// </summary>
    public void DestroyAd()
    {
        if (_bannerView != null)
        {
            Debug.Log("Destroying banner view.");
            _bannerView.Destroy();
            _bannerView = null;
        }
    }
}
