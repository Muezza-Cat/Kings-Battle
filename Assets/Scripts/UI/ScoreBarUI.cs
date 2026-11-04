using UnityEngine;
using UnityEngine.UI;

public class ScoreBarUI : MonoBehaviour
{
    [HideInInspector] public HordeFormation hordeFormation;
    public Image scoreBar;
    public Image deadStatusIndicator;


    [Header("Settings")]
    private float updateCooldownTime = 0.1f;
    private float updateCooldownElapsedTime = 0f;


    private void Update()
    {
        updateCooldownElapsedTime += Time.deltaTime;
        if (updateCooldownElapsedTime < updateCooldownTime) return;
        updateCooldownElapsedTime = 0f;

        if (GameplayManager.Instance.GetPlayerRemainingLives(hordeFormation) == 0) deadStatusIndicator.gameObject.SetActive(true);
    }
}
