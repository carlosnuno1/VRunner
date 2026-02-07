using UnityEngine;

public class TouchDebug : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Color originalColor;
    public Color hoverColor = Color.green; // Changing to green for "Success"

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        originalColor = meshRenderer.material.color;
    }

    // Triggers when ANY collider enters the volume
    private void OnTriggerEnter(Collider other)
    {
        meshRenderer.material.color = hoverColor;
        Debug.Log("TOUCHED BY: " + other.gameObject.name);
    }

    // Triggers when they leave
    private void OnTriggerExit(Collider other)
    {
        meshRenderer.material.color = originalColor;
    }
}