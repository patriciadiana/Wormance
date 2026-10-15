using UnityEngine;
using UnityEngine.InputSystem;

public class ParallaxController : MonoBehaviour
{
    [Tooltip("How far this layer moves (in world units) at the screen edge. Use small values for far layers, bigger for near layers.")]
    public Vector2 strength = new Vector2(0.3f, 0.2f);

    [Tooltip("Higher = snappier, lower = floatier.")]
    public float smoothing = 5f;

    [Tooltip("Tick this so the layer moves opposite to the cursor.")]
    public bool invert = false;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Cursor position normalized to -1..1 (0,0 = screen center)
        Vector2 mouse = Mouse.current.position.ReadValue();
        float x = (mouse.x / Screen.width - 0.5f) * 2f;
        float y = (mouse.y / Screen.height - 0.5f) * 2f;
        Vector2 offset = new Vector2(x, y);
        offset = Vector2.ClampMagnitude(offset, 1f);

        if (invert) offset = -offset;

        Vector3 target = startPos + new Vector3(offset.x * strength.x, offset.y * strength.y, 0f);

        // Smooth, frame-rate independent movement
        transform.position = Vector3.Lerp(transform.position, target, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
    }
}