using System;
using System.Collections;
using Unity.Services.LevelPlay;
using UnityEngine;





public class AdsManager : MonoBehaviour
{
    #region Singleton
    public static AdsManager Instance { get; private set; }
    #endregion



    [Header("Important keys")]
    [SerializeField] private string APP_KEY;
    [SerializeField] private string REWARDED_AD_KEY;
    [SerializeField] private string INTERSTITIAL_AD_KEY;

    [Header("Private Fields")]
    private LevelPlayInterstitialAd interstitialAd;
    private LevelPlayRewardedAd rewardedAd;

    public event Action OnRewardedAdCompleted;

    private bool successfullyInitialized = false;

    public bool rewardedAdLoaded = false;
    private int interstitialRetryAttempt;
    private int rewardedRetryAttempt;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);                      
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }


    private void Start()
    {
        Debug.Log($"MY GAME: ADS MANAGER STARTED | ID: {GetInstanceID()}");

        LevelPlay.OnInitSuccess += LevelPlay_OnInitSuccess;
        LevelPlay.OnInitFailed += LevelPlay_OnInitFailed;

        LevelPlay.Init(APP_KEY);
    }


    private void LevelPlay_OnInitSuccess(LevelPlayConfiguration obj)
    {
        Debug.Log("MY GAME: INIT SUCCESS");
        successfullyInitialized = true;

        //Create Ad Objects;
        CreateInterstitialAdUnit();
        CreateRewardedAdUnit();


        //Load Ads before displaying them;
        interstitialAd.LoadAd();
        rewardedAd.LoadAd();

    }

    private void LevelPlay_OnInitFailed(LevelPlayInitError obj)
    {
        Debug.Log("MY GAME: LEVELPLAY INITIALIZATION FAILED: " + obj);

        successfullyInitialized = false;
    }



    #region Interstitial
    private void CreateInterstitialAdUnit()
    {
        interstitialAd = new LevelPlayInterstitialAd(INTERSTITIAL_AD_KEY);

        interstitialAd.OnAdLoaded += InterstitialAd_OnAdLoaded;
        interstitialAd.OnAdLoadFailed += InterstitialAd_OnAdLoadFailed;
        interstitialAd.OnAdDisplayFailed += InterstitialAd_OnAdDisplayFailed;
        interstitialAd.OnAdClosed += InterstitialAd_OnAdClosed;
    }


    private void InterstitialAd_OnAdLoaded(LevelPlayAdInfo obj)
    {
        Debug.Log("MY GAME: The Interstitial Ad was loaded");
        interstitialRetryAttempt = 0;
    }
    private void InterstitialAd_OnAdLoadFailed(LevelPlayAdError obj)
    {
        Debug.Log($"MY GAME: Interstitial Ad was NOT Loaded ErrorMessage : {obj.ErrorMessage} ErrorCode : {obj.ErrorCode}");

        interstitialRetryAttempt++;
        float delay = Mathf.Pow(2, Mathf.Min(6, interstitialRetryAttempt));
        StartCoroutine(RetryLoadInterstitial(delay));
    }

    private IEnumerator RetryLoadInterstitial(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        interstitialAd.LoadAd();
    }


    private void InterstitialAd_OnAdDisplayFailed(LevelPlayAdInfo arg1, LevelPlayAdError arg2)
    {
        Debug.Log("MY GAME: Interstitial Ad display failed");
        interstitialAd.LoadAd();
    }
    private void InterstitialAd_OnAdClosed(LevelPlayAdInfo obj)
    {
        Debug.Log("MY GAME: The Interstitial Ad was Closed");
        interstitialAd.LoadAd(); //load another ad;
    }

    private bool IsInterstitialAdLoaded()
    {
        return successfullyInitialized && interstitialAd != null && interstitialAd.IsAdReady();
    }

    public void ShowInterstitialAd()
    {
        if (!IsInterstitialAdLoaded()) return;

        interstitialAd.ShowAd();
    }
    #endregion


    #region Rewarded
    private void CreateRewardedAdUnit()
    {
        rewardedAd = new LevelPlayRewardedAd(REWARDED_AD_KEY);

        rewardedAd.OnAdLoaded += RewardedAd_OnAdLoaded;
        rewardedAd.OnAdLoadFailed += RewardedAd_OnAdLoadFailed;
        rewardedAd.OnAdDisplayFailed += RewardedAd_OnAdDisplayFailed;
        rewardedAd.OnAdRewarded += RewardedAd_OnAdRewarded;
        rewardedAd.OnAdClosed += RewardedAd_OnAdClosed;
    }


    private void RewardedAd_OnAdLoaded(LevelPlayAdInfo obj)
    {
        Debug.Log("MY GAME: Rewarded Ad was Loaded");

        rewardedAdLoaded = true;
        rewardedRetryAttempt = 0;
    }
    private void RewardedAd_OnAdLoadFailed(LevelPlayAdError obj)
    {
        Debug.Log($"MY GAME: Rewarded Ad was NOT Loaded ErrorMessage : {obj.ErrorMessage} ErrorCode : {obj.ErrorCode}");
        OnRewardedAdCompleted = null;

        rewardedAdLoaded = false;

        rewardedRetryAttempt++;
        float delay = Mathf.Pow(2, Mathf.Min(6, rewardedRetryAttempt));
        StartCoroutine(RetryLoadRewarded(delay));
        //rewardedAd.LoadAd();
    }

    private IEnumerator RetryLoadRewarded(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        rewardedAd.LoadAd();
    }

    private void RewardedAd_OnAdDisplayFailed(LevelPlayAdInfo arg1, LevelPlayAdError arg2)
    {
        Debug.Log("MY GAME: Rewarded Ad display failed");

        rewardedAd.LoadAd();
    }
    private void RewardedAd_OnAdRewarded(LevelPlayAdInfo arg1, LevelPlayReward arg2)
    {
        Debug.Log("MY GAME: Reward Rewarded");
        OnRewardedAdCompleted?.Invoke();
        OnRewardedAdCompleted = null;
    }

    private void RewardedAd_OnAdClosed(LevelPlayAdInfo obj)
    {
        Debug.Log("MY GAME: Rewarded Ad Closed");
        rewardedAd.LoadAd();
    }



    private bool IsRewardedAdLoaded()
    {
        return successfullyInitialized && rewardedAd != null && rewardedAd.IsAdReady();
    }

    public void ShowRewardedAd(Action rewardCallback)
    {
        if (!IsRewardedAdLoaded()) return;

        OnRewardedAdCompleted = null;
        OnRewardedAdCompleted = rewardCallback;
        rewardedAd.ShowAd();
    }
    #endregion


    private void OnDestroy()
    {
        if (Instance != this) return;
        LevelPlay.OnInitSuccess -= LevelPlay_OnInitSuccess;
        LevelPlay.OnInitFailed -= LevelPlay_OnInitFailed;
    }
}
