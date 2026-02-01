using UnityEngine;
using Unity.XR.CoreUtils;
using System.Collections;
using System.Collections.Generic;

public class CapsuleControllerDriver : MonoBehaviour
{
    public XROrigin m_XROrigin;
    public float m_MinHeight;
    public float m_MaxHeight;
    public CapsuleCollider m_collider;


    // Update is called once per frame
    void Update()
    {
        var height = Mathf.Clamp(m_XROrigin.CameraInOriginSpaceHeight, m_MinHeight, m_MaxHeight);

            Vector3 center = m_XROrigin.CameraInOriginSpacePos;
            center.y = height / 2f + m_collider.radius;

            m_collider.height = height;
            m_collider.center = center;
    }
}
