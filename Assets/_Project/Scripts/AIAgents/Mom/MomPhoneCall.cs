using System.Collections;
using UnityEngine;
using TMPro;

public class MomPhoneCall : MonoBehaviour, IInteractable
{
    private DialogueComponent dialogueComponent;

    [Header("UI Notification")]
    [SerializeField] private TMP_Text globalNotificationText;
    [SerializeField] private string notificationMessage = "Her mom is calling! Go to the phone to answer.";

    private bool _isRinging;
    private bool _callAnswered;

    private void Awake()
    {
        dialogueComponent = GetComponent<DialogueComponent>();

        if (globalNotificationText != null)
            globalNotificationText.gameObject.SetActive(false);
    }

    public void StartRinging(DialogueData dialogue)
    {
        dialogueComponent.dialogueData = dialogue;
        _callAnswered = false;
        _isRinging = true;

        if (globalNotificationText != null)
        {
            globalNotificationText.text = notificationMessage;
            globalNotificationText.gameObject.SetActive(true);
        }
    }

    public string GetPrompt()
    {
        if (!_isRinging || _callAnswered) return "";
        return "Answer Phone";
    }

    public void Interact(GameObject interactor)
    {
        if (!_isRinging || _callAnswered) return;
        if (dialogueComponent == null || dialogueComponent.dialogueData == null) return;

        _callAnswered = true;
        _isRinging = false;

        if (globalNotificationText != null)
            globalNotificationText.gameObject.SetActive(false);

        DialogueSystem.Instance.HandleInteraction(dialogueComponent);
    }
}