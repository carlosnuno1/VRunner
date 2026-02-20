using System.Collections;
using UnityEngine;


public class SpawnIntoSocket : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;
    [SerializeField] private GameObject prefab;

    [Header("Options")]
    [SerializeField] private bool matchSocketPose = true;
    [SerializeField] private bool disablePhysicsBeforeSocket = true;
    [SerializeField] private float waitTime = 2.0f;

    public void SpawnAndInsert()
    {
        StartCoroutine(SpawnAndInsertRoutine());
    }

    private IEnumerator SpawnAndInsertRoutine()
    {
        yield return new WaitForSeconds(waitTime);
        
        if (socket.hasSelection)
        {
            var old = socket.firstInteractableSelected;
            socket.interactionManager.SelectExit(socket, old);
        }

        var go = Instantiate(prefab);
        var interactable = go.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (interactable == null)
        {
            Debug.LogError("Prefab must have an XRBaseInteractable (e.g., XRGrabInteractable).");
            Destroy(go);
            yield break;
        }

        Transform attach = socket.attachTransform != null ? socket.attachTransform : socket.transform;

        if (matchSocketPose)
        {
            go.transform.SetPositionAndRotation(attach.position, attach.rotation);
        }
        else
        {
            go.transform.position = attach.position;
        }

        var rb = go.GetComponent<Rigidbody>();
        if (disablePhysicsBeforeSocket && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        yield return null;

        socket.interactionManager.SelectEnter(socket, (UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable)interactable);
    }
}