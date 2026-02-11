using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float mouseSensitivity = 0.15f;
    public Transform cameraTransform;

    private float verticalRotation = 0f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseDelta.x);

        verticalRotation -= mouseDelta.y;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        Vector2 input = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) input.y = 1;
        if (Keyboard.current.sKey.isPressed) input.y = -1;
        if (Keyboard.current.aKey.isPressed) input.x = -1;
        if (Keyboard.current.dKey.isPressed) input.x = 1;

        Vector3 moveDir = transform.right * input.x + transform.forward * input.y;

        Vector3 currentVel = moveDir * moveSpeed;
        currentVel.y = rb.linearVelocity.y;
        rb.linearVelocity = currentVel;
    }
}