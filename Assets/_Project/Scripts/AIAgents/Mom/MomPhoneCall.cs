
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
    [SerializeField] private float unansweredCallDelay = 5f;
    [SerializeField] private float missedCallSuspicion = 5f;

    private bool _isRinging;
    private bool _callAnswered;
    private Coroutine missedCallCoroutine;

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

        missedCallCoroutine = StartCoroutine(MissedCallTimer());
    }

    private IEnumerator MissedCallTimer()
    {
        yield return new WaitForSeconds(unansweredCallDelay);

        if (!_callAnswered && _isRinging)
        {
            SuspicionManager.Instance.ModifySuspicion(
                dialogueComponent.dialogueData.npcId,
                missedCallSuspicion
            );

            Debug.Log($"Mom's suspicion increased by {missedCallSuspicion} because the phone call was unanswered.");
            _isRinging = false;

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