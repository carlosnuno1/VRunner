using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f; // Maximum health of the enemy
    public LevelManager LevelManager;
    private float currentHealth; // Current health of the enemy

    void Start()
    {
        // Initialize the health
        currentHealth = maxHealth;
    }

    // Method to deal damage to the enemy
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        // Check if the health falls below 0 and if so, die
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Method to handle enemy death
    private void Die()
    {
        LevelManager.EnemyKilled();
        Debug.Log("Enemy died!");
        // You can add additional logic here, like playing a death animation, disabling the enemy, etc.
        Destroy(gameObject); // Destroys the enemy game object
    }
}