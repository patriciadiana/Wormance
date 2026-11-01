using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchToCustomization : MonoBehaviour, IInteractable
{
    public static SwitchToCustomization Instance { get; private set; }

    [Header("Scene Settings")]
    public string customizationSceneName = "CustomizeWorm";

    private bool isCustomizing = false;
    private Camera mainCamera;
    private Canvas mainCanvas;

    private void Awake()
    {
        Instance = this;
    }

    public string GetPrompt()
    {
        if (isCustomizing) return "";
        return "Customize Worm";
    }

    public void Interact(GameObject interactor)
    {
        if (isCustomizing) return;

        mainCamera = Camera.main;

        // Find the main Canvas in the scene and disable it
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var canvas in allCanvases)
        {
            if (canvas.gameObject.scene.name != customizationSceneName)
            {
                mainCanvas = canvas;
                break;
            }
        }

        StartCoroutine(LoadCustomization());
    }

    private IEnumerator LoadCustomization()
    {
        isCustomizing = true;

        GameStateMachine.Instance.SetState(GameState.Paused);

        // Hide Main Scene visuals and UI
        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (mainCanvas != null) mainCanvas.enabled = false;

        // Load Customization Scene on top
        yield return SceneManager.LoadSceneAsync(customizationSceneName, LoadSceneMode.Additive);

        // Disable duplicate Audio Listeners
        Scene customScene = SceneManager.GetSceneByName(customizationSceneName);
        foreach (GameObject rootObj in customScene.GetRootGameObjects())
        {
            if (rootObj.GetComponentInChildren<AudioListener>() != null)
                rootObj.GetComponentInChildren<AudioListener>().enabled = false;
        }
    }

    public void FinishCustomization()
    {
        StartCoroutine(UnloadCustomization());
    }

    private IEnumerator UnloadCustomization()
    {
        // Unload Customization Scene
        yield return SceneManager.UnloadSceneAsync(customizationSceneName);

        // Restore Main Scene visuals and UI
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);
        if (mainCanvas != null) mainCanvas.enabled = true;

        // Update the Worm with the new gear
        WormAssembler wormAssembler = FindAnyObjectByType<WormAssembler>();
        if (wormAssembler != null)
        {
            wormAssembler.ApplySavedCustomization();
        }

        GameStateMachine.Instance.SetState(GameState.Playing);
        isCustomizing = false;
    }
}