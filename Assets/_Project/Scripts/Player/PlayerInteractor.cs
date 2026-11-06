using System;
using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 2.5f;
    [SerializeField] private LayerMask interactMask;

    [SerializeField] private PlayerHandInventory handInventory;

    public event Action<IInteractable> OnTargetChanged;

    private PlayerInputHandler input;
    private IInteractable currentTarget;

    void Awake() => input = GetComponent<PlayerInputHandler>();

    void Update()
    {
        if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsDialogueActive())
        {
            return;
        }

        DetectTarget();

        if (input.InteractPressed)
        {
            
            if (currentTarget is WormAI worm)
            {
                Debug.LogWarning("give pickup");
                Debug.Log("target is worm");
                worm.TryReceiveHeldItem(this.gameObject);

                Pickup heldPickup = GetComponentInChildren<Pickup>();
                if (heldPickup != null)
                {
                    Destroy(heldPickup.gameObject);
                }
            }
            else
            {

                    if(currentTarget != null)
                   {
                currentTarget.Interact(gameObject);
            }
        
            input.ConsumeInteract();
            }

            
        }
        if(input.DropPressed)
        {
            if(handInventory.HasHeldPickup)
            {
                handInventory.TryDropHeldPickup(playerCamera.transform.position + playerCamera.transform.forward, playerCamera.transform.forward);
            }
            input.ConsumeDrop();
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
