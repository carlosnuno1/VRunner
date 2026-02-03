using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

public class SimpleFPSController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float mouseSensitivity = 0.15f;
    public Transform cameraTransform;

    private float verticalRotation = 0f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Locks the mouse to the center of the game window
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 1. Mouse Look (New Input System)
        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity;

        // Rotate body (Left/Right)
        transform.Rotate(Vector3.up * mouseDelta.x);

        // Rotate camera (Up/Down) with clamping
        verticalRotation -= mouseDelta.y;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        // 2. Movement (New Input System)
        Vector2 input = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) input.y = 1;
        if (Keyboard.current.sKey.isPressed) input.y = -1;
        if (Keyboard.current.aKey.isPressed) input.x = -1;
        if (Keyboard.current.dKey.isPressed) input.x = 1;

        Vector3 moveDir = transform.right * input.x + transform.forward * input.y;

        // Use modern linearVelocity for movement
        Vector3 currentVel = moveDir * moveSpeed;
        currentVel.y = rb.linearVelocity.y; // Preserve gravity
        rb.linearVelocity = currentVel;
    }
}