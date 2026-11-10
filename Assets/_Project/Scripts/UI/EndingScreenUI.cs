using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndingScreenUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button mainMenuButton;

    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.OnGameEnded += ShowEnding;
    }

    private void OnDisable()
    {
        GameEvents.OnGameEnded -= ShowEnding;
    }

    private void Start()
    {
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    private void ShowEnding(string title, string description)
    {
        panel.SetActive(true);
        titleText.text = title;
        descriptionText.text = description;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}