using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The target the camera should follow (usually the player)
    public float smoothSpeed = 0.125f; // Speed of camera following
    public Vector3 offset; // Offset between the camera and the target (e.g., behind the player)

    void LateUpdate()
    {
        // Calculate desired position based on target's position and the offset
        Vector3 desiredPosition = target.position + offset;
        
        // Smoothly interpolate between current position and desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // Update the camera position
        transform.position = smoothedPosition;

        // Optionally, make the camera always look at the target
        transform.LookAt(target);
    }
}