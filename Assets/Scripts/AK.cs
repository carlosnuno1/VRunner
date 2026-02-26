using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class AK : AutomaticGun  
{
    // public InputActionReference leftReloadButton;
    // public InputActionReference rightReloadButton;
    [SerializeField] private InputActionAsset actions;
    public float recoilForce = 7;
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    public GameObject holster;
    public GameObject Self;
    public float damage = 10f;
    public AudioSource akAudioSource;
    public AudioClip akSound;
    
    private float triggerValue;
    private float reloadButtonValue;

    private InputAction leftReloadButton;
    private InputAction rightReloadButton;

    // [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private ParticleSystem ImpactParticleSystem;
    [SerializeField] private TrailRenderer BulletTrail;
    

    private void Awake()
    {
        leftReloadButton = actions.FindAction("Player/Left Reload", true);
        rightReloadButton = actions.FindAction("Player/Right Reload", true);
        leftReloadButton.Enable();
        rightReloadButton.Enable();
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
            reloadButtonValue = leftReloadButton.ReadValue<float>();
            Debug.Log("ReloadButton value: " + reloadButtonValue);
        } 
        else if (grabbingObject.CompareTag("Right Hand"))
        {
            reloadButtonValue = rightReloadButton.ReadValue<float>();
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
        akAudioSource.PlayOneShot(akSound);
        RaycastHit hit;
        AddEffect();
        if(Physics.Raycast(gunTipTransform.position, gunTipTransform.forward, out hit, gunData.shootingRange, gunData.targetLayerMask))
        {
            TrailRenderer trail = Instantiate(BulletTrail, gunTipTransform.position, Quaternion.identity);
            // ParticleSystem currentMuzzleFlash = Instantiate(muzzleFlash, gunTipTransform.position, Quaternion.identity);
            // Destroy(currentMuzzleFlash, .5f);
            StartCoroutine(SpawnTrail(trail, hit));
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                // Deal damage to the enemy
                enemy.TakeDamage(damage);
                Debug.Log("Enemy hit for " + damage + " damage.");
            }        
        }
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, RaycastHit Hit)
    {
        float time = 0;
        Vector3 startPosition = Trail.transform.position;

        while (time < 1)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, Hit.point, time);
            time += Time.deltaTime / Trail.time;

            yield return null;
        }
        Trail.transform.position = Hit.point;
        Instantiate(ImpactParticleSystem, Hit.point, Quaternion.LookRotation(Hit.normal));
        Destroy(Trail.gameObject,Trail.time);
    }

    private void AddEffect()
    {
        var interactable = grab as UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable;

        if (interactable != null && interactable.interactorsSelecting.Count > 0)
        {
            // First hand recoil
            var interactor1 = interactable.interactorsSelecting[0];
            Transform attachPoint1 = interactor1.transform.Find("AttachPoint");
            Rigidbody recoilPointrb1 = attachPoint1?.GetComponent<Rigidbody>();
            if (recoilPointrb1 != null)
            {
                // Apply recoil force to first hand
                recoilPointrb1.AddForce(-transform.forward * recoilForce, ForceMode.Impulse);
                recoilPointrb1.transform.localRotation = Quaternion.AngleAxis(-10 * recoilForce, Vector3.right);
            }

            if (interactable.interactorsSelecting.Count > 1)
            {
                // Second hand recoil (possibly with a smaller force or counteracting torque)
                var interactor2 = interactable.interactorsSelecting[1];
                Transform attachPoint2 = interactor2.transform.Find("AttachPoint");
                Rigidbody recoilPointrb2 = attachPoint2?.GetComponent<Rigidbody>();
                if (recoilPointrb2 != null)
                {
                    // Apply a smaller recoil force or counteracting torque to prevent flipping
                    recoilPointrb2.AddForce(-transform.forward * recoilForce * 0.5f, ForceMode.Impulse);
                    recoilPointrb2.transform.localRotation = Quaternion.AngleAxis(-5 * recoilForce, Vector3.right);
                }
            }
        }
    }
    

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Disconnected:
                leftReloadButton.Disable();
                rightReloadButton.Disable();
                break;
            case InputDeviceChange Reconnected:
                leftReloadButton.Enable();
                rightReloadButton.Enable();
                break;
        }
    }

    public void ReturnToHolster()
    {
        Self.transform.position = holster.transform.position;
    }

    public void DestroyAK()
    {
        StartCoroutine(DestroySelf());
    }

    private IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(2.0f);

        var interactable = grab as UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable;

        if (interactable == null || interactable.interactorsSelecting.Count <= 0)
        {
            Destroy(Self);
            Debug.Log("Destorying ak");
        }
    }
}
