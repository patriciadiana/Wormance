using UnityEngine;

public class WormAssembler : MonoBehaviour
{
    [Header("Sockets")]
    public Transform hatSocket;
    public Transform eyewearSocket;

    void Start()
    {
        ApplySavedCustomization();
    }

    public void ApplySavedCustomization()
    {
        int hatIdx = CustomizationData.selectedHatIndex;
        if (hatIdx >= 0 && CustomizationData.availableHats != null && hatIdx < CustomizationData.availableHats.Length)
        {
            AccessoryData hatData = CustomizationData.availableHats[hatIdx];
            if (hatData != null && hatData.prefab != null)
            {
                GameObject hat = Instantiate(hatData.prefab, hatSocket);
                hat.transform.localPosition = Vector3.zero;
                hat.transform.localRotation = Quaternion.identity;
            }
        }

        int eyeIdx = CustomizationData.selectedEyewearIndex;
        if (eyeIdx >= 0 && CustomizationData.availableEyewear != null && eyeIdx < CustomizationData.availableEyewear.Length)
        {
            AccessoryData eyeData = CustomizationData.availableEyewear[eyeIdx];
            if (eyeData != null && eyeData.prefab != null)
            {
                GameObject eye = Instantiate(eyeData.prefab, eyewearSocket);
                eye.transform.localPosition = Vector3.zero;
                eye.transform.localRotation = Quaternion.identity;
            }
        }
    }
}