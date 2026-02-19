using UnityEngine;

public class IKArmScaling : MonoBehaviour
{
    [Header("Arm Bones")]
    public Transform shoulder;
    public Transform upperArm;
    public Transform forearm;
    public Transform hand;

    [Header("IK Target (Controller)")]
    public Transform handTarget;

    [Header("Stretch Settings")]
    [Range(1.0f, 1.2f)]
    public float maxStretch = 1.12f; 
    public float stretchLerpSpeed = 10f;

    private float originalUpperArmLength;
    private float originalForearmLength;
    private float originalArmLength;
    private float currentStretch = 1f;

    void Start()
    {
        CacheArmLength();
    }

    void CacheArmLength()
    {
        originalUpperArmLength = Vector3.Distance(upperArm.position, forearm.position);
        originalForearmLength = Vector3.Distance(forearm.position, hand.position);
        originalArmLength = originalUpperArmLength + originalForearmLength;
    }

    void LateUpdate()
    {
        if (!upperArm || !forearm || !hand || !handTarget || !shoulder)
            return;

        float targetDistance = Vector3.Distance(shoulder.position, handTarget.position);

        float targetStretch = targetDistance / originalArmLength;
        targetStretch = Mathf.Clamp(targetStretch, 1f, maxStretch);

        currentStretch = Mathf.Lerp(currentStretch, targetStretch, Time.deltaTime * stretchLerpSpeed);

        Vector3 scale = new Vector3(1f, currentStretch, 1f);
        upperArm.localScale = scale;
        forearm.localScale = scale;
    }

    void OnDisable()
    {
        if (upperArm) upperArm.localScale = Vector3.one;
        if (forearm) forearm.localScale = Vector3.one;
    }
}
