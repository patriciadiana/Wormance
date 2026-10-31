using TMPro;
using UnityEngine;

public class WormCustomization : MonoBehaviour
{
    [Header("Sockets")]
    public Transform hatSocket;
    public Transform eyewearSocket;

    [Header("Available Items")]
    public AccessoryData[] availableHats;
    public AccessoryData[] availableEyewear;

    [Header("UI References")]
    public TextMeshProUGUI hatNameText;
    public TMPro.TMP_Text eyewearNameText;

    private int currentHatIndex = 0;
    private int currentEyewearIndex = 0;

    private GameObject currentHatModel;
    private GameObject currentEyewearModel;

    void Start()
    {
        UpdateHat();
        UpdateEyewear();
    }

    public void NextHat()
    {
        if (availableHats.Length == 0) return;
        currentHatIndex++;
        if (currentHatIndex >= availableHats.Length) currentHatIndex = 0;
        UpdateHat();
    }

    public void PreviousHat()
    {
        if (availableHats.Length == 0) return;
        currentHatIndex--;
        if (currentHatIndex < 0) currentHatIndex = availableHats.Length - 1;
        UpdateHat();
    }

    private void UpdateHat()
    {
        if (currentHatModel != null) Destroy(currentHatModel);

        if (availableHats.Length > 0 && availableHats[currentHatIndex].prefab != null)
        {
            currentHatModel = Instantiate(availableHats[currentHatIndex].prefab, hatSocket);
            currentHatModel.transform.localPosition = Vector3.zero;
            currentHatModel.transform.localRotation = Quaternion.identity;
        }

        if (hatNameText != null && availableHats.Length > 0)
            hatNameText.text = availableHats[currentHatIndex].accessoryName;
    }

    public void NextEyewear()
    {
        if (availableEyewear.Length == 0) return;
        currentEyewearIndex++;
        if (currentEyewearIndex >= availableEyewear.Length) currentEyewearIndex = 0;
        UpdateEyewear();
    }

    public void PreviousEyewear()
    {
        if (availableEyewear.Length == 0) return;
        currentEyewearIndex--;
        if (currentEyewearIndex < 0) currentEyewearIndex = availableEyewear.Length - 1;
        UpdateEyewear();
    }

    private void UpdateEyewear()
    {
        if (currentEyewearModel != null) Destroy(currentEyewearModel);

        if (availableEyewear.Length > 0 && availableEyewear[currentEyewearIndex].prefab != null)
        {
            currentEyewearModel = Instantiate(availableEyewear[currentEyewearIndex].prefab, eyewearSocket);
            currentEyewearModel.transform.localPosition = Vector3.zero;
            currentEyewearModel.transform.localRotation = Quaternion.identity;
        }

        if (eyewearNameText != null && availableHats.Length > 0)
            eyewearNameText.text = availableHats[currentHatIndex].accessoryName;
    }
}