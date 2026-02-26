using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int totalEnemiesRequired = 5;
    public int currentEnemiesKilled = 0;
    public float timer = 0f;
    public bool isTimerRunning = false;
    public bool playerInEndZone = false;
    public bool levelComplete = false;

    void Update()
    {
        if (isTimerRunning && !levelComplete)
        {
            timer += Time.deltaTime;
        }
    }

    public void StartLevel()
    {
        if (!isTimerRunning)
        {
            timer = 0f;
            isTimerRunning = true;
            Debug.Log("Started");
        }
    }

    public void EnemyKilled()
    {
        currentEnemiesKilled++;
        CheckCompletion();
    }

    public void SetPlayerInEndZone(bool inside)
    {
        playerInEndZone = inside;
        if (playerInEndZone)
        {
            CheckCompletion();
        }
    }

    private void CheckCompletion()
    {
        if (currentEnemiesKilled >= totalEnemiesRequired && playerInEndZone && !levelComplete)
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        levelComplete = true;
        isTimerRunning = false;
        Debug.Log("Level Complete: " + timer);
    }
}