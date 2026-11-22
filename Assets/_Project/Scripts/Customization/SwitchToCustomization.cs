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

        Canvas[] allCanvases = FindObjectsByType<Canvas>();
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

        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (mainCanvas != null) mainCanvas.enabled = false;

        yield return SceneManager.LoadSceneAsync(customizationSceneName, LoadSceneMode.Additive);

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
        yield return SceneManager.UnloadSceneAsync(customizationSceneName);

        if (mainCamera != null) mainCamera.gameObject.SetActive(true);
        if (mainCanvas != null) mainCanvas.enabled = true;

        WormAssembler wormAssembler = FindAnyObjectByType<WormAssembler>();
        if (wormAssembler != null)
        {
            wormAssembler.ApplySavedCustomization();
        }

        GameStateMachine.Instance.SetState(GameState.Playing);
        isCustomizing = false;
    }
}