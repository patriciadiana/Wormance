
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MomPhoneCall : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    [SerializeField] private DialogueComponent dialogueComponent;

    [Header("Interaction Prompt")]
    [SerializeField] private TMP_Text interactionPrompt;

    [Header("Call Settings")]
    [SerializeField] private float initialCallDelay = 5f;
    [SerializeField] private float interactionDistance = 3f;

    private Transform _playerCamera;
    private bool _isRinging;
    private bool _callAnswered;

    private static MomPhoneCall currentPhone;

    private void Awake()
    {
        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(false);
    }


    private IEnumerator Start()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
            _playerCamera = mainCamera.transform;

        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(false);

        yield return new WaitForSeconds(initialCallDelay);

        _isRinging = true;

        if (interactionPrompt != null)
        {
            interactionPrompt.text =
                "Her mom is calling… Go to the phone to answer.";
            interactionPrompt.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (_playerCamera == null || Keyboard.current == null)
            return;

        Ray ray = new Ray(
            _playerCamera.position,
            _playerCamera.forward
        );

        MomPhoneCall detectedPhone = null;

        if (_isRinging && !_callAnswered &&
            Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            detectedPhone =
                hit.collider.GetComponentInParent<MomPhoneCall>();
        }

        if (currentPhone != detectedPhone)
        {
            if (currentPhone != null)
                currentPhone.SetPromptVisible(false);

            currentPhone = detectedPhone;

            if (currentPhone != null)
                currentPhone.SetPromptVisible(true);
        }

        if (Keyboard.current.eKey.wasPressedThisFrame &&
            detectedPhone != null)
        {
            detectedPhone.Interact(_playerCamera.gameObject);
        }
    }


    private void SetPromptVisible(bool visible)
    {
        if (interactionPrompt == null)
            return;

        if (!_isRinging || _callAnswered)
        {
            interactionPrompt.gameObject.SetActive(false);
            return;
        }

        interactionPrompt.text = visible
            ? "Press [E] to Answer Phone"
            : "Her mom is calling… Go to the phone to answer.";

        interactionPrompt.gameObject.SetActive(true);
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

        if (dialogueComponent == null ||
            dialogueComponent.dialogueData == null)
        {
            Debug.LogWarning(
                "[MomPhoneCall] Assign a DialogueComponent and its dialogue asset."
            );
            return;
        }

        if (DialogueSystem.Instance == null)
        {
            Debug.LogWarning("[MomPhoneCall] DialogueSystem not found.");
            return;
        }

        if (DialogueSystem.Instance.IsDialogueActive())
            return;

        _callAnswered = true;
        _isRinging = false;

        SetPromptVisible(false);

        if (currentPhone == this)
            currentPhone = null;

        DialogueSystem.Instance.HandleInteraction(dialogueComponent);
    }

    private void OnDisable()
    {
        if (currentPhone == this)
            currentPhone = null;

        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(false);
    }
}
