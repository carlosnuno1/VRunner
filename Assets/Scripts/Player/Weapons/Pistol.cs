using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class Pistol : Gun  
{
    public InputActionReference fireButton;
    public InputActionReference reloadButton;
    private float triggerValue;
    private float reloadButtonValue;
    // public InputActionReference reloadButton;

    private void Awake()
    {
        fireButton.action.Enable();
        reloadButton.action.Enable();
        InputSystem.onDeviceChange += OnDeviceChange;
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    public override void Update()
    {
        base.Update();

        HandleInput();
    }

    private void HandleInput()
    {
        float triggerValue = fireButton.action.ReadValue<float>();
        Debug.Log("Trigger value: " + triggerValue);

        if (triggerValue > 0)
        {
            TryShoot();
        }

        reloadButtonValue = reloadButton.action.ReadValue<float>();
        Debug.Log("ReloadButton value: " + reloadButtonValue);

        if (reloadButtonValue > 0)
        {
            TryReload();
        }
    }


    public override void Shoot()
    {
        RaycastHit hit;

        if(Physics.Raycast(gunTipTransform.position, gunTipTransform.forward, out hit, gunData.shootingRange, gunData.targetLayerMask))
        {
            Debug.Log(gunData.gunName + " hit " + hit.collider.name);
        }
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Disconnected:
                reloadButton.action.Disable();
                fireButton.action.Disable();
                break;
            case InputDeviceChange Reconnected:
                fireButton.action.Enable();
                reloadButton.action.Enable();
                break;
        }
    }
}
