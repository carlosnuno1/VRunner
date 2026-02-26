using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class AutomaticGun : MonoBehaviour
{
    public GunData gunData;
    public GameObject gunTip;
    [HideInInspector]public Transform gunTipTransform;

    private float currentAmmo = 0f;
    private float nextTimeToFire = 0f;

    private bool isReloading = false;

    // New boolean to track automatic firing
    private bool stillFiring = false;

    private void Start()
    {
        currentAmmo = gunData.magazineSize;
        gunTipTransform = gunTip.GetComponent<Transform>();
    }

    public virtual void Update()
    {
        // If stillFiring is true, try to shoot continuously
        if (stillFiring)
        {
            TryShoot();
        }
    }

    // Function to start firing (turn on stillFiring)
    public void ActivateFiring()
    {
        stillFiring = true;
    }

    // Function to stop firing (turn off stillFiring)
    public void DeactivateFiring()
    {
        stillFiring = false;
    }

    public void TryReload()
    {
        if (!isReloading && currentAmmo < gunData.magazineSize)
        {
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log(gunData.gunName + " is reloading...");
        yield return new WaitForSeconds(gunData.reloadTime);

        currentAmmo = gunData.magazineSize;
        isReloading = false;

        Debug.Log(gunData.gunName + " is reloaded");
    }

    // Modify TryShoot to allow firing while stillFiring is true
    public void TryShoot()
    {
        if (isReloading)
        {
            Debug.Log(gunData.gunName + " is Reloading");
            return;
        }

        if (currentAmmo <= 0f)
        {
            Debug.Log(gunData.gunName + " has no bullets left, please reload");
            return;
        }

        if (Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + (1 / gunData.fireRate);
            HandleShoot();
        }
    }
    
    private void HandleShoot()
    {
        currentAmmo--;
        Debug.Log(gunData.gunName + " Shot!, Bullets left: " + currentAmmo);
        Shoot();
    }

    public abstract void Shoot();
}