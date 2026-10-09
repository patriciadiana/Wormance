using System;
using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 2.5f;
    [SerializeField] private LayerMask interactMask;

    public event Action<IInteractable> OnTargetChanged;

    private PlayerInputHandler input;
    private IInteractable currentTarget;

    void Awake() => input = GetComponent<PlayerInputHandler>();

    void Update()
    {
        DetectTarget();

        if (input.InteractPressed)
        {
            currentTarget?.Interact(gameObject);
            input.ConsumeInteract();
        }
    }

    void DetectTarget()
    {
        IInteractable newTarget = null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactMask))
        {
            newTarget = hit.collider.GetComponentInParent<IInteractable>();
        }

        if (newTarget != currentTarget)
        {
            currentTarget = newTarget;
            OnTargetChanged?.Invoke(currentTarget);
        }
    }
}
