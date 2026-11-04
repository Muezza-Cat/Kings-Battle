using System;
using UnityEngine;
using UnityEngine.UI;




public class PauseWindowUI : MonoBehaviour
{
    [SerializeField] private Button pauseButton;

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button crossButton;
    [SerializeField] private Button quitButton;





    private void Start()
    {
        resumeButton.onClick.AddListener(() =>
        {
            GameStateManager.Instance.ToggleGameState();
            UIManager.Instance.DisablePauseWindow();
        });
        crossButton.onClick.AddListener(() =>
        {
            GameStateManager.Instance.ToggleGameState();
            UIManager.Instance.DisablePauseWindow();
        });



        quitButton.onClick.AddListener(() =>
        {
            SceneLoader.ReloadScene();
        });
        restartButton.onClick.AddListener(() =>
        {
            // <Testing>
            GameStateManager.Instance.ToggleGameState();
            // </Testing>

            SceneLoader.ReloadSceneAsRestartGame();

        });

        UIManager.Instance.DisablePauseWindow();
    }
}
