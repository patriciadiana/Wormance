using UnityEngine;

public class GameStateMachine : MonoBehaviour
{
    public static GameStateMachine Instance { get ; private set; }

    public GameState currentState { get; private set; } = GameState.Boot;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetState(GameState next)
    {
        if (currentState == next) return;
        currentState = next;
        GameEvents.RaiseGameStateChanged(next);
    }

    public bool IsPlaying => currentState == GameState.Playing;
}
