using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [Header("Buttons")]
    public Button newGameButton;
    public Button continueButton;
    public Button optionsButton;
    public Button quitButton;

    [Header("BGs")]
    public GameObject preNewGame;   
    public GameObject postNewGame;   

    private bool gameStarted = false;

    void Start()
    {
        newGameButton.onClick.AddListener(NewGameClicked);
        continueButton.onClick.AddListener(ContinueClicked);
        optionsButton.onClick.AddListener(OptionsClicked);
        quitButton.onClick.AddListener(QuitClicked);

        RefreshMenu();
    }

    void RefreshMenu()
    {
        continueButton.interactable = gameStarted;

        preNewGame.SetActive(!gameStarted);
        postNewGame.SetActive(gameStarted);
    }

    void NewGameClicked()
    {
        gameStarted = true;
        RefreshMenu();
        //SceneManager.LoadScene("_");
    }

    void ContinueClicked()
    {
        //WIP 
    }

    void OptionsClicked()
    {
        //WIP options screen and functionality
    }

    void QuitClicked()
    {
        Application.Quit();
    }
}