using System;
using TMPro;
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
    private UnitTemplateSO unitTemplateSO;
    private UnitSelectionWindowUI unitSelectionWindowUI;


    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image unitImage;
    [SerializeField] private TextMeshProUGUI unitNameText;
    [SerializeField] private Button selectionButton;
    private TextMeshProUGUI selectionButtonText;


    [Header("Settings")]
    private const string SELECTED = "Selected";
    private const string SELECT = "Select";

    private const float bgDefaultAlphaValue = 130f / 255f; //Color uses a normalized value, i.e, 0f - 1f;
    private const float bgSelectedAlphaValue = 1f;

    public bool isSelected = false;



    private void Awake()
    {
        unitSelectionWindowUI = GetComponentInParent<UnitSelectionWindowUI>();
        selectionButtonText = selectionButton.GetComponentInChildren<TextMeshProUGUI>();
    }



    public void Initialize(TemplateDataSO templateDataSO)
    {
        unitTemplateSO = templateDataSO.unitTemplateSO;
        //unitImage.sprite = templateDataSO.unitSprite;
        unitNameText.text = templateDataSO.UNIT_NAME;
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



    public UnitTemplateSO GetUnitTemplateSO()
    {
        return unitTemplateSO;
    }
}
