using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class Grapple : MonoBehaviour
{
    [Header("Input")]
    [Header("References")]
    public LineRenderer lr;
    public Transform gunTip, hand, player;
    public LayerMask whatIsGrappleable;
    [Header("Swinging")]
    private Vector3 currentGrapplePosition;
    private float maxSwingDistance = 25f;
    private Vector3 swingPoint;
    private SpringJoint joint;
    public InputActionReference grappleTrigger;
    private void StartSwing()
    {
        RaycastHit hit;
        if (Physics.Raycast(hand.position, hand.forward, out hit, maxSwingDistance, whatIsGrappleable))
        {
            swingPoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = swingPoint;

            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

            joint.maxDistance = distanceFromPoint * .8f;
            joint.minDistance = distanceFromPoint * .25f;

            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;
        }
    }

    void StopSwing()
    {
        lr.positionCount = 0;
        Destroy(joint);
    }

    void Update()
    {
        if (grappleTrigger) {
            StartSwing();
            Debug.Log("bruh");
        }
        if (grappleTrigger) StopSwing();
    }

    void LateUpdate()
    {
        DrawRope();   
    }

    void DrawRope()
    {
        if (!joint) return;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, swingPoint);
    }
}
