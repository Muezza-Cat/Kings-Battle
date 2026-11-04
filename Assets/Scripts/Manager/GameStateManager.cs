using System;
using UnityEngine;
using UnityEngine.UI;


public class GameStateManager : MonoBehaviour
{

    //Under construction;
    public static GameStateManager Instance {  get; private set; }

    #region events
    public event Action OnStateChanged;
    #endregion


    [Header("UI")]
    [SerializeField] private GameObject tapToPlay;

    [Header("Settings")]
    [SerializeField] private float gameplayTimeMax = 6f;
    private float gameplayElapsedTime = 0f;

    public bool isPlayerTheWinner = false;



    //States;
    public enum State
    {
        MainMenu,
        TouchToStart,
        StartCountdown,
        Playing,
        Over
    }
    public State state;


    [Header("Flags")]
    public bool gamePaused = true;
    [HideInInspector] public bool isGameOver = false;


    public void Awake()
    {
        Instance = this;

        gameplayElapsedTime = gameplayTimeMax;
        state = State.MainMenu;
    }
    

    private void Start()
    {
        Debug.Log("Before Toggle In GameStateManager : " + Time.timeScale);
        ToggleGameState();
        Debug.Log("After Toggle In GameStateManager : " + Time.timeScale);
    }


    public void Update()
    {
        
        switch (state)
        {
            default:
            case State.MainMenu:
                break;
            case State.TouchToStart:
                break;
            case State.StartCountdown:
                OnStateChanged?.Invoke();
                break;
            case State.Playing: //TimerUI, Level Text, UI polish pending.....
                gameplayElapsedTime -= Time.deltaTime;
                if (gameplayElapsedTime <= 0f)
                {
                    OnStateChanged?.Invoke();
                    state = State.Over;
                }
                break;
            case State.Over:
                if (isGameOver) return;

                OnStateChanged?.Invoke();
                isGameOver = true;
                if (SaveSystem.Instance != null) SaveSystem.Instance.SaveData(); // Save at game end;
                else Debug.LogWarning("SaveSystem Instance is null");
                // Imp note : Null Reference Exception prevents the code below from executing; Make sure to keep risky code at the bottom with null checks after main logic ends.
                break;
        }
    }




    public bool IsGameOver()
    {
        return state == State.Over;
    }


    public bool IsCountdownToStartActive()
    {
        return state == State.StartCountdown;
    }

    public bool IsPlayerTheWinner()
    {
        return isPlayerTheWinner;
    }


    public void ToggleGameState()
    {
        gamePaused = !gamePaused;

        if (gamePaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}
