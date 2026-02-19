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
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor;
    

    [Header("Web Settings")]
    public float webStrength = 8.5f;
    public float webDamper = 7f;
    public float webMassScale = 4.5f;
    public float webZipStrength = 5f;
    public float maxDistance;
    public LayerMask webLayers;

    private SpringJoint joint;
    private FixedJoint endJoint;
    private Vector3 webPoint;
    private float distanceFromPoint;
    private bool webShot;
    private bool isHolding;

    private void Awake()
    {
        isPressed.action.Enable();
        InputSystem.onDeviceChange += OnDeviceChange;
        lineRenderer= GetComponent<LineRenderer>();

        webEnd.transform.parent = null;

    }

    private void HandleInput()
    {
        float triggerValue = isPressed.action.ReadValue<float>();
        bool isHolding = (interactor as UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor)?.hasSelection ?? false;
        Debug.Log("Trigger value: " + triggerValue);

        if (triggerValue > 0 && !webShot && !isHolding)
        {
            webShot = true;
            ShootWeb();
        }
        else if (triggerValue == 0 && webShot || isHolding)
        {
            webShot = false;
            StopWeb();
        }
    }

    private void ShootWeb()
    {
        RaycastHit hit;
        if(Physics.Raycast(shooterTip.position, shooterTip.forward, out hit, maxDistance, webLayers))
        {
            webPoint = hit.point;
            webEnd.transform.position = webPoint;

            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = webPoint;

            distanceFromPoint = Vector3.Distance(player.transform.position, webPoint) * .75f;
            joint.minDistance = 0;
            joint.maxDistance = distanceFromPoint;

            joint.spring = webStrength;
            joint.damper = webDamper;
            joint.massScale = webMassScale;
        }
    }

    private void StopWeb()
    {
        Destroy(joint);

        if (endJoint) Destroy(endJoint);
        lineRenderer.positionCount = 0;
        
    }

    private void Update()
    {
        HandleInput();
        if (webShot && joint)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, shooterTip.position);
            lineRenderer.SetPosition(1, webEnd.transform.position);
        }
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Disconnected:
                isPressed.action.Disable();
                break;
            case InputDeviceChange Reconnected:
                isPressed.action.Enable();
                break;
        }
    }
}
