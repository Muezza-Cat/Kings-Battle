using TMPro;
using UnityEngine;





public class CountdownUI : MonoBehaviour
{
    [Header("Reference")]
    private TextMeshProUGUI countdownText;
    private GameStateManager gameStateManager;
    private UIManager uiManager;

    [Header("Settings")]
    [SerializeField] private float startCountdownMax = 3f;
    private float countdownElapsedTime = 0f;





    private void Awake()
    {
        countdownText = GetComponent<TextMeshProUGUI>();

        countdownElapsedTime = startCountdownMax;
    }



    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
        uiManager = UIManager.Instance;

        gameStateManager.OnStateChanged += GameStateManager_OnStateChanged;
        Hide();
    }



    private void GameStateManager_OnStateChanged()
    {
        if (gameStateManager.IsCountdownToStartActive())
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    //Animation pending.....
    private void Update()
    {
        countdownElapsedTime -= Time.unscaledDeltaTime;
        countdownText.text = Mathf.CeilToInt(countdownElapsedTime).ToString();

        if (countdownElapsedTime <= 0f)
        {
            gameStateManager.state = GameStateManager.State.Playing;
            gameStateManager.ToggleGameState();

            uiManager.EnableInGameUI();
            Hide();
        }
    }


    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
