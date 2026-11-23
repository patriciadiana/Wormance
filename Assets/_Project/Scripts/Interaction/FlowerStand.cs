using UnityEngine;

public class FlowerStand : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject flowerPrefab;   
    [SerializeField] private Transform handAnchor;      

    public string GetPrompt() => "Take a flower";

    public void Interact(GameObject interactor)
    {
        if (flowerPrefab == null || interactor == null || handAnchor == null) return;

        PlayerHandInventory handInventory =
            interactor.GetComponentInParent<PlayerHandInventory>()
            ?? interactor.GetComponentInChildren<PlayerHandInventory>(true);

        if (handInventory == null || handInventory.HasHeldPickup) return;

        GameObject flowerGO = Instantiate(flowerPrefab);
        Pickup flower = flowerGO.GetComponent<Pickup>();
        if (flower == null)
        {
            Destroy(flowerGO);
            return;
        }

        flowerGO.transform.SetParent(handAnchor, false);
        flowerGO.transform.localPosition = Vector3.zero;
        flowerGO.transform.localRotation = Quaternion.identity;

        if (!flower.TryHoldInPlayerHand(handInventory))
        {
            Destroy(flowerGO);
        }
    }
}