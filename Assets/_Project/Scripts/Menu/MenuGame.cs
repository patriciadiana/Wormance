using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MenuGame : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string menuSceneName = "OanaScene";

    [Header("Settings")]
    [SerializeField] private bool pauseAudio = true;

    private bool menuOpen;
    private bool busy; 
    private CursorLockMode previousLockState;
    private bool previousCursorVisible;

    private void Update()
    {
        if (busy || !MenuKeyPressed()) return;

        if (menuOpen)
        {
            StartCoroutine(CloseMenu());
        }
        else
        {
            if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsDialogueActive()) return;

            StartCoroutine(OpenMenu());
        }
    }

    private bool MenuKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Alpha1);
#endif
    }

    private IEnumerator OpenMenu()
    {
        busy = true;

        PauseGame();

        AsyncOperation load = SceneManager.LoadSceneAsync(menuSceneName, LoadSceneMode.Additive);

        if (load == null)
        {
            Debug.LogError($"[MenuGame] Could not load '{menuSceneName}'. Is it in Build Settings?", this);
            ResumeGame();
            busy = false;
            yield break;
        }

        yield return load;

        menuOpen = true;
        busy = false;
    }

    private IEnumerator CloseMenu()
    {
        busy = true;

        Scene menuScene = SceneManager.GetSceneByName(menuSceneName);
        if (menuScene.isLoaded)
            yield return SceneManager.UnloadSceneAsync(menuScene);

        ResumeGame();

        menuOpen = false;
        busy = false;
    }

    private void PauseGame()
    {
        previousLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;

        if (pauseAudio)
            AudioListener.pause = true;

        if (GameStateMachine.Instance != null)
            GameStateMachine.Instance.SetState(GameState.Paused);
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (GameStateMachine.Instance != null)
            GameStateMachine.Instance.SetState(GameState.Playing);

        Cursor.lockState = previousLockState;
        Cursor.visible = previousCursorVisible;
    }

    private void OnDestroy()
    {
        if (menuOpen)
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}