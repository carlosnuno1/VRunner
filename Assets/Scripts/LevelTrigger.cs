using UnityEngine;

public class LevelTrigger : MonoBehaviour
{
    public LevelManager manager;
    public enum TriggerType { Start, End }
    public TriggerType type;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (type == TriggerType.Start) manager.StartLevel();
            else manager.SetPlayerInEndZone(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (type == TriggerType.End) manager.SetPlayerInEndZone(false);
        }
    }
}