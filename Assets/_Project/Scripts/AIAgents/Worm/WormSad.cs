using UnityEngine;

[RequireComponent(typeof(WormAI))]
public class WormSad : MonoBehaviour
{
    private enum State
    {
        Normal,         
        GoingToSad,    
        Sad,             
        GoingToRecover   
    }

    [Header("Music Point")]
    [SerializeField] private Transform sadPoint;
    [SerializeField] private float arriveDistance = 1.5f;

    [Header("Happiness")]
    [SerializeField] private int sadThreshold = 30;
    [SerializeField] private int recoverThreshold = 40;

    [Header("Music")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioClip sadMusic;

    private WormAI worm;
    private State state = State.Normal;

    private void Awake()
    {
        worm = GetComponent<WormAI>();

        if (audioManager == null)
        {
            audioManager = FindAnyObjectByType<AudioManager>();
        }
    }

    private void Update()
    {
        if (worm == null || worm.Stats == null || sadPoint == null) return;

        int happiness = worm.Stats.Happiness;

        switch (state)
        {
            case State.Normal:
                if (happiness < sadThreshold)
                {
                    state = State.GoingToSad;
                    worm.OverrideDestination = sadPoint;
                }
                break;

            case State.GoingToSad:
                if (happiness >= recoverThreshold)
                {
                    state = State.Normal;
                    worm.OverrideDestination = null;
                }
                else if (HasArrived())
                {
                    if (audioManager != null && sadMusic != null)
                    {
                        audioManager.ChangeMusic(sadMusic);
                    }

                    state = State.Sad;
                    worm.OverrideDestination = null;
                }
                break;

            case State.Sad:
                if (happiness >= recoverThreshold)
                {
                    state = State.GoingToRecover;
                    worm.OverrideDestination = sadPoint;
                }
                break;

            case State.GoingToRecover:
                if (happiness < sadThreshold)
                {
                    state = State.Sad;
                    worm.OverrideDestination = null;
                }
                else if (HasArrived())
                {
                    if (audioManager != null)
                    {
                        audioManager.RestoreBackgroundMusic();
                    }

                    state = State.Normal;
                    worm.OverrideDestination = null;
                }
                break;
        }
    }

    private bool HasArrived()
    {
        Vector3 offset = sadPoint.position - transform.position;
        offset.y = 0f;
        return offset.magnitude <= arriveDistance;
    }

    private void OnDisable()
    {
        if (worm != null)
        {
            worm.OverrideDestination = null;
        }

        if ((state == State.Sad || state == State.GoingToRecover) && audioManager != null)
        {
            audioManager.RestoreBackgroundMusic();
        }

        state = State.Normal;
    }
}