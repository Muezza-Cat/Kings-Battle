using System;
using TMPro;
using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.UI;



public class UnitTemplateUI : MonoBehaviour
{
    #region Events
    public event EventHandler<OnTemplateUpdatedEventArgs> OnTemplateUpdated;
    #endregion

    public class OnTemplateUpdatedEventArgs : EventArgs
    {
        public UnitTemplateUI unitTemplateUI;
    }




    [Header("Reference")]
    private TemplateDataSO templateDataSO;
    private UnitSelectionWindowUI unitSelectionWindowUI;


    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image unitImage;
    [SerializeField] private TextMeshProUGUI unitNameText;
    [SerializeField] private Button selectionButton;
    private TextMeshProUGUI selectionButtonText;
    
    [SerializeField] private Button watchAdsButton;
    private TextMeshProUGUI watchAdsButtonText;

    [SerializeField] private TextMeshProUGUI statsInfo;

    [SerializeField] private Sprite grayedButtonSprite;
    [SerializeField] private Sprite redButtonSprite;


    [Header("Settings")]
    private const string SELECTED = "Selected";
    private const string SELECT = "Select";

    private const float bgDefaultAlphaValue = 200f / 255f; //Color uses a normalized value, i.e, 0f - 1f;
    private const float bgSelectedAlphaValue = 1f;

    public bool isSelected = false;
    private int remainingAdsCount;



    private void Awake()
    {
        unitSelectionWindowUI = GetComponentInParent<UnitSelectionWindowUI>();
        selectionButtonText = selectionButton.GetComponentInChildren<TextMeshProUGUI>();
        watchAdsButtonText = watchAdsButton.GetComponentInChildren<TextMeshProUGUI>();

        //Testing
        selectionButton.gameObject.SetActive(true);
        watchAdsButton.gameObject.SetActive(false);
    }



    public void Initialize(TemplateDataSO templateDataSO, int isLocked, int remainingAdsToWatch)
    {
        if (templateDataSO == null) return;

        this.templateDataSO = templateDataSO;
        unitNameText.text = templateDataSO.UNIT_NAME;
        unitImage.sprite = templateDataSO.unitSprite; //Incomplete
        backgroundImage.color = templateDataSO.templateBGColor;
        statsInfo.text = $"Dmg : {templateDataSO.damage}\n HP : {templateDataSO.hp}";

        //Testing
        remainingAdsCount = remainingAdsToWatch;

        if (isLocked == 1) TemplateButtonsOnLocked(remainingAdsToWatch);
        else TemplateButtonOnUnlocked();
    }


    //Visual on Initialization
    public void TemplateButtonsOnLocked(int adsRemaining)
    {
        watchAdsButton.gameObject.SetActive(true);
        selectionButton.gameObject.SetActive(false);

        watchAdsButtonText.text = $"Watch Ad {adsRemaining} / {templateDataSO.adsToWatch}";
    }

    public void TemplateButtonOnUnlocked()
    {
        watchAdsButton.gameObject.SetActive(false);
        selectionButton.gameObject.SetActive(true);    
    }



    private void Start()
    {
        unitSelectionWindowUI.ResetAllTemplates += ResetToDefault;

        if (isSelected) DisplayAsSelected();

        selectionButton.onClick.AddListener(() =>
        {
            //UI Part;
            if (isSelected) return;

            OnTemplateUpdated?.Invoke(this, new OnTemplateUpdatedEventArgs() { unitTemplateUI = this});

            isSelected = true;
            DisplayAsSelected();
        });

        watchAdsButton.onClick.AddListener(() =>
        {
            AdsManager.Instance.ShowRewardedAd(OnRewardRewarded);
        });
    }

    private void OnRewardRewarded()
    {
        remainingAdsCount = Mathf.Max(0, remainingAdsCount - 1);
        if (remainingAdsCount <= 0) 
        {
            PlayerPrefs.SetInt(TemplateDataSO.IS_LOCKED + templateDataSO.unit_ID, 0);
            TemplateButtonOnUnlocked();
        } 

        PlayerPrefs.SetInt(TemplateDataSO.ADS + templateDataSO.unit_ID, remainingAdsCount);
        PlayerPrefs.Save();

        Debug.Log("Updated adsToWatch");
        watchAdsButtonText.text = $"Watch Ad {remainingAdsCount} / {templateDataSO.adsToWatch}";
    }


    public void DisplayAsSelected()
    {
        selectionButtonText.text = SELECTED;
        selectionButton.image.sprite = unitSelectionWindowUI.selectedButtonImage;

        Color color = backgroundImage.color;
        color.a = bgSelectedAlphaValue;
        backgroundImage.color = color;
    }

    public void ResetToDefault(object sender, EventArgs e)
    {
        isSelected = false;
        selectionButtonText.text = SELECT;
        selectionButton.image.sprite = unitSelectionWindowUI.unselectedButtonImage;

        Color color = backgroundImage.color;
        color.a = bgDefaultAlphaValue;
        backgroundImage.color = color;
    }

    public TemplateDataSO GetTemplateDataSO()
    {
        return templateDataSO;
    }

    public UnitTemplateSO GetUnitTemplateSO()
    {
        return templateDataSO.unitTemplateSO;
    }

    public void GrayOutWatchAdsButton()
    {
        watchAdsButton.image.sprite = grayedButtonSprite;
    }

    public void RedOutWatchAdsButton()
    {
        watchAdsButton.image.sprite = redButtonSprite;
    }
}
