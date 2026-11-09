using UnityEngine;
using UnityEngine.UI;

public class CreditsWindowUI : MonoBehaviour
{
    private Button closeCreditsWindowButton;


    private void Awake()
    {
        closeCreditsWindowButton = GetComponentInChildren<Button>();
    }


    private void Start()
    {
        closeCreditsWindowButton.onClick.AddListener(() => 
        {
            UIManager.Instance.DisableCreditsWindow();
        });

        UIManager.Instance.DisableCreditsWindow();
    }
}
