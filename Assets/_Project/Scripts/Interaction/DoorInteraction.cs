using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DoorInteraction : MonoBehaviour, IInteractable
{
    public float openAngle = 90f;
    public float openSpeed = 2f;

    public bool isOpen = false;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Coroutine _currentCoroutine;
    private NavMeshObstacle _navMeshObstacle;

    public bool IsOpen => isOpen;
    public bool IsMoving => _currentCoroutine != null;

    public bool TryGetApproachPosition(Vector3 fromPosition, float distance, out Vector3 approachPosition)
    {
        Collider doorCollider = GetComponentInChildren<Collider>();
        if (doorCollider == null)
        {
            approachPosition = transform.position;
            return false;
        }

        Vector3 closestPoint = doorCollider.ClosestPoint(fromPosition);
        Vector3 direction = closestPoint - doorCollider.bounds.center;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = fromPosition - doorCollider.bounds.center;
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < 0.001f)
        {
            approachPosition = transform.position;
            return false;
        }

        approachPosition = closestPoint + direction.normalized * distance;
        approachPosition.y = fromPosition.y;
        return true;
    }

    public bool TryGetCenteredApproachPosition(Vector3 fromPosition, float distance, out Vector3 approachPosition)
    {
        Renderer doorRenderer = GetComponentInChildren<Renderer>();
        if (doorRenderer == null)
        {
            return TryGetApproachPosition(fromPosition, distance, out approachPosition);
        }

        Vector3 center = doorRenderer.bounds.center;
        center.y = fromPosition.y;

        Vector3 normal = doorRenderer.transform.right;
        normal.y = 0f;
        if (normal.sqrMagnitude < 0.001f)
        {
            normal = transform.forward;
            normal.y = 0f;
        }

        if (normal.sqrMagnitude < 0.001f)
        {
            normal = (fromPosition - center).normalized;
        }
        else
        {
            normal.Normalize();
            if (Vector3.Dot(fromPosition - center, normal) < 0f)
            {
                normal = -normal;
            }
        }

        approachPosition = center + normal * distance;
        approachPosition.y = fromPosition.y;
        return true;
    }

    void Awake()
    {
        _closedRotation = transform.rotation;

        _openRotation = Quaternion.Euler(
            transform.eulerAngles + new Vector3(0, openAngle, 0)
        );

        BoxCollider doorCollider = GetComponentInChildren<BoxCollider>();

        if (doorCollider != null)
        {
            _navMeshObstacle =
                doorCollider.gameObject.GetComponent<NavMeshObstacle>();

            if (_navMeshObstacle == null)
            {
                _navMeshObstacle =
                    doorCollider.gameObject.AddComponent<NavMeshObstacle>();
            }

            _navMeshObstacle.shape = NavMeshObstacleShape.Box;
            _navMeshObstacle.center = doorCollider.center;
            _navMeshObstacle.size = doorCollider.size;
            _navMeshObstacle.carving = true;
            _navMeshObstacle.enabled = !isOpen;
        }
    }

    public string GetPrompt()
    {
        return isOpen ? "Close Door" : "Open Door";
    }

    public void Interact(GameObject interactor)
    {
        if (_currentCoroutine != null)
        {
            return;
        }

        _currentCoroutine = StartCoroutine(ToggleDoor());
    }

    private IEnumerator ToggleDoor()
    {
        isOpen = !isOpen;

        yield return RotateTo(
            isOpen ? _openRotation : _closedRotation
        );
    }

    private IEnumerator RotateTo(Quaternion targetRotation)
    {
        if (_navMeshObstacle != null)
        {
            _navMeshObstacle.enabled = false;
        }

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
        {
            _navMeshObstacle.enabled = !isOpen;
        }

        _currentCoroutine = null;
    }
}