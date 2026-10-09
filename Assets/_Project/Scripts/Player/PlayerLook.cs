using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerLook : MonoBehaviour
{
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float maxPitch = 85f;

    private PlayerInputHandler input;
    private float pitch;

    void Awake() => input = GetComponent<PlayerInputHandler>();

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        transform.Rotate(Vector3.up * input.Look.x * sensitivity);

        pitch -= input.Look.y * sensitivity;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
        cameraHolder.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
