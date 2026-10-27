using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class LightSwitchInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private Light[] lightsToControl;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private TMP_Text interactionPrompt;

    [SerializeField] private Transform switchLever;
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;
    [SerializeField] private Vector3 leverDirection = Vector3.right;
    [SerializeField] private float flipAngle = 35f;
    [SerializeField] private float animationDuration = 0.15f;

    private Transform playerCamera;
    private Quaternion downRotation;
    private Quaternion upRotation;
    private Coroutine animationCoroutine;
    private bool isOn;

    private static LightSwitchInteraction currentSwitch;

    private void Awake()
    {
        if (switchLever != null)
        {
            downRotation = switchLever.localRotation;

            Vector3 worldAxis =
                switchLever.parent != null
                    ? switchLever.parent.TransformDirection(
                        downRotation * rotationAxis.normalized)
                    : downRotation * rotationAxis.normalized;

            Vector3 worldDirection =
                switchLever.TransformDirection(leverDirection.normalized);

            Quaternion positiveRotation =
                downRotation * Quaternion.AngleAxis(
                    flipAngle, rotationAxis.normalized);

            Quaternion negativeRotation =
                downRotation * Quaternion.AngleAxis(
                    -flipAngle, rotationAxis.normalized);

            Vector3 positiveDirection =
                Quaternion.AngleAxis(flipAngle, worldAxis) * worldDirection;

            Vector3 negativeDirection =
                Quaternion.AngleAxis(-flipAngle, worldAxis) * worldDirection;

            upRotation = positiveDirection.y >= negativeDirection.y
                ? positiveRotation
                : negativeRotation;

            switchLever.localRotation = downRotation;
        }

        foreach (Light lightSource in lightsToControl)
        {
            if (lightSource != null)
                lightSource.enabled = false;
        }

        isOn = false;
    }

    private void Start()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
            playerCamera = mainCamera.transform;

        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (playerCamera == null || Keyboard.current == null)
            return;

        LightSwitchInteraction detectedSwitch = null;

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
            detectedSwitch = hit.collider.GetComponentInParent<LightSwitchInteraction>();

        if (currentSwitch != detectedSwitch)
        {
            if (currentSwitch != null)
                currentSwitch.SetPromptVisible(false);

            currentSwitch = detectedSwitch;

            if (currentSwitch != null)
                currentSwitch.SetPromptVisible(true);
        }

        if (detectedSwitch == this && Keyboard.current.eKey.wasPressedThisFrame)
            Interact(playerCamera.gameObject);
    }

    public string GetPrompt()
    {
        return isOn ? "Turn Off Light" : "Turn On Light";
    }

    public void Interact(GameObject interactor)
    {
        isOn = !isOn;

        foreach (Light lightSource in lightsToControl)
        {
            if (lightSource != null)
                lightSource.enabled = isOn;
        }

        if (switchLever != null)
        {
            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);

            animationCoroutine = StartCoroutine(
                AnimateSwitch(isOn ? upRotation : downRotation));
        }

        SetPromptVisible(true);
    }

    private IEnumerator AnimateSwitch(Quaternion targetRotation)
    {
        Quaternion startRotation = switchLever.localRotation;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f, 1f, Mathf.Clamp01(elapsed / animationDuration));

            switchLever.localRotation = Quaternion.Slerp(
                startRotation, targetRotation, t);

            yield return null;
        }

        switchLever.localRotation = targetRotation;
        animationCoroutine = null;
    }

    private void SetPromptVisible(bool visible)
    {
        if (interactionPrompt == null)
            return;

        interactionPrompt.text = "Press E to " + GetPrompt();
        interactionPrompt.gameObject.SetActive(visible);
    }

    private void OnDisable()
    {
        if (currentSwitch == this)
        {
            currentSwitch = null;

            if (interactionPrompt != null)
                interactionPrompt.gameObject.SetActive(false);
        }
    }
}