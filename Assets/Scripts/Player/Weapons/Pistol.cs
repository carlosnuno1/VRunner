using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class Pistol : Gun  
{
    public InputActionReference leftReloadButton;
    public InputActionReference rightReloadButton;
    public float recoilForce = 7;
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    public GameObject holster;
    public GameObject pistol;
    
    private float triggerValue;
    private float reloadButtonValue;
    

    private void Awake()
    {
        leftReloadButton.action.Enable();
        rightReloadButton.action.Enable();
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
        var interactor = grab.firstInteractorSelecting;
        if (interactor == null) return;
        
        var interactorComp = interactor as Component;
        if (interactorComp == null) return;

        GameObject grabbingObject = interactorComp.gameObject;

        if (grabbingObject == null) return;

        if (grabbingObject.CompareTag("Left Hand"))
        {
            reloadButtonValue = leftReloadButton.action.ReadValue<float>();
            Debug.Log("ReloadButton value: " + reloadButtonValue);
        } 
        else if (grabbingObject.CompareTag("Right Hand"))
        {
            reloadButtonValue = rightReloadButton.action.ReadValue<float>();
            Debug.Log("ReloadButton value: " + reloadButtonValue);
        }

        if (reloadButtonValue > 0 && grab.isSelected)
        {
            TryReload();
            Debug.Log(grab.firstInteractorSelecting);
        }
    }


    public override void Shoot()
    {
        RaycastHit hit;
        AddEffect();
        if(Physics.Raycast(gunTipTransform.position, gunTipTransform.forward, out hit, gunData.shootingRange, gunData.targetLayerMask))
        {
            Debug.Log(gunData.gunName + " hit " + hit.collider.name);
        }
    }

    private void AddEffect()
    {
        var interactor = grab.firstInteractorSelecting;
        if (interactor == null)
        {
            Debug.Log("interactor null");
            return;
        }
        
        var interactorComp = interactor as Component;
        if (interactorComp == null)
        {
            Debug.Log("interactor as component null");
            return;
        }

        GameObject grabbingObject = interactorComp.gameObject;

        Transform attachPoint = grabbingObject.transform.Find("AttachPoint");
        if (attachPoint != null)
        {
            Rigidbody recoilPointrb = attachPoint.GetComponent<Rigidbody>();
            Debug.Log("adding recoil force");
            recoilPointrb.AddForce(-transform.forward * recoilForce, ForceMode.Impulse);
            recoilPointrb.transform.localRotation = Quaternion.AngleAxis(-10 * recoilForce, Vector3.right);
        } 
        else 
        {
            Debug.Log("attachpoint of child not found");
            return;
        }

        
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Disconnected:
                leftReloadButton.action.Disable();
                rightReloadButton.action.Disable();
                break;
            case InputDeviceChange Reconnected:
                leftReloadButton.action.Enable();
                rightReloadButton.action.Enable();
                break;
        }
    }

    public void ReturnToHolster()
    {
        pistol.transform.position = holster.transform.position;
    }
}
