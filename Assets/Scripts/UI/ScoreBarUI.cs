using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBarUI : MonoBehaviour
{
    [Header("Reference")]
    private GameplayManager gameplayManager;


    [Header("Settings")]
    [HideInInspector] public HordeFormation hordeFormation;

    public Image scoreBar;
    public Image deadStatusIndicator;

    [SerializeField] private GameObject livesCounterUI;
    public TextMeshProUGUI scoreText;
    private TextMeshProUGUI livesCount;


    [Header("Timers")]
    private float updateCooldownTime = 0.1f;
    private float updateCooldownElapsedTime = 0f;


    private void Awake()
    {
        livesCount = livesCounterUI.GetComponentInChildren<TextMeshProUGUI>();
    }



    private void Start()
    {
        //gameplayManager = GameplayManager.Instance;

        StartCoroutine(InitializeGetRemainingLivesRoutine());
    }

    private IEnumerator InitializeGetRemainingLivesRoutine()
    {
        while (true)
        {
            if (hordeFormation != null)
            {
                livesCount.text = GameplayManager.Instance.GetPlayerRemainingLives(hordeFormation).ToString();
                yield break;
            }
            yield return null;
        }
    }
    


    private void Update()
    {
        updateCooldownElapsedTime += Time.deltaTime;
        if (updateCooldownElapsedTime < updateCooldownTime) return;
        updateCooldownElapsedTime = 0f;

        if (GameplayManager.Instance.GetPlayerRemainingLives(hordeFormation) == 0)
        {
            deadStatusIndicator.gameObject.SetActive(true);
            livesCounterUI.SetActive(false);
        }
        else livesCount.text = GameplayManager.Instance.GetPlayerRemainingLives(hordeFormation).ToString();
    }
}
