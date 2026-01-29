using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class WebShooter : MonoBehaviour
{
    [Header("Core Variables")]
    public Transform shooterTip;
    public Rigidbody player;
    public GameObject webEnd;
    public LineRenderer lineRenderer;
    public InputActionReference isPressed;
    

    [Header("Web Settings")]
    public float webStrength = 8.5f;
    public float webDamper = 7f;
    public float webMassScale = 4.5f;
    public float webZipStrength = 5f;

    private SpringJoint joint;
    private FixedJoint endJoint;
    private Vector3 webPoint;
    private float distanceFromPoint;

    private void Awake()
    {
        isPressed.action.Enable();
        isPressed.action.performed += HandleInput;
        InputSystem.onDeviceChange += OnDeviceChange;
        lineRenderer= GetComponent<LineRenderer>();

    }

    private void HandleInput(InputAction.CallbackContext context)
    {
        print(isPressed);
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Disconnected:
                isPressed.action.Disable();
                isPressed.action.performed -=HandleInput;
                break;
            case InputDeviceChange Reconnected:
                isPressed.action.Enable();
                isPressed.action.performed += HandleInput;
                break;
        }
    }
}
