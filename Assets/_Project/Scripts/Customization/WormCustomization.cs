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
        // 1. Unlock and show the mouse for UI clicking
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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

        if (availableHats.Length > 0)
        {
            AccessoryData data = availableHats[currentHatIndex];

            if (data.prefab != null)
            {
                currentHatModel = Instantiate(data.prefab, hatSocket);
                currentHatModel.transform.localPosition = Vector3.zero;
                currentHatModel.transform.localRotation = Quaternion.identity;
            }

            if (hatNameText != null)
                hatNameText.text = data.accessoryName;
        }
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

        if (availableEyewear.Length > 0)
        {
            AccessoryData data = availableEyewear[currentEyewearIndex];

            if (data.prefab != null)
            {
                currentEyewearModel = Instantiate(data.prefab, eyewearSocket);
                currentEyewearModel.transform.localPosition = Vector3.zero;
                currentEyewearModel.transform.localRotation = Quaternion.identity;
            }

            if (eyewearNameText != null)
                eyewearNameText.text = data.accessoryName;
        }
    }

    public void SaveAndPlay()
    {
        // 1. Save the choices
        CustomizationData.selectedHatIndex = currentHatIndex;
        CustomizationData.selectedEyewearIndex = currentEyewearIndex;
        CustomizationData.availableHats = availableHats;
        CustomizationData.availableEyewear = availableEyewear;

        // 2. Clear UI selection so it doesn't eat the 'E' key in the Main Scene
        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

        // 3. Close the scene using the Trigger
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