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
    public TextMeshProUGUI eyewearNameText;

    private int currentHatIndex = 0;
    private int currentEyewearIndex = 0;

    private GameObject currentHatModel;
    private GameObject currentEyewearModel;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (CustomizationData.selectedHatIndex >= 0
            && availableHats != null
            && CustomizationData.selectedHatIndex < availableHats.Length)
        {
            currentHatIndex = CustomizationData.selectedHatIndex;
        }

        if (CustomizationData.selectedEyewearIndex >= 0
            && availableEyewear != null
            && CustomizationData.selectedEyewearIndex < availableEyewear.Length)
        {
            currentEyewearIndex = CustomizationData.selectedEyewearIndex;
        }

        UpdateHat();
        UpdateEyewear();
    }

    public void NextHat()
    {
        if (availableHats == null || availableHats.Length == 0) return;
        currentHatIndex++;
        if (currentHatIndex >= availableHats.Length) currentHatIndex = 0;
        UpdateHat();
    }

    public void PreviousHat()
    {
        if (availableHats == null || availableHats.Length == 0) return;
        currentHatIndex--;
        if (currentHatIndex < 0) currentHatIndex = availableHats.Length - 1;
        UpdateHat();
    }

    private void UpdateHat()
    {
        if (currentHatModel != null)
        {
            currentHatModel.transform.SetParent(null);
            Destroy(currentHatModel);
            currentHatModel = null;
        }

        if (availableHats != null && availableHats.Length > 0)
        {
            AccessoryData data = availableHats[currentHatIndex];

            if (data != null && data.prefab != null)
            {
                currentHatModel = Instantiate(data.prefab, hatSocket);
                currentHatModel.transform.localPosition = Vector3.zero;
                currentHatModel.transform.localRotation = Quaternion.identity;
            }

            if (hatNameText != null && data != null)
                hatNameText.text = data.accessoryName;
        }
    }

    public void NextEyewear()
    {
        if (availableEyewear == null || availableEyewear.Length == 0) return;
        currentEyewearIndex++;
        if (currentEyewearIndex >= availableEyewear.Length) currentEyewearIndex = 0;
        UpdateEyewear();
    }

    public void PreviousEyewear()
    {
        if (availableEyewear == null || availableEyewear.Length == 0) return;
        currentEyewearIndex--;
        if (currentEyewearIndex < 0) currentEyewearIndex = availableEyewear.Length - 1;
        UpdateEyewear();
    }

    private void UpdateEyewear()
    {
        if (currentEyewearModel != null)
        {
            currentEyewearModel.transform.SetParent(null);
            Destroy(currentEyewearModel);
            currentEyewearModel = null;
        }

        if (availableEyewear != null && availableEyewear.Length > 0)
        {
            AccessoryData data = availableEyewear[currentEyewearIndex];

            if (data != null && data.prefab != null)
            {
                currentEyewearModel = Instantiate(data.prefab, eyewearSocket);
                currentEyewearModel.transform.localPosition = Vector3.zero;
                currentEyewearModel.transform.localRotation = Quaternion.identity;
            }

            if (eyewearNameText != null && data != null)
                eyewearNameText.text = data.accessoryName;
        }
    }

    public void SaveAndPlay()
    {
        CustomizationData.selectedHatIndex = currentHatIndex;
        CustomizationData.selectedEyewearIndex = currentEyewearIndex;
        CustomizationData.availableHats = availableHats;
        CustomizationData.availableEyewear = availableEyewear;

        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

        if (SwitchToCustomization.Instance != null)
        {
            SwitchToCustomization.Instance.FinishCustomization();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
        }
    }
}