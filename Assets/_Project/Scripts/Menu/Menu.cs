using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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

    [Header("Canvases")]
    public GameObject menuCanvas;      
    public GameObject optionsCanvas;  
    public Button optionsBackButton;  

    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "MainScene";

    private bool gameStarted = false;

    void Start()
    {
    
        bool overRunningGame = SceneManager.GetSceneByName(gameSceneName).isLoaded;

        if (overRunningGame)
        {
            PrepareAsPauseMenu();
        }
        else
        {
         
            Time.timeScale = 1f;
            AudioListener.pause = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            CheckEventSystem();
        }

       
        IgnoreRaycasts(preNewGame);
        IgnoreRaycasts(postNewGame);

        Hook(newGameButton, NewGameClicked);
        Hook(continueButton, ContinueClicked);
        Hook(optionsButton, OptionsClicked);
        Hook(quitButton, QuitClicked);
        Hook(optionsBackButton, OptionsBackClicked);

        if (optionsCanvas != null)
            optionsCanvas.SetActive(false); 

        RefreshMenu();

       
        if (overRunningGame)
        {
            if (newGameButton != null) newGameButton.gameObject.SetActive(false);
            if (continueButton != null) continueButton.gameObject.SetActive(false);
        }
    }

    
    void PrepareAsPauseMenu()
    {
        gameStarted = true;

        foreach (Canvas canvas in GetSceneComponents<Canvas>())
        {
            if (!canvas.isRootCanvas) continue;

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
        }

        foreach (Camera cam in GetSceneComponents<Camera>()) cam.enabled = false;
        foreach (AudioListener listener in GetSceneComponents<AudioListener>()) listener.enabled = false;
        foreach (Light sceneLight in GetSceneComponents<Light>()) sceneLight.enabled = false;
        foreach (EventSystem eventSystem in FindObjectsByType<EventSystem>())
        {
            if (eventSystem.gameObject.scene != gameObject.scene)
            {
                foreach (EventSystem own in GetSceneComponents<EventSystem>())
                    own.enabled = false;
                break;
            }
        }
    }

    List<T> GetSceneComponents<T>() where T : Component
    {
        var found = new List<T>();

        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            found.AddRange(root.GetComponentsInChildren<T>(true));

        return found;
    }

    void Hook(Button button, UnityAction action)
    {
        if (button == null) return;

        int persistent = button.onClick.GetPersistentEventCount();
        for (int i = 0; i < persistent; i++)
            button.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    void IgnoreRaycasts(GameObject root)
    {
        if (root == null) return;

        foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true))
            graphic.raycastTarget = false;
    }

    void CheckEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            Debug.LogWarning("[Menu] There is no EventSystem in this scene, so no button can be clicked. " +
                             "Add one with GameObject > UI > Event System.", this);
            return;
        }

    }

    void RefreshMenu()
    {
        if (continueButton != null)
            continueButton.interactable = gameStarted;

        if (preNewGame != null) preNewGame.SetActive(!gameStarted);
        if (postNewGame != null) postNewGame.SetActive(gameStarted);
    }

    public void NewGameClicked()
    {
        gameStarted = true;
        RefreshMenu();
        SceneManager.LoadScene(gameSceneName);
    }

    public void ContinueClicked()
    {
        //WIP
    }

    public void OptionsClicked()
    {
        menuCanvas.SetActive(false);
        optionsCanvas.SetActive(true);
    }

    public void OptionsBackClicked()
    {
        if (menuCanvas == null || optionsCanvas == null) return;

        optionsCanvas.SetActive(false);
        menuCanvas.SetActive(true);
    }

    public void QuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}