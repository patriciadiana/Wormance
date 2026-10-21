
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoorInteraction : MonoBehaviour, IInteractable
{
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public float interactionDistance = 3f;

    public bool isOpen = false;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Coroutine _currentCoroutine;
    private Transform _playerCamera;

    void Start()
    {
        _closedRotation = transform.rotation;
        _openRotation = Quaternion.Euler(
            transform.eulerAngles + new Vector3(0, openAngle, 0)
        );

        Camera mainCamera = Camera.main;

        if (mainCamera != null)
            _playerCamera = mainCamera.transform;
    }

    void Update()
    {
        if (_playerCamera == null || Keyboard.current == null)
            return;

        if (!Keyboard.current.eKey.wasPressedThisFrame)
            return;

        Ray ray = new Ray(
            _playerCamera.position,
            _playerCamera.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            DoorInteraction door =
                hit.collider.GetComponentInParent<DoorInteraction>();

            if (door == this)
                Interact(_playerCamera.gameObject);
        }
    }

    public string GetPrompt()
    {
        return isOpen ? "Close Door" : "Open Door";
    }

    public void Interact(GameObject interactor)
    {
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);

        _currentCoroutine = StartCoroutine(ToggleDoor());
    }

    private IEnumerator ToggleDoor()
    {
        Quaternion targetRotation = isOpen
            ? _closedRotation
            : _openRotation;

        isOpen = !isOpen;

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * openSpeed
            );

            yield return null;
        }

        transform.rotation = targetRotation;
        _currentCoroutine = null;
    }
}