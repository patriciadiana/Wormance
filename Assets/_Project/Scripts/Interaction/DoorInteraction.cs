
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
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
    private NavMeshObstacle _navMeshObstacle;

    public bool IsOpen => isOpen;
    public bool IsMoving => _currentCoroutine != null;

    void Awake()
    {
        _closedRotation = transform.rotation;
        _openRotation = Quaternion.Euler(
            transform.eulerAngles + new Vector3(0, openAngle, 0)
        );

        BoxCollider doorCollider = GetComponentInChildren<BoxCollider>();
        if (doorCollider != null)
        {
            _navMeshObstacle = doorCollider.gameObject.GetComponent<NavMeshObstacle>();
            if (_navMeshObstacle == null)
            {
                _navMeshObstacle = doorCollider.gameObject.AddComponent<NavMeshObstacle>();
            }

            _navMeshObstacle.shape = NavMeshObstacleShape.Box;
            _navMeshObstacle.center = doorCollider.center;
            _navMeshObstacle.size = doorCollider.size;
            _navMeshObstacle.carving = true;
            _navMeshObstacle.enabled = !isOpen;
        }
    }

    void Start()
    {
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
        isOpen = !isOpen;
        yield return RotateTo(isOpen ? _openRotation : _closedRotation);
    }


    private IEnumerator RotateTo(Quaternion targetRotation)
    {
        if (_navMeshObstacle != null)
            _navMeshObstacle.enabled = false;

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
        if (_navMeshObstacle != null)
            _navMeshObstacle.enabled = !isOpen;

        _currentCoroutine = null;
    }
}