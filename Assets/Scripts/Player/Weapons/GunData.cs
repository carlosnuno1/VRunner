using UnityEngine;

[CreateAssetMenu (fileName = "NewGunData", menuName = "Gun/GunData")]
public class GunData : ScriptableObject
{
    public string gunName;

    public LayerMask targetLayerMask;

    [Header("Fire Config")]
    public float shootingRange;
    public float fireRate;

    [Header("ReloadConfig")]
    public float magazineSize;
    public float reloadTime;

}
