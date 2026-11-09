using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;


//Testing Phase;
public class UnitSelectionWindowUI : MonoBehaviour
{
    #region Singleton
    public static UnitSelectionWindowUI Instance;
    #endregion


    #region Events
    public event EventHandler ResetAllTemplates; //Disselect all other and select this one only;
    #endregion



    [Header("Reference")]
    public UnitTemplateListSO unitTemplateListSO;

    [SerializeField] private GameObject baseUnitTemplateUI; //Must have one default;
    [SerializeField] private Transform unitSelectionWindowUITransform;
    [SerializeField] private Transform unitTemplateUIParent;
    public Sprite selectedButtonImage;
    public Sprite unselectedButtonImage;

    private int selectedUnitTemplateIndex;


    [Header("Buttons")]
    [SerializeField] private Button openUnitSelectionUIButton;
    [SerializeField] private Button closeUnitSelectionUIButton;

    [Header("Collection")]
    private List<UnitTemplateUI> unitTemplateList;




    private void Awake()
    {
        Instance = this;

        unitTemplateList = new List<UnitTemplateUI>();

        /*foreach (Transform child in transform)
        {
            if (child == baseUnitTemplateUI.transform) continue;

            UnitTemplateUI unitTemplateUI = child.GetComponent<UnitTemplateUI>();
            unitTemplateList.Add(unitTemplateUI);

            unitTemplateUI.OnTemplateUpdated += UnitTemplateUI_OnTemplateUpdated;
        }*/

        foreach (TemplateDataSO templateDataSO in unitTemplateListSO.templateDataSOList)
        {
            GameObject unitTemplateUIGameObject = Instantiate(baseUnitTemplateUI, unitTemplateUIParent);

            int remainingAdsCount;
            int isLocked;
            if (PlayerPrefs.HasKey(TemplateDataSO.ADS + templateDataSO.unit_ID)) 
            {
                remainingAdsCount = PlayerPrefs.GetInt(TemplateDataSO.ADS + templateDataSO.unit_ID);
            } 
            else
            {
                remainingAdsCount = templateDataSO.adsToWatch;
                PlayerPrefs.SetInt(TemplateDataSO.ADS + templateDataSO.unit_ID, templateDataSO.adsToWatch);
                PlayerPrefs.Save();
            }
            if (PlayerPrefs.HasKey(TemplateDataSO.IS_LOCKED + templateDataSO.unit_ID))
            {
                isLocked = PlayerPrefs.GetInt(TemplateDataSO.IS_LOCKED + templateDataSO.unit_ID);
            }
            else
            {
                isLocked = (templateDataSO.isLocked) ? 1 : 0;
                PlayerPrefs.SetInt(TemplateDataSO.IS_LOCKED + templateDataSO.unit_ID, (templateDataSO.isLocked)? 1 : 0);
                PlayerPrefs.Save();
            }

            unitTemplateUIGameObject.gameObject.SetActive(true);

            UnitTemplateUI unitTemplateUI = unitTemplateUIGameObject.GetComponent<UnitTemplateUI>();
            unitTemplateUI.Initialize(templateDataSO, isLocked, remainingAdsCount);

            unitTemplateList.Add(unitTemplateUI);

            unitTemplateUI.OnTemplateUpdated += UnitTemplateUI_OnTemplateUpdated;
        }

        if (AdsManager.Instance != null && AdsManager.Instance.rewardedAdLoaded)
        {
            foreach (UnitTemplateUI unitTemplateUI in unitTemplateList)
            {
                unitTemplateUI.RedOutWatchAdsButton();
            }
        }
        else
        {
            foreach (UnitTemplateUI unitTemplateUI in unitTemplateList)
            {
                unitTemplateUI.GrayOutWatchAdsButton();
            }
        }
    }


    private void Start()
    {
        openUnitSelectionUIButton.onClick.AddListener(() =>
        {
            ShowWindow();
        });
        closeUnitSelectionUIButton.onClick.AddListener(() =>
        {
            HideWindow();
        });

        HideWindow();
    }





    private void UnitTemplateUI_OnTemplateUpdated(object sender, UnitTemplateUI.OnTemplateUpdatedEventArgs e)
    {
        selectedUnitTemplateIndex = unitTemplateList.IndexOf(e.unitTemplateUI);
        SaveSystem.Instance.SaveData();

        ResetAllTemplates?.Invoke(this, EventArgs.Empty);
    }


    public List<UnitTemplateUI> GetUnitTemplateList()
    {
        return unitTemplateList;
    }

    public int GetSelectedUnitTemplateIndex()
    {
        return selectedUnitTemplateIndex;
    }


    #region SaveSystem
    [Serializable]
    public class SaveObject
    {
        public int unitTemplateIndex;
    }




    public SaveObject Save()
    {
        return new SaveObject() { unitTemplateIndex = selectedUnitTemplateIndex};
    }

    public void Load(SaveObject saveObject)
    {
        if (saveObject.unitTemplateIndex < 0)
        {
            Debug.LogWarning("Invalid unitTemplateIndex");
            return;
        }

        selectedUnitTemplateIndex = saveObject.unitTemplateIndex;

        unitTemplateList[saveObject.unitTemplateIndex].DisplayAsSelected();
        unitTemplateList[saveObject.unitTemplateIndex].isSelected = true;
    }
    #endregion

    private void ShowWindow()
    {
        unitSelectionWindowUITransform.gameObject.SetActive(true);
    }


    private void HideWindow()
    {
        unitSelectionWindowUITransform.gameObject.SetActive(false);
    }
}
