using System;
using Unity.Hierarchy;
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
        }
    }


    private void Start()
    {
        LevelPlay.OnInitSuccess += LevelPlay_OnInitSuccess;
        LevelPlay.OnInitFailed += LevelPlay_OnInitFailed;

        LevelPlay.Init(APP_KEY);
    }


    private void LevelPlay_OnInitSuccess(LevelPlayConfiguration obj)
    {
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
        successfullyInitialized = false;
        Debug.LogError("LevelPlay Initialization Failed!");
    }



    #region Interstitial
    private void CreateInterstitialAdUnit()
    {
        interstitialAd = new LevelPlayInterstitialAd(INTERSTITIAL_AD_KEY);

        interstitialAd.OnAdLoaded += InterstitialAd_OnAdLoaded;
        interstitialAd.OnAdLoadFailed += InterstitialAd_OnAdLoadFailed;
        interstitialAd.OnAdClosed += InterstitialAd_OnAdClosed;
    }


    private void InterstitialAd_OnAdLoaded(LevelPlayAdInfo obj)
    {
        Debug.Log("The Interstitial Ad was loaded");
    }
    private void InterstitialAd_OnAdLoadFailed(LevelPlayAdError obj)
    {
        Debug.Log("The Interstitial Ad was NOT loaded");
        interstitialAd.LoadAd(); //Try load again.
    }
    private void InterstitialAd_OnAdClosed(LevelPlayAdInfo obj)
    {
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
        rewardedAd.OnAdRewarded += RewardedAd_OnAdRewarded;
        rewardedAd.OnAdClosed += RewardedAd_OnAdClosed;
    }

    private void RewardedAd_OnAdLoaded(LevelPlayAdInfo obj)
    {
        Debug.Log("Rewarded Ad was Loaded");
    }
    private void RewardedAd_OnAdLoadFailed(LevelPlayAdError obj)
    {
        Debug.Log("Rewarded Ad was NOT Loaded");
        rewardedAd.LoadAd();
    }

    private void RewardedAd_OnAdRewarded(LevelPlayAdInfo arg1, LevelPlayReward arg2)
    {
        isRewardRewarded = true;
        Debug.Log("Reward Rewarded");
        OnRewardedAdCompleted?.Invoke();
        OnRewardedAdCompleted = null;

        Time.timeScale = 0f; //Dummy;
    }

    private bool isRewardRewarded = false;
    private void RewardedAd_OnAdClosed(LevelPlayAdInfo obj)
    {

        if (!isRewardRewarded)
        { 
            OnRewardedAdCompleted = null;
            Debug.Log("Cleared OnRewardAdCompleted");
        }
        //Closed in between;
        rewardedAd.LoadAd();

        Time.timeScale = 0f; //Dummy;
    }



    private bool IsRewardedAdLoaded()
    {
        return successfullyInitialized && rewardedAd != null && rewardedAd.IsAdReady();
    }

    public void ShowRewardedAd(Action rewardCallback)
    {
        if (!IsRewardedAdLoaded()) return;

        OnRewardedAdCompleted = rewardCallback;
        isRewardRewarded = false;
        rewardedAd.ShowAd();
        Time.timeScale = 1f; //Dummy;
    }
    #endregion


    private void OnDisable()
    {
        LevelPlay.OnInitSuccess -= LevelPlay_OnInitSuccess;
        LevelPlay.OnInitFailed -= LevelPlay_OnInitFailed;
    }
}
