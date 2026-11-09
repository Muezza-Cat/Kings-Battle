using UnityEngine;
using UnityEngine.UI;



public class UIManager : MonoBehaviour
{

    #region Singleton
    public static UIManager Instance { get; private set; }
    #endregion


    [Header("References")]
    [SerializeField] private GameObject mainMenuContainer;
    [SerializeField] private GameObject pauseWindowUI;
    [SerializeField] private GameObject gameOverWindowUI;
    //[SerializeField] private GameObject settingsWindowUI;
    [SerializeField] private GameObject inGameUI;
    [SerializeField] private GameObject touchToStartUI;
    [SerializeField] private GameObject creditsWindowUI;
    //[SerializeField] private GameObject joystickUI;

    //[SerializeField] private Button settingsButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button openCreditsButton;

    private SettingsUI settingsUI;
    private PauseWindowUI pauseUI;





    private void Awake()
    {
        Instance = this;

        DisableInGameUI();
        DisableTouchToStartUI();

        //settingsUI = settingsWindowUI.GetComponent<SettingsUI>();
        //if (settingsUI == null) Debug.LogWarning("Null reference to settingsUI");
        pauseUI = pauseWindowUI.GetComponent<PauseWindowUI>();
        if (pauseUI == null) Debug.LogWarning("Null reference to pauseUI");
    }


    private void Start()
    {
        //settingsButton.onClick.AddListener(() =>
        //{
        //    EnableSettingsWindow();
        //});
        pauseButton.onClick.AddListener(() =>
        {
            EnablePauseWindow();

            GameStateManager.Instance.ToggleGameState();
        });
        openCreditsButton.onClick.AddListener(() => 
        {
            EnableCreditsWindow();
        });
    }



    //Credits Window
    public void EnableCreditsWindow()
    {
        creditsWindowUI.SetActive(true);
    }

    public void DisableCreditsWindow()
    {
        creditsWindowUI.SetActive(false);
    }


    //MainMenuUI
    public void EnableMainMenuUIContainer()
    {
        mainMenuContainer.SetActive(true);
    }
    public void DisableMainMenuUIContainer()
    {
        mainMenuContainer.SetActive(false);
    }

    //GameOver Window
    public void EnableGameOverWindowUI()
    {
        gameOverWindowUI.SetActive(true);
    }
    public void DisableGameOverWindowUI()
    {
        gameOverWindowUI.SetActive(false);
    }

    //Settings Window
    //public void EnableSettingsWindow()
    //{
    //    settingsWindowUI.SetActive(true);
    //    settingsButton.gameObject.SetActive(false);
    //}
    //public void DisableSettingsWindow()
    //{
    //    settingsWindowUI.SetActive(false);
    //    settingsButton.gameObject.SetActive(true);
    //}

    //Pause Window
    public void EnablePauseWindow()
    {
        pauseWindowUI.SetActive(true);
        pauseButton.gameObject.SetActive(false);
    }
    public void DisablePauseWindow()
    {
        pauseWindowUI.SetActive(false);
        pauseButton.gameObject.SetActive(true);
    }

    //Score UI
    public void EnableInGameUI()
    {
        inGameUI.SetActive(true);
    }
    public void DisableInGameUI()
    {
        inGameUI.SetActive(false);
    }

    //TouchToStart UI
    public void EnableTouchToStartUI()
    {
        touchToStartUI.SetActive(true);
    }
    public void DisableTouchToStartUI()
    {
        touchToStartUI.SetActive(false);
    }


    /*bool toggleButtonActivity = false;
    public void TogglePauseAndSettingsButton()
    {
        toggleButtonActivity = !toggleButtonActivity;

        pauseButton.gameObject.SetActive(toggleButtonActivity);
        settingsButton.gameObject.SetActive(!toggleButtonActivity);
    }*/
}
