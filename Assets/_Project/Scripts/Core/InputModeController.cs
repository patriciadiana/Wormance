using UnityEngine;

public class InputModeController : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerLook playerLook;

    void OnEnable() => GameEvents.OnGameStateChanged += HandleStateChanged;
    void OnDisable() => GameEvents.OnGameStateChanged -= HandleStateChanged;

    void Start() => ApplyMode(GameStateMachine.Instance.currentState);

    void HandleStateChanged(GameState current) => ApplyMode(current);

    void ApplyMode(GameState state)
    {
        bool playing = state == GameState.Playing;
        bool inDialogue = state == GameState.Dialogue;

        Cursor.lockState = playing ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !playing;

        if (inputHandler) inputHandler.enabled = playing || inDialogue;
        if (playerMovement) playerMovement.enabled = playing;
        if (playerLook) playerLook.enabled = playing;
    }
}