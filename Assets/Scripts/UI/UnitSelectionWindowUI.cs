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
            unitTemplateUIGameObject.gameObject.SetActive(true);

            UnitTemplateUI unitTemplateUI = unitTemplateUIGameObject.GetComponent<UnitTemplateUI>();
            unitTemplateUI.Initialize(templateDataSO);

            unitTemplateList.Add(unitTemplateUI);

            unitTemplateUI.OnTemplateUpdated += UnitTemplateUI_OnTemplateUpdated;
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



    private void ShowWindow()
    {
        unitSelectionWindowUITransform.gameObject.SetActive(true);
    }


    private void HideWindow()
    {
        unitSelectionWindowUITransform.gameObject.SetActive(false);
    }
}
