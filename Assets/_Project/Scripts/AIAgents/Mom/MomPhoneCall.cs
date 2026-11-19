using System.Collections;
using UnityEngine;
using TMPro;

public class MomPhoneCall : MonoBehaviour, IInteractable
{
    private DialogueComponent dialogueComponent;

    [Header("UI Notification")]
    [SerializeField] private TMP_Text globalNotificationText;
    [SerializeField]
    private string notificationMessage =
        "Her mom is calling! Go to the phone to answer.";

    [Header("Missed Call Suspicion")]
    [SerializeField] private float unansweredCallDelay = 15f;
    [SerializeField] private float missedCallSuspicion = 5f;

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;

    private bool _isRinging;
    private bool _callAnswered;

    private Coroutine missedCallCoroutine;
    private Coroutine ringingAudioCoroutine;

    private void Awake()
    {
        dialogueComponent = GetComponent<DialogueComponent>();

        if (globalNotificationText != null)
            globalNotificationText.gameObject.SetActive(false);
    }

    public void StartRinging(DialogueData dialogue)
    {
        if (dialogueComponent == null || dialogue == null)
            return;

        dialogueComponent.dialogueData = dialogue;

        _callAnswered = false;
        _isRinging = true;

        if (globalNotificationText != null)
        {
            globalNotificationText.text = notificationMessage;
            globalNotificationText.gameObject.SetActive(true);
        }

        if (missedCallCoroutine != null)
            StopCoroutine(missedCallCoroutine);

        if (ringingAudioCoroutine != null)
            StopCoroutine(ringingAudioCoroutine);

        ringingAudioCoroutine = StartCoroutine(LoopRingingAudio());
        missedCallCoroutine = StartCoroutine(MissedCallTimer());
    }

    private IEnumerator LoopRingingAudio()
    {
        while (_isRinging && !_callAnswered)
        {
            if (audioManager != null)
                audioManager.PlaySFX(0);

            yield return new WaitForSeconds(6f);
        }

        ringingAudioCoroutine = null;
    }

    private IEnumerator MissedCallTimer()
    {
        yield return new WaitForSeconds(unansweredCallDelay);

        if (!_callAnswered && _isRinging)
        {
            _isRinging = false;

            if (audioManager != null)
                audioManager.StopSFX();

            if (ringingAudioCoroutine != null)
            {
                StopCoroutine(ringingAudioCoroutine);
                ringingAudioCoroutine = null;
            }

            SuspicionManager.Instance.ModifySuspicion(
                dialogueComponent.dialogueData.npcId,
                missedCallSuspicion
            );

            if (globalNotificationText != null)
                globalNotificationText.gameObject.SetActive(false);
        }

        missedCallCoroutine = null;
    }

    public string GetPrompt()
    {
        if (!_isRinging || _callAnswered)
            return "";

        return "Answer Phone";
    }

    public void Interact(GameObject interactor)
    {
        if (!_isRinging || _callAnswered)
            return;

        if (dialogueComponent == null || dialogueComponent.dialogueData == null)
            return;

        _callAnswered = true;
        _isRinging = false;

        if (audioManager != null)
            audioManager.StopSFX();

        if (ringingAudioCoroutine != null)
        {
            StopCoroutine(ringingAudioCoroutine);
            ringingAudioCoroutine = null;
        }

        if (missedCallCoroutine != null)
        {
            StopCoroutine(missedCallCoroutine);
            missedCallCoroutine = null;
        }

        if (globalNotificationText != null)
            globalNotificationText.gameObject.SetActive(false);

        DialogueSystem.Instance.HandleInteraction(dialogueComponent);
    }
}