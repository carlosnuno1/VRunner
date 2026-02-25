using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Climb : MonoBehaviour
{
    public Rigidbody playerRigidbody;                  // Reference to the player's Rigidbody
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor leftHandInteractor;      // Reference to the left hand interactor
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor rightHandInteractor;     // Reference to the right hand interactor
    public float throwStrengthScalar = 2f;             // Scalar factor for throw strength
    public float smoothClimbSpeed = 2f;                // Speed for smooth climbing

    private bool isClimbing = false;                   // Flag to track if the player is climbing
    private Vector3 previousLeftHandPos;               // Previous position of the left hand
    private Vector3 previousRightHandPos;              // Previous position of the right hand

    private Vector3 leftHandVelocity;                  // Left hand velocity for scaling throw strength
    private Vector3 rightHandVelocity;                 // Right hand velocity for scaling throw strength

    // Called when the interactor selects an object (hand grabs an object)
    public void OnSelectEnter(SelectEnterEventArgs args)
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable = args.interactableObject as UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable; // Explicit cast to XRBaseInteractable
        if (interactable != null && interactable.CompareTag("Climable")) // Ensure climbable surfaces are tagged correctly
        {
            isClimbing = true;
            previousLeftHandPos = leftHandInteractor.transform.position;
            previousRightHandPos = rightHandInteractor.transform.position;
            Debug.Log("Started Climbing");
        }
    }

    // Called when the interactor exits an object (hand releases the object)
    public void OnSelectExit(SelectExitEventArgs args)
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable = args.interactableObject as UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable; // Explicit cast to XRBaseInteractable
        if (interactable != null && interactable.CompareTag("Climable"))
        {
            // Apply an upward force based on the velocity of the hand when releasing the object
            float leftHandThrowStrength = leftHandVelocity.y * throwStrengthScalar; // Scaling by hand velocity
            float rightHandThrowStrength = rightHandVelocity.y * throwStrengthScalar; // Scaling by hand velocity

            // Average the throw strengths from both hands to decide the final throw strength
            float throwStrength = (leftHandThrowStrength + rightHandThrowStrength) / 2;

            // Apply upward force when releasing the climbable object
            playerRigidbody.linearVelocity = new Vector3(playerRigidbody.linearVelocity.x, throwStrength, playerRigidbody.linearVelocity.z);
            isClimbing = false;
            Debug.Log("Stopped Climbing");
        }
    }

    void Update()
    {
        if (isClimbing)
        {
            // Capture the velocity of both hands (difference in position)
            leftHandVelocity = (leftHandInteractor.transform.position - previousLeftHandPos) / Time.deltaTime;
            rightHandVelocity = (rightHandInteractor.transform.position - previousRightHandPos) / Time.deltaTime;

            // Transfer the hand velocity to the player
            Vector3 averageVelocity = (leftHandVelocity + rightHandVelocity) / 2;
            playerRigidbody.linearVelocity = new Vector3(averageVelocity.x, playerRigidbody.linearVelocity.y, averageVelocity.z); // Preserve Y-axis velocity for gravity

            // Update the previous hand positions for the next frame
            previousLeftHandPos = leftHandInteractor.transform.position;
            previousRightHandPos = rightHandInteractor.transform.position;
        }
    }

    // Attach the events to the interactors in the inspector or programmatically
    void OnEnable()
    {
        leftHandInteractor.selectEntered.AddListener(OnSelectEnter); // Using the new event system
        leftHandInteractor.selectExited.AddListener(OnSelectExit);  // Using the new event system

        rightHandInteractor.selectEntered.AddListener(OnSelectEnter); // Using the new event system
        rightHandInteractor.selectExited.AddListener(OnSelectExit);  // Using the new event system
    }

    void OnDisable()
    {
        leftHandInteractor.selectEntered.RemoveListener(OnSelectEnter); // Using the new event system
        leftHandInteractor.selectExited.RemoveListener(OnSelectExit);  // Using the new event system

        rightHandInteractor.selectEntered.RemoveListener(OnSelectEnter); // Using the new event system
        rightHandInteractor.selectExited.RemoveListener(OnSelectExit);  // Using the new event system
    }
}