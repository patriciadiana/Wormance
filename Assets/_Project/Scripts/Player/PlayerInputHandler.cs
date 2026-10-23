using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }

    public bool JumpPressed { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool GivePressed { get; private set; }

    private PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => Move = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => Move = Vector2.zero;

        controls.Player.Look.performed += ctx => Look = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => Look = Vector2.zero;

        controls.Player.Jump.performed += ctx => JumpPressed = true;
        controls.Player.Interact.performed += ctx => InteractPressed = true;
    }

    void OnEnable() => controls.Player.Enable();
    void OnDisable() => controls.Player.Disable();

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            GivePressed = true;
        }
    }

    public void ConsumeJump() => JumpPressed = false;
    public void ConsumeInteract() => InteractPressed = false;
    public void ConsumeGive() => GivePressed = false;
}
