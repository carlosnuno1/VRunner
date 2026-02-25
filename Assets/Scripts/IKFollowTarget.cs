using UnityEngine;

public class IKFollowTarget : MonoBehaviour
{
    public Transform followTarget;
    public float positionLerp = 30f;
    public float rotationLerp = 30f;

    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, followTarget.position, Time.deltaTime * positionLerp);
        transform.rotation = Quaternion.Slerp(transform.rotation, followTarget.rotation, Time.deltaTime * rotationLerp);
    }
}