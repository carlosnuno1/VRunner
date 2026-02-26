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
    [SerializeField] private InputActionAsset actions;
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor;
    public bool leftHand;
    
    private InputAction trigger;

    [Header("Web Settings")]
    public float webStrength = 8.5f;
    public float webDamper = 7f;
    public float webMassScale = 4.5f;
    public float webZipStrength = 5f;
    public float maxDistance;
    public LayerMask webLayers;

    [Header("Prediction")]
    public RaycastHit predictionHit;
    public float predictionSphereCastRadius;
    public Transform predictionPoint;

    private SpringJoint joint;
    private FixedJoint endJoint;
    private Vector3 webPoint;
    private float distanceFromPoint;
    private bool webShot;
    private bool isHolding;
    private Vector3 realHitPoint;

    private void Awake()
    {
        if (leftHand)
        {
            trigger = actions.FindAction("Player/Left Trigger", true);
        } else
        {
            trigger = actions.FindAction("Player/Right Trigger", true);
        }
        trigger.Enable();
        InputSystem.onDeviceChange += OnDeviceChange;
        lineRenderer= GetComponent<LineRenderer>();

        webEnd.transform.parent = null;

    }

    private void HandleInput()
    {
        float triggerValue = trigger.ReadValue<float>();
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

    private void CheckForSwingPoints()
    {
        bool isHolding = (interactor as UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor)?.hasSelection ?? false;
        
        if (joint != null) return;

        RaycastHit sphereCastHit;
        Physics.SphereCast(shooterTip.position, predictionSphereCastRadius, shooterTip.forward, out sphereCastHit, maxDistance, webLayers);
        
        RaycastHit raycastHit;
        Physics.Raycast(shooterTip.position, shooterTip.forward, out raycastHit, maxDistance, webLayers);

        if (raycastHit.point != Vector3. zero)
        {
            realHitPoint = raycastHit.point;
        }
        else if (sphereCastHit.point != Vector3.zero)
        {
            realHitPoint = sphereCastHit.point;
        }
        else
        {
            realHitPoint = Vector3.zero;
        }

        if(realHitPoint != Vector3.zero)
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        }
        else
        {
            predictionPoint.gameObject.SetActive(false);
        }

        if (isHolding)
        {
            predictionPoint.gameObject.SetActive(false);
        }
        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
    }

     void ShootWeb()
    {
        if(realHitPoint != Vector3.zero)
        {
            predictionPoint.gameObject.SetActive(false);
            webPoint = predictionHit.point;
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
        CheckForSwingPoints();
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
                trigger.Disable();
                break;
            case InputDeviceChange Reconnected:
                trigger.Enable();
                break;
        }
    }
}
