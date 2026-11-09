using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddUnitsUIButton : MonoBehaviour
{
    #region Singleton
    public static AddUnitsUIButton Instance { get; private set; }
    #endregion

    private GameplayManager gameplayManager;

    private Button button;
    private Image buttonImage;
    [SerializeField] private TextMeshProUGUI unitCountText;

    [Header("Settings")]
    private float unclickableButtonAlphaTransparency;
    private float clickableButtonAlphaTransparency;


    private void Awake()
    {
        Instance = this;

        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();

        clickableButtonAlphaTransparency = 1f;
        unclickableButtonAlphaTransparency = 80f / 255f;
    }

    private void Start()
    {
        button.onClick.AddListener(() =>
        {
            GameplayManager.Instance.playerHorde.AddUnit();
        });
    }



    private float uiElemsResetTime = 0.1f;
    private float uiElemsUpdateElapsedTime = 0f;
    private void Update()
    {
        uiElemsUpdateElapsedTime += Time.deltaTime;
        if (uiElemsUpdateElapsedTime < uiElemsResetTime) return;
        uiElemsUpdateElapsedTime = 0f;

        unitCountText.text = $"{GameplayManager.Instance.playerHorde.GetUnitCount()} / 26";

        if (GameplayManager.Instance.playerHorde.IsFlagNearby() || (GameplayManager.Instance.playerHorde.transform.position - GameplayManager.Instance.GetHomeTransform(GameplayManager.Instance.playerHorde).position).sqrMagnitude < 10f * 10f)
        {
            DisplayAsClickable();
        }
        else
        {
            DisplayAsUnclickable();
        }
    }



    private void DisplayAsClickable()
    {
        Color color = buttonImage.color;
        color.a = clickableButtonAlphaTransparency;
        buttonImage.color = color;
    }

    private void DisplayAsUnclickable()
    {
        Color color = buttonImage.color;
        color.a = unclickableButtonAlphaTransparency;
        buttonImage.color = color;
    }
}
