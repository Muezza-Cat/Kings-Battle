using TMPro;
using UnityEngine;

public class CoinsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinsCountText;


    private void Start()
    {
        GameplayManager.Instance.playerHorde.UpdateCoinCountUI += PlayerHorde_UpdateCoinCountUI;
    }

    private void PlayerHorde_UpdateCoinCountUI(int coinsCount)
    {
        coinsCountText.text = coinsCount.ToString();
    }
}
