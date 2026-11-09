using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;





//Result after the time is over and player is unable to kill other or doesn't die;
public class GameOverResultUI : MonoBehaviour
{
    [Header("Tempalte")]
    [SerializeField] private GameObject finalScoreUITemplate;
    [SerializeField] private Transform finalRankParentTransform;


    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button restartButton;


    [Header("Reference")]
    private GameplayManager gameplayManager;
    private GameStateManager gameStateManager;
    private TouchInputSystem touchInputSystem;
    [SerializeField] private HordeFormation playerHorde;

    [SerializeField] private Image resultImage;
    [SerializeField] private Sprite victorySprite;
    [SerializeField] private GameObject stars;
    [SerializeField] private Sprite defeatSprite;

    [SerializeField] private TextMeshProUGUI resultText; //Win or Lose;
    [SerializeField] private TextMeshProUGUI obtainedRankText;
    [SerializeField] private TextMeshProUGUI winnerText;


    [Header("Settings")]
    private float scoreBarWidthMax = 500f;
    float highestScore;
    private HordeFormation highestScoringHorde;


    [Header("Collection")]
    private List<float> sortedScoreList;
    private List<HordeFormation> sortedHordeByScore;

    [Header("String")]
    private string LOSE = "Defeat";
    private string WIN = "Victory";

    //PlayerPrefs saving level number;
    public const string LEVEL_KEY = "level";
    private int levelNumber = 1;

    /*[Header("Flag")]
    private bool isLevelCompleted = false;*/



    private void Awake()
    {
        if(PlayerPrefs.HasKey(LEVEL_KEY)) levelNumber = PlayerPrefs.GetInt(LEVEL_KEY);  

        sortedScoreList = new List<float>();
        sortedHordeByScore = new List<HordeFormation>();

        touchInputSystem = new TouchInputSystem();
        //touchInputSystem.Enable();
    }


    private void Start()
    {
        gameplayManager = GameplayManager.Instance;
        gameStateManager = GameStateManager.Instance;

        highestScore = 0f;
        highestScoringHorde = gameplayManager.GetHordeFormation()[0];

        gameStateManager.OnStateChanged += GameStateManager_OnStateChanged;

        nextButton.onClick.AddListener(() =>
        {
            AdsManager.Instance.ShowInterstitialAd();
            SceneLoader.LoadScene(/*(SceneLoader.Scene) SceneManager.GetActiveScene().buildIndex*/ SceneManager.GetActiveScene().buildIndex + 1);
            levelNumber++;

            PlayerPrefs.SetInt(LEVEL_KEY, levelNumber);
            PlayerPrefs.Save();
        });
        restartButton.onClick.AddListener(() =>
        {
            AdsManager.Instance.ShowInterstitialAd();
            SceneLoader.ReloadSceneAsRestartGame();
            //OnRestartLevel();
        });
        quitButton.onClick.AddListener(() =>
        {
            SceneLoader.ReloadScene();
        });


        UIManager.Instance.DisableGameOverWindowUI();
    }


    private void GameStateManager_OnStateChanged()
    {
        //if (gameStateManager.IsCountdownToStartActive() || gameStateManager.state == GameStateManager.State.MainMenu || gameStateManager.state == GameStateManager.State.TouchToStart) return;
        if (gameplayManager.GetPlayerRemainingLives(playerHorde) == 0) //If player dead;
        {
            GameStateManager.Instance.isPlayerTheWinner = false;

            UIManager.Instance.EnableGameOverWindowUI();
            CalculateFinalScore(GetDeadHordeList());
            DisplayFinalScores();

            ShowDefeatTemplate();

            Debug.Log("Before Toggle in GameOver : " + Time.timeScale);
            gameStateManager.ToggleGameState();
            Debug.Log("After Toggle in GameOver : " + Time.timeScale);
        }
        else if (gameplayManager.IsPlayerTheOnlySurviver())
        {
            UIManager.Instance.EnableGameOverWindowUI();
            
            CalculateFinalScore(GetDeadHordeList());
            DisplayFinalScores();

            winnerHorde = gameplayManager.playerHorde;
            ShowVictoryTemplate();
            GameStateManager.Instance.isPlayerTheWinner = true;

            Debug.Log("Before Toggle in GameOver : " + Time.timeScale);
            gameStateManager.ToggleGameState();
            Debug.Log("After Toggle in GameOver : " + Time.timeScale);
        }
        else if (gameStateManager.IsGameOver())
        {
            UIManager.Instance.EnableGameOverWindowUI();

            CalculateFinalScore(GetDeadHordeList());
            DisplayFinalScores();

            if (winnerHorde == playerHorde)
            {
                GameStateManager.Instance.isPlayerTheWinner = true;
                ShowVictoryTemplate();
            }
            else
            {
                GameStateManager.Instance.isPlayerTheWinner = false;
                ShowDefeatTemplate();
            }

            Debug.Log("Before Toggle in GameOver : " + Time.timeScale);
            gameStateManager.ToggleGameState();
            Debug.Log("After Toggle in GameOver : " + Time.timeScale);
        }
    }



    private List<HordeFormation> GetDeadHordeList()
    {
        List<HordeFormation> deadHordeList = new List<HordeFormation>();
        foreach (HordeFormation hordeFormation in gameplayManager.GetHordeFormation())
        {
            if (gameplayManager.GetPlayerRemainingLives(hordeFormation) == 0) deadHordeList.Add(hordeFormation);
        }
        return deadHordeList;
    }


    //Logic part of FinalScore calculation;
    private HordeFormation winnerHorde;
    private void CalculateFinalScore(List<HordeFormation> excludingHordes)
    {
        bool shouldIterate = true;
        int highestScoreIndex = 0;

        List<int> alreadySelectedIndexes = new List<int>();
        for (int i = 0; i < gameplayManager.GetScoreList().Count; i++)
        {
            highestScore = 0f;
            for (int j = 0; j < gameplayManager.GetScoreList().Count; j++)
            {
                shouldIterate = true;
                foreach (int index in alreadySelectedIndexes)
                {
                    if (j == index)
                    {
                        shouldIterate = false;
                        break;
                    }
                }

                if (!shouldIterate) continue;

                if (gameplayManager.GetScoreList()[j] > highestScore && shouldIterate)
                {
                    highestScore = gameplayManager.GetScoreList()[j];
                    highestScoreIndex = j;
                    highestScoringHorde = gameplayManager.GetHordeFormation()[j];
                }
            }
            sortedScoreList.Add(highestScore);
            sortedHordeByScore.Add(highestScoringHorde);
            alreadySelectedIndexes.Add(highestScoreIndex);
        }

        winnerHorde = sortedHordeByScore[0];

        for (int k = 0; k < excludingHordes.Count; k++)
        {
            foreach (HordeFormation excludingHorde in excludingHordes)
            {
                if (sortedHordeByScore[k] == excludingHorde)
                {
                    winnerHorde = sortedHordeByScore[k + 1];
                }
                else 
                {
                    winnerHorde = sortedHordeByScore[k];
                    return;
                }
            }
        }
    }

    //Just UI of the FinalScore;
    private void DisplayFinalScores()
    {
        for (int i = 0; i < GameplayManager.Instance.GetHordeFormation().Count; i++)
        {
            ScoreUITemplate scoreUITemplate = Instantiate(finalScoreUITemplate, finalRankParentTransform).GetComponent<ScoreUITemplate>();
            scoreUITemplate.gameObject.SetActive(true);
            scoreUITemplate.transform.localPosition = new Vector2(0f, i * -50f);

            float scoreBarWidth = (sortedScoreList[i] / (ScoreUI.GetHighestScore() + 110f)) * scoreBarWidthMax;
            scoreUITemplate.Initialize(Mathf.FloorToInt(sortedScoreList[i]), scoreBarWidth + 10f);
            scoreUITemplate.scoreBar.color = gameplayManager.GetUnitMaterial(sortedHordeByScore[i]).color;
        }
    }


    private void ShowVictoryTemplate()
    {
        resultImage.sprite = victorySprite;
        resultText.text = WIN;
        resultText.color = Color.white;
        stars.SetActive(true);
        obtainedRankText.text = "Your Rank : #1";
        winnerText.text = "Winner : " + winnerHorde.name;

        winnerText.color = gameplayManager.GetUnitMaterial(winnerHorde).color;

        HideUIButton(new Button[] { nextButton, restartButton, quitButton }); //Just for formality;
        ShowUIButton(new Button[] { nextButton, restartButton, quitButton });
    }

    private void ShowDefeatTemplate()
    {
        resultImage.sprite = defeatSprite;
        resultText.text = LOSE;
        resultText.color = Color.red;
        stars.SetActive(false);
        obtainedRankText.text = "Your Rank : #" + (sortedHordeByScore.IndexOf(playerHorde) + 1).ToString();
        winnerText.text = winnerHorde.name;

        HideUIButton(new Button[] { nextButton, restartButton, quitButton });
        ShowUIButton(new Button[] { quitButton, restartButton });
    }
   




    private void ShowUIButton(Button[] buttons)
    {
        foreach (Button button in buttons)
        {
            button.gameObject.SetActive(true);
        }
    }
    private void HideUIButton(Button[] buttons)
    {
        foreach (Button button in buttons)
        {
            button.gameObject.SetActive(false);
        }
    }



    private void Touch_started(InputAction.CallbackContext obj)
    {
        UIManager.Instance.DisableTouchToStartUI();
        UIManager.Instance.EnableInGameUI();
        //UIManager.Instance.TogglePauseAndSettingsButton();

        touchInputSystem.Touch.Touch.started -= Touch_started;
        touchInputSystem.Touch.Disable();
        touchInputSystem.Dispose();
    }
}